using System;
using System.IO;
using System.Linq;

namespace Cmf.CLI.Core.Objects;

public class ConcatStreams : Stream
{
    private readonly bool leaveOpen;

    private readonly Stream[] sources;

    /// <summary>
    /// The "global" position inside this stream (sum of all the positions of the streams we have read until now)
    /// </summary>
    private long position;

    /// <summary>
    /// The index of the current source stream we are reading. If it is greater than the sources length, it means
    /// we have no more source streams to read
    /// </summary>
    private long currentSource = 0;

    public ConcatStreams(Stream[] sources, bool leaveOpen = false)
    {
        this.sources = sources;
        this.leaveOpen = leaveOpen;
    }

    public override bool CanRead => true;

    public override bool CanSeek => sources.All(s => s.CanSeek);

    public override bool CanWrite => false;

    public override long Length => sources.Sum(s => s.Length);

    public override long Position
    {
        get => position;
        set
        {
            Seek(value, SeekOrigin.Begin);
        }
    }

    public override void Flush()
    {
        foreach (var stream in sources)
        {
            stream.Flush();
        }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = 0;

        while (currentSource < sources.Length && offset + bytesRead < buffer.Length)
        {
            // Number of bytes we can still fit into the buffer while respecting
            // the requested byte counts
            var maxBytes = count - bytesRead;

            int sourceBytesRead = sources[currentSource].Read(buffer, offset + bytesRead, maxBytes);

            if (sourceBytesRead == 0)
            {
                currentSource += 1;
                continue;
            }

            // Advance the position
            position += sourceBytesRead;
            bytesRead += sourceBytesRead;
        }

        return bytesRead;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        if (origin == SeekOrigin.Begin || origin == SeekOrigin.Current)
        {
            if (origin == SeekOrigin.Current)
            {
                offset = position + offset;
            }

            position = 0;
            currentSource = 0;

            while (currentSource < sources.Length)
            {
                var sourceLen = sources[currentSource].Length;
                var sourceOffset = Math.Min(sourceLen, offset - position);

                sources[currentSource].Seek(sourceOffset, SeekOrigin.Begin);
                position += sourceOffset;

                // If we seek to the end of the current stream, and it still
                // wasn't enough, we need to seek more on the next stream
                if (sourceOffset == sourceLen)
                {
                    currentSource++;
                }
                else
                {
                    break;
                }
            }

            return position;
        }
        else
        {
            throw new NotImplementedException($"Not yet implemented ConcatStreams.Seek() from {origin} origin");
        }
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException("The method is not supported.");
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new System.NotImplementedException("The method is not supported.");
    }
    
    
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                if (!leaveOpen)
                {
                    foreach (var subStream in sources)
                    {
                        subStream.Dispose();
                    }
                }
            }
        }
        finally
        {
            base.Dispose(disposing);
        }
    }
}