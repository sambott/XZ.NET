using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PortableWikiViewer.Core
{
    public class XZStreamReader
    {

        public class BlockInfo
        {
            public long BlockStart { get; set; }
            public int BlockSize { get; set; }
            public long? CompressedSize { get; set; }
            public long? UncompressedSize { get; set; }
        }

        public enum CheckType : byte
        {
            NONE =   0x00,
            CRC32 =  0x01,
            CRC64 =  0x04,
            SHA256 = 0x0A
        }

        private readonly BinaryReader _reader;
        private byte[] MagicHeader = new byte[] { 0xFD, 0x37, 0x7A, 0x58, 0x5a, 0x00 };
        private readonly long _streamStart;
        internal List<BlockInfo> BlocksInfo { get; private set; } = new List<BlockInfo>();

        internal CheckType BlockCheckType { get; private set; }
        private int _checkSize => ((((int)BlockCheckType) + 2) / 3) * 4;

        public XZStreamReader(Stream stream)
        {
            _reader = new BinaryReader(stream, Encoding.UTF8, true);
            _streamStart = stream.Position;
            ProcessHeader();
            PreProcessBlocks();
            ProcessIndex();
            ProcessFooter();
        }

        private void ProcessFooter()
        {
            throw new NotImplementedException();
        }

        private void ProcessIndex()
        {
            throw new NotImplementedException();
        }

        private void PreProcessBlocks()
        {
            for(;;)
            {
                var blockSizeByte = _reader.ReadByte();
                if (blockSizeByte == 0)
                    break;
                BlocksInfo.Add(ProcessBlockHeader(blockSizeByte));
            }
        }

        private BlockInfo ProcessBlockHeader(byte blockSizeByte)
        {
            var info = new BlockInfo();
            info.BlockStart = _reader.BaseStream.Position - 1;
            info.BlockSize = (blockSizeByte + 1) * 4;

            var blockFlags = _reader.ReadByte();
            byte filters = (byte)(blockFlags & 0x03);
            byte reserved = (byte)(blockFlags & 0x3C);
            byte compressedSizePresentByte = (byte)(blockFlags & 0x40);
            byte uncompressedSizePresentByte = (byte)(blockFlags & 0x80);

            return info;
        }

        private void ProcessHeader()
        {
            CheckMagicBytes(_reader.ReadBytes(6));
            ProcessStreamFlags();
            CheckBlockCheckTypeIsSupported();
        }

        private void CheckBlockCheckTypeIsSupported()
        {
            switch (BlockCheckType)
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

        private void ProcessStreamFlags()
        {
            byte[] streamFlags = _reader.ReadBytes(2);
            UInt32 crc = unchecked((uint)ReadLittleEndianInt());
            UInt32 calcCrc = Crc32.Compute(streamFlags);
            if (crc != calcCrc)
                throw new InvalidDataException("Stream header corrupt");

            BlockCheckType = (CheckType)(streamFlags[1] & 0x0F);
            byte futureUse = (byte)(streamFlags[1] & 0xF0);
            if (futureUse != 0 || streamFlags[0] != 0)
                throw new InvalidDataException("Unknown XZ Stream Version");
        }

        private void CheckMagicBytes(byte[] header)
        {
            if (!Enumerable.SequenceEqual(header, MagicHeader))
                throw new InvalidDataException("Invalid XZ Stream");
        }

        private int ReadLittleEndianInt()
        {
            return (_reader.ReadByte() + (_reader.ReadByte() << 8) + (_reader.ReadByte() << 16) + (_reader.ReadByte() << 24));
        }
    }
}
