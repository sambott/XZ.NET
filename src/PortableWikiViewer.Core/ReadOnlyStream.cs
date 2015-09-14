using System;
using System.IO;

namespace PortableWikiViewer.Core
{
    public abstract class ReadOnlyStream : Stream
    {
        public Stream BaseStream { get; private set; }

        public long StreamStartPosition { get; set; }

        public override bool CanRead => BaseStream.CanRead && !_endOfStream;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                return BaseStream.Position;
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        bool _endOfStream;

        public ReadOnlyStream(Stream stream)
        {
            BaseStream = stream;
            if (!BaseStream.CanRead)
                throw new InvalidDataException("Must be able to read from stream");
            StreamStartPosition = BaseStream.Position;
    }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
