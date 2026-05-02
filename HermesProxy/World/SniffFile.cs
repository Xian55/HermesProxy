using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Framework.Logging;

namespace HermesProxy.World;

public sealed class SniffFile
{
    // Monotonic counter suffixed to filenames so multiple captures within a single proxy
    // process (e.g. realm-switch, reconnect) get distinct paths even though they share the
    // process-wide StartupStamp.
    private static int _sessionCounter = 0;

    // 64 KB FileStream buffer — packet logging is bursty sequential writes; the default 4 KB
    // buffer means more frequent syscalls. Larger buffer reduces kernel transitions.
    private const int FileBufferSize = 64 * 1024;

    public readonly string FilePath;

    public SniffFile(string fileName, ushort build)
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
        _fileWriter = new BinaryWriter(stream);
        _gameVersion = build;
    }
    BinaryWriter _fileWriter;
    ushort _gameVersion;
    readonly Lock _lock = new();

    public void WriteHeader()
    {
        _fileWriter.Write('P');
        _fileWriter.Write('K');
        _fileWriter.Write('T');
        UInt16 sniffVersion = 0x201;
        _fileWriter.Write(sniffVersion);
        _fileWriter.Write(_gameVersion);

        for (int i = 0; i < 40; i++)
        {
            byte zero = 0;
            _fileWriter.Write(zero);
        }
    }

    public void WritePacket(uint opcode, bool isFromClient, ReadOnlySpan<byte> data)
    {
        lock (_lock)
        {
            byte direction = !isFromClient ? (byte)0xff : (byte)0x0;
            _fileWriter.Write(direction);

            uint unixtime = (uint)Time.UnixTime;
            _fileWriter.Write(unixtime);
            _fileWriter.Write(Environment.TickCount);

            if (isFromClient)
            {
                uint packetSize = (uint)(data.Length - 2 + sizeof(uint));
                _fileWriter.Write(packetSize);
                _fileWriter.Write(opcode);

                // Skip the 2-byte opcode prefix; single bulk write of the payload.
                _fileWriter.Write(data[2..]);
            }
            else
            {
                uint packetSize = (uint)data.Length + sizeof(ushort);
                _fileWriter.Write(packetSize);
                ushort opcode2 = (ushort)opcode;
                _fileWriter.Write(opcode2);
                _fileWriter.Write(data);
            }

            // Flush so that the .pkt is parseable mid-session — e.g. when a test
            // harness uses Stop-Process -Force on the proxy (no graceful Dispose),
            // the 64 KB FileStream buffer would otherwise keep recent packets
            // in-process and the on-disk file would appear empty/short.
            _fileWriter.Flush();
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
            _fileWriter.Flush();
            _fileWriter.Close();
        }
    }

    private bool _closed;
}
