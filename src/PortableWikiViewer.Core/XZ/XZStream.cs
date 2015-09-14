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

    public sealed class XZStream : XZReadOnlyStream
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
        public XZHeader Header { get; private set; }
        public bool HeaderIsRead { get; private set; }

        bool _endOfStream;

        public XZStream(Stream stream) : base(stream)
        {
        }

        private void ReadHeader()
        {
            Header = XZHeader.FromStream(BaseStream);
            AssertBlockCheckTypeIsSupported();
            HeaderIsRead = true;
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
            int bytesRead = 0;
            if (_endOfStream)
                throw new EndOfStreamException();
            try
            {
                if (!HeaderIsRead)
                    ReadHeader();
                bytesRead = BaseStream.Read(buffer, offset, count);//TODO
            }
            catch (EndOfStreamException)
            {
                _endOfStream = true;
            }
            return bytesRead;
        }
    }
}
