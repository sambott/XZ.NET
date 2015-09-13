using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PortableWikiViewer.Core.XZ.Filters;

namespace PortableWikiViewer.Core.XZ
{
    public sealed class XZBlock
    {
        public long StreamStartPosition { get; set; }
        public int BlockHeaderSize { get; set; }
        public ulong? CompressedSize { get; set; }
        public ulong? UncompressedSize { get; set; }
        public List<BlockFilter> Filters { get; set; } = new List<BlockFilter>();
        public bool HeaderLoaded { get; private set; }

        private readonly BinaryReader _reader;
        private int _numFilters;

        public XZBlock(Stream stream)
        {
            _reader = new BinaryReader(stream, Encoding.UTF8, true);
            StreamStartPosition = _reader.BaseStream.Position;
        }

        public void LoadHeader()
        {
            byte[] headerCache = CacheHeader();

            using (var cache = new MemoryStream(headerCache))
            using (var cachedReader = new BinaryReader(cache))
            {
                cachedReader.BaseStream.Position = 1; // skip the header size byte
                ReadBlockFlags(cachedReader);
                ReadFilters(cachedReader);
            }
            HeaderLoaded = true;
        }

        private byte ReadHeaderSize()
        {
            var blockHeaderSizeByte = _reader.ReadByte();
            if (blockHeaderSizeByte == 0)
                throw new XZIndexMarkerReachedException();
            BlockHeaderSize = (blockHeaderSizeByte + 1) * 4;
            return blockHeaderSizeByte;
        }

        private byte[] CacheHeader()
        {
            byte blockHeaderSizeByte = ReadHeaderSize();

            byte[] blockHeaderWithoutCrc = new byte[BlockHeaderSize - 4];
            blockHeaderWithoutCrc[0] = blockHeaderSizeByte;
            _reader.ReadBytes(BlockHeaderSize - 5).CopyTo(blockHeaderWithoutCrc, 1);

            uint crc = _reader.ReadLittleEndianUInt32();
            uint calcCrc = Crc32.Compute(blockHeaderWithoutCrc);
            if (crc != calcCrc)
                throw new InvalidDataException("Block header corrupt");

            return blockHeaderWithoutCrc;
        }

        private void ReadFilters(BinaryReader reader, long baseStreamOffset = 0)
        {
            int nonLastSizeChangers = 0;
            for (int i = 0; i < _numFilters; i++)
            {
                var filter = BlockFilter.Read(reader);
                if ((i + 1 == _numFilters && !filter.AllowAsLast)
                    || (i + 1 < _numFilters && !filter.AllowAsNonLast))
                    throw new InvalidDataException("Block Filters in bad order");
                if (filter.ChangesDataSize && i + 1 < _numFilters)
                    nonLastSizeChangers++;
                filter.ValidateFilter();
                Filters.Add(filter);
            }
            if (nonLastSizeChangers > 2)
                throw new InvalidDataException("More than two non-last block filters cannot change stream size");

            int blockHeaderPaddingSize = BlockHeaderSize -
                (4 + (int)(reader.BaseStream.Position - baseStreamOffset));
            byte[] blockHeaderPadding = reader.ReadBytes(blockHeaderPaddingSize);
            if (!blockHeaderPadding.All(b => b == 0))
                throw new InvalidDataException("Block header contains unknown fields");
        }

        private void ReadBlockFlags(BinaryReader reader)
        {
            var blockFlags = reader.ReadByte();
            _numFilters = (blockFlags & 0x03) + 1;
            byte reserved = (byte)(blockFlags & 0x3C);

            if (reserved != 0)
                throw new InvalidDataException("Reserved bytes used, perhaps an unknown XZ implementation");

            bool compressedSizePresent = (blockFlags & 0x40) != 0;
            bool uncompressedSizePresent = (blockFlags & 0x80) != 0;

            if (compressedSizePresent)
                CompressedSize = reader.ReadXZInteger();
            if (uncompressedSizePresent)
                UncompressedSize = reader.ReadXZInteger();
        }
    }
}
