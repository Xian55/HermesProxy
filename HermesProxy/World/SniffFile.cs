using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HermesProxy.World;

public sealed class SniffFile
{
    // Monotonic counter suffixed to filenames so simultaneous sessions can't collide on the
    // one-second-granular Unix timestamp (two logins in the same second previously raced for
    // the same path).
    private static int _sessionCounter = 0;

    // 64 KB FileStream buffer — packet logging is bursty sequential writes; the default 4 KB
    // buffer means more frequent syscalls. Larger buffer reduces kernel transitions.
    private const int FileBufferSize = 64 * 1024;

    public SniffFile(string fileName, ushort build)
    {
        string dir = "PacketsLog";
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        int seq = Interlocked.Increment(ref _sessionCounter);
        string file = fileName + "_" + build + "_" + Time.UnixTime + "_" + seq + ".pkt";
        string path = Path.Combine(dir, file);

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
        }
    }

    public void CloseFile()
    {
        // Guard against races with an in-flight WritePacket on another thread — session teardown
        // can run concurrently with the last packets being flushed.
        // Flush() is redundant here (BinaryWriter.Close -> FileStream.Dispose flushes the 64 KB
        // buffer before closing the handle) but makes the intent explicit, and keeps the invariant
        // that "CloseFile returned cleanly => all previously-written bytes are in the OS page cache".
        lock (_lock)
        {
            _fileWriter.Flush();
            _fileWriter.Close();
        }
    }
}
