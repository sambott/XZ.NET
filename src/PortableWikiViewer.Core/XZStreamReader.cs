using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortableWikiViewer.Core
{
    public class XZStreamReader
    {
        public enum CheckType : byte
        {
            NONE =   0x00,
            CRC32 =  0x01,
            CRC64 =  0x04,
            SHA256 = 0x0A
        }

        private readonly BinaryReader _reader;
        private byte[] MagicHeader = new byte[] { 0xFD, 0x37, 0x7A, 0x58, 0x5a, 0x00 };
        private CheckType _checkType;
        private int _checkSize { get { return ((((int)_checkType) + 2) / 3) * 4; } }

        public XZStreamReader(Stream stream)
        {
            _reader = new BinaryReader(stream, Encoding.UTF8, true);
            ProcessHeader();
        }

        private void ProcessHeader()
        {
            CheckMagicBytes(_reader.ReadBytes(6));
            ProcessStreamFlags(_reader.ReadBytes(2));
            CheckCheckIsSupported();
        }

        private void CheckCheckIsSupported()
        {
            switch (_checkType)
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

        private void ProcessStreamFlags(byte[] streamFlags)
        {
            byte typeOfCheck = (byte)(streamFlags[1] & 0x0F);
            byte futureUse = (byte)(streamFlags[1] & 0xF0);
            if (futureUse != 0 || streamFlags[0] != 0)
                throw new InvalidDataException("Unknown XZ Stream Version");

            _checkType = (CheckType)typeOfCheck;

            UInt32 crc = unchecked((uint)ReadLittleEndianInt());
            UInt32 calcCrc = Crc32.Compute(streamFlags);
            if (crc != calcCrc)
                throw new Exception("Stream header corrupt");
        }

        private void CheckMagicBytes(byte[] header)
        {
            if (!Enumerable.SequenceEqual(header, MagicHeader))
                throw new InvalidDataException("Not an XZ Stream");
        }

        public int ReadLittleEndianInt()
        {
            return (_reader.ReadByte() + (_reader.ReadByte() << 8) + (_reader.ReadByte() << 16) + (_reader.ReadByte() << 24));
        }
    }
}
