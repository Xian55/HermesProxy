using System;
using System.IO;
using System.Threading;
using Framework.Logging;

namespace HermesProxy.World;

/// <summary>
/// A per-session .pkt capture that WowPacketParser reads.
/// </summary>
/// <remarks>
/// Two formats, chosen once when the capture opens. PKT 2.1 stores a server opcode in 2 bytes:
/// every legacy stream and every modern build before 4.4.0. A modern stream whose opcodes carry a
/// group (4.4.0 on) is written as PKT 3.1, the format TrinityCore's own packet log uses, which
/// stores the opcode in 4 bytes in both directions.
/// </remarks>
public abstract class SniffFile
{
    // Monotonic counter suffixed to filenames so multiple captures within a single proxy
    // process (e.g. realm-switch, reconnect) get distinct paths even though they share the
    // process-wide StartupStamp.
    private static int _sessionCounter = 0;

    // 64 KB FileStream buffer — packet logging is bursty sequential writes; the default 4 KB
    // buffer means more frequent syscalls. Larger buffer reduces kernel transitions.
    private const int FileBufferSize = 64 * 1024;

    public readonly string FilePath;

    private SniffFile(string fileName, uint build)
    {
        string dir = "PacketsLog";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        int seq = Interlocked.Increment(ref _sessionCounter);
        // Filename embeds Log.StartupStamp (yyyyMMdd_HHmmss) so the .pkt shares its token
        // with hermes-<StartupStamp>.log — enables exact-match correlation between the
        // text log and the binary capture instead of fuzzy unix-time proximity.
        string file = fileName + "_" + build + "_" + Log.StartupStamp + "_" + seq + ".pkt";
        string path = Path.Combine(dir, file);

        this.FilePath = path;

        var stream = new FileStream(
            path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            FileBufferSize,
            FileOptions.SequentialScan);
        Writer = new BinaryWriter(stream);
    }
    private protected readonly BinaryWriter Writer;
    readonly Lock _lock = new();
    bool _closed;

    // Serialises the open-and-header sequence across every thread that can start a
    // capture. Two hazards it closes, both of which corrupted the .pkt ordering:
    //
    //  * The field was published before WriteHeader ran, so a concurrent WritePacket
    //    on the other socket could land a packet ahead of the PKT header.
    //  * Two threads could each build a SniffFile. _sessionCounter gives them distinct
    //    filenames, the last assignment wins, and everything written to the loser is
    //    stranded in an orphan capture.
    //
    // Contended only for the first packet of a session; every call after that returns
    // on the volatile fast-path read below without touching this lock.
    private static readonly Lock _openLock = new();

    /// <summary>
    /// Returns the capture referenced by <paramref name="sniffFile"/>, creating it and
    /// writing its PKT header first if it is not open yet. The field is assigned only
    /// after the header is on disk, so a reader that observes a non-null reference is
    /// guaranteed to be appending after a complete header.
    /// </summary>
    /// <param name="opcodeSize">Bytes a server opcode takes on this stream's wire. Above 2 the
    /// capture is PKT 3.1, because PKT 2.1 has no room for the rest.</param>
    public static SniffFile EnsureOpen(ref SniffFile sniffFile, string fileName, uint build, int opcodeSize)
    {
        var existing = Volatile.Read(ref sniffFile);
        if (existing != null)
            return existing;

        lock (_openLock)
        {
            existing = sniffFile;
            if (existing != null)
                return existing;

            SniffFile created = opcodeSize == sizeof(ushort)
                ? new Pkt21(fileName, build)
                : new Pkt31(fileName, build);
            created.WriteHeader();

            // Publish last. Everything above is invisible to other threads until this
            // write lands, which is what makes the header-before-packets order hold.
            Volatile.Write(ref sniffFile, created);

            // Once per capture, so the interpolation here is not on any hot path.
            Log.Print(LogType.Trace, $"Opened {fileName} sniff file: {created.FilePath}");
            return created;
        }
    }

    private protected abstract void WriteHeader();

    private protected abstract void WriteRecord(uint opcode, bool isFromClient, ReadOnlySpan<byte> payload);

