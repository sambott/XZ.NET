using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PortableWikiViewer.Core.XZ.Filters;

namespace PortableWikiViewer.Core.XZ
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

        private BlockInfo ProcessBlockHeader(byte blockHeaderSizeByte)
        {
            var info = new BlockInfo();
            info.BlockHeaderStart = _reader.BaseStream.Position - 1;
            info.BlockHeaderSize = (blockHeaderSizeByte + 1) * 4;

            byte[] blockHeaderWithoutCrc = new byte[info.BlockHeaderSize - 4];
            blockHeaderWithoutCrc[0] = blockHeaderSizeByte;
            _reader.ReadBytes(info.BlockHeaderSize - 5).CopyTo(blockHeaderWithoutCrc, 1);
            
            uint crc = _reader.ReadLittleEndianUInt32();
            uint calcCrc = Crc32.Compute(blockHeaderWithoutCrc);
            if (crc != calcCrc)
                throw new InvalidDataException("Block header corrupt");

            using (var cache = new MemoryStream(blockHeaderWithoutCrc))
            using (var cachedReader = new BinaryReader(cache))
            {
                cachedReader.BaseStream.Position = 1;

                var blockFlags = cachedReader.ReadByte();
                int numFilters = (blockFlags & 0x03) + 1;
                byte reserved = (byte)(blockFlags & 0x3C);
                bool compressedSizePresent = (blockFlags & 0x40) != 0;
                bool uncompressedSizePresent = (blockFlags & 0x80) != 0;

                if (compressedSizePresent)
                    info.CompressedSize = cachedReader.ReadXZInteger();
                if (uncompressedSizePresent)
                    info.UncompressedSize = cachedReader.ReadXZInteger();

                if (reserved != 0)
                    throw new InvalidDataException("Reserved bytes used, perhaps an unknown XZ implementation");

                int nonLastSizeChangers = 0;
                for (int i = 0; i < numFilters; i++)
                {
                    var filter = BlockFilter.Read(cachedReader);
                    if ((i + 1 == numFilters && !filter.AllowAsLast)
                        || (i + 1 < numFilters && !filter.AllowAsNonLast))
                        throw new InvalidDataException("Block Filters in bad order");
                    if (filter.ChangesDataSize && i + 1 < numFilters)
                        nonLastSizeChangers++;
                    filter.ValidateFilter();
                    info.Filters.Add(filter);
                }
                if (nonLastSizeChangers > 2)
                    throw new InvalidDataException("More than two non-last block filters cannot change stream size");

                int blockHeaderPaddingSize = info.BlockHeaderSize -
                    (4 + (int)(cachedReader.BaseStream.Position - info.BlockHeaderStart));
                byte[] blockHeaderPadding = cachedReader.ReadBytes(blockHeaderPaddingSize);
                if (!blockHeaderPadding.All(b => b == 0))
                    throw new InvalidDataException("Block header contains unknown fields");
            }
            return info;
        }

        private void ProcessHeader()
        {
            CheckMagicBytes(_reader.ReadBytes(6));
            ProcessStreamFlags();
            AssertBlockCheckTypeIsSupported();
        }

        private void AssertBlockCheckTypeIsSupported()
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
            UInt32 crc = _reader.ReadLittleEndianUInt32();
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
    }
}
