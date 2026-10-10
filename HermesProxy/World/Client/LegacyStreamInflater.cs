using System;
using System.IO;
using System.IO.Compression;

namespace HermesProxy.World.Client;

/// <summary>
/// Inflates the compressed packets of a 4.x legacy server.
/// </summary>
/// <remarks>
/// TrinityCore 4.3.4 runs one zlib stream for the whole connection and sync-flushes it after each
/// packet, so a packet carries only the next slice of that stream: inflating each one on its own
/// fails from the second packet on. Input the inflater has not consumed yet (the sync-flush marker
/// can trail the output it ends) stays queued for the next packet.
/// </remarks>
internal sealed class LegacyStreamInflater : IDisposable
{
    private readonly FeedStream _input = new();
    private readonly ZLibStream _zlib;

    public LegacyStreamInflater() => _zlib = new ZLibStream(_input, CompressionMode.Decompress, leaveOpen: true);

    /// <summary>Feeds one packet's compressed bytes and fills <paramref name="destination"/> from the stream.</summary>
    public bool TryInflate(ReadOnlySpan<byte> compressed, Span<byte> destination)
    {
        _input.Append(compressed);
        try
        {
            _zlib.ReadExactly(destination);
            return true;
        }
        catch (Exception e) when (e is EndOfStreamException or InvalidDataException)
        {
            return false;
        }
    }

    public void Dispose()
    {
        _zlib.Dispose();
        _input.Dispose();
    }

    /// <summary>A read-only stream over the compressed bytes received so far and not yet inflated.</summary>
    private sealed class FeedStream : Stream
    {
        private byte[] _buffer = new byte[4096];
        private int _start;
        private int _end;

        public void Append(ReadOnlySpan<byte> data)
        {
            int pending = _end - _start;
            if (_buffer.Length - _end < data.Length)
            {
                byte[] target = pending + data.Length > _buffer.Length
                    ? new byte[Math.Max(_buffer.Length * 2, pending + data.Length)]
                    : _buffer;
                _buffer.AsSpan(_start, pending).CopyTo(target);
                _buffer = target;
                _start = 0;
                _end = pending;
            }

            data.CopyTo(_buffer.AsSpan(_end));
            _end += data.Length;
        }

        public override int Read(Span<byte> destination)
        {
            int count = Math.Min(destination.Length, _end - _start);
            _buffer.AsSpan(_start, count).CopyTo(destination);
            _start += count;
            return count;
        }

        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