    /// <param name="payload">The packet body after its opcode.</param>
    public void WritePacket(uint opcode, bool isFromClient, ReadOnlySpan<byte> payload)
    {
        lock (_lock)
        {
            if (_closed)
                return;

            WriteRecord(opcode, isFromClient, payload);

            // Flush so that the .pkt is parseable mid-session — e.g. when a test
            // harness uses Stop-Process -Force on the proxy (no graceful Dispose),
            // the 64 KB FileStream buffer would otherwise keep recent packets
            // in-process and the on-disk file would appear empty/short.
            Writer.Flush();
        }
    }

    public void CloseFile()
    {
        // Idempotent + thread-safe. Both modern sockets (Realm + Instance) hit
        // `CMSG_LOG_DISCONNECT` independently when the V3_4_3 client tears down,
        // so this can be called twice in rapid succession from different threads.
        // The lock serialises access; the _closed flag stops a second close-call
        // from invoking Flush/Close on an already-disposed BinaryWriter (which
        // previously threw ObjectDisposedException in the proxy's session-cleanup
        // path — observed on every disconnect with reason=7).
        lock (_lock)
        {
            if (_closed)
                return;
            _closed = true;
            Writer.Flush();
            Writer.Close();
        }
    }

    /// <summary>
    /// "PKT", version 0x201, u16 build, 40-byte session key. A record is u8 direction, u32 unix
    /// time, i32 tick count, u32 size, then a u32 opcode from the client or a u16 opcode from the
    /// server, then the payload.
    /// </summary>
    private sealed class Pkt21 : SniffFile
    {
        private readonly ushort _build;

        public Pkt21(string fileName, uint build) : base(fileName, build) => _build = (ushort)build;

        private protected override void WriteHeader()
        {
            Writer.Write('P');
            Writer.Write('K');
            Writer.Write('T');
            Writer.Write((ushort)0x201);
            Writer.Write(_build);

            Span<byte> sessionKey = stackalloc byte[40];
            Writer.Write(sessionKey);
        }

        private protected override void WriteRecord(uint opcode, bool isFromClient, ReadOnlySpan<byte> payload)
        {
            Writer.Write(isFromClient ? (byte)0x00 : (byte)0xff);
            Writer.Write((uint)Time.UnixTime);
            Writer.Write(Environment.TickCount);

            if (isFromClient)
            {
                Writer.Write((uint)(payload.Length + sizeof(uint)));
                Writer.Write(opcode);
            }
            else
            {
                Writer.Write((uint)(payload.Length + sizeof(ushort)));
                Writer.Write((ushort)opcode);
            }

            Writer.Write(payload);
        }
    }

    /// <summary>
    /// TrinityCore's <c>PacketLog</c> layout: "PKT", version 0x301, sniffer id, u32 build, locale,
    /// 40-byte session key, start unix time, start tick count, optional-data size. A record is the
    /// direction ("SMSG"/"CMSG"), connection id, tick count, optional-data size, length, then a u32
    /// opcode and the payload.
    /// </summary>
    private sealed class Pkt31 : SniffFile
    {
        // WowPacketParser special-cases a few sniffer ids ('T' is TrinityCore's, which promises a
        // socket address in every record); this one only has to be none of them.
        private const byte SnifferId = (byte)'H';
        private const uint ServerToClient = 0x47534D53;
        private const uint ClientToServer = 0x47534D43;

        private readonly uint _build;

        public Pkt31(string fileName, uint build) : base(fileName, build) => _build = build;

        private protected override void WriteHeader()
        {
            Writer.Write("PKT"u8);
            Writer.Write((ushort)0x301);
            Writer.Write(SnifferId);
            Writer.Write(_build);
            Writer.Write("enUS"u8);

            Span<byte> sessionKey = stackalloc byte[40];
            Writer.Write(sessionKey);

            // A record's time is the start time plus its tick count minus this one.
            Writer.Write((uint)Time.UnixTime);
            Writer.Write((uint)Environment.TickCount);
            Writer.Write(0);
        }

        private protected override void WriteRecord(uint opcode, bool isFromClient, ReadOnlySpan<byte> payload)
        {
            Writer.Write(isFromClient ? ClientToServer : ServerToClient);
            Writer.Write(0);
            Writer.Write((uint)Environment.TickCount);
            Writer.Write(0);
            Writer.Write(payload.Length + sizeof(uint));
            Writer.Write(opcode);
            Writer.Write(payload);
        }
    }
}
