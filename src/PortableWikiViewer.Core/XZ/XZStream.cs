using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PortableWikiViewer.Core.XZ.Filters;

namespace PortableWikiViewer.Core.XZ
{
    public enum CheckType : byte
    {
        NONE =   0x00,
        CRC32 =  0x01,
        CRC64 =  0x04,
        SHA256 = 0x0A
    }

    public sealed class XZStream : Stream
    {
        private void AssertBlockCheckTypeIsSupported()
        {
            switch (Header.BlockCheckType)
            {
                case CheckType.NONE:
                    break;
                case CheckType.CRC32:
                    break;
                case CheckType.CRC64:
                    break;
                case CheckType.SHA256:
                    throw new NotImplementedException();
                default:
                    throw new NotSupportedException("Check Type unknown to this version of decoder.");
            }
        }

        public Stream BaseStream { get; private set; }
        public XZHeader Header { get; private set; }

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

        public XZStream(Stream stream)
        {
            BaseStream = stream;
            if (!BaseStream.CanRead)
                throw new InvalidDataException("Must be able to read from stream");
            Header = XZHeader.FromStream(BaseStream);
            AssertBlockCheckTypeIsSupported();
        }

        private void ProcessBlocks()
        {
            for(;;)
            {
                try
                {

                }
                catch (XZIndexMarkerReachedException)
                {
                    break;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            //if (_endOfStream)
                throw new EndOfStreamException();
           /* try
            {
                return BaseStream.Read(buffer, offset, count);
            }
            catch (EndOfStreamException)
            {
                _endOfStream = true;
            }*/
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
