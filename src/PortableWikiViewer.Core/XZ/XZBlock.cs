using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using PortableWikiViewer.Core.XZ.Filters;

namespace PortableWikiViewer.Core.XZ
{
    public sealed class XZBlock : ReadOnlyStream
    {
        public int BlockHeaderSize { get; set; }
        public ulong? CompressedSize { get; set; }
        public ulong? UncompressedSize { get; set; }
        public List<BlockFilter> Filters { get; set; } = new List<BlockFilter>();
        public bool HeaderIsLoaded { get; private set; }
        
        private int _numFilters;

        public XZBlock(Stream stream) : base(stream)
        {
        }

        private void LoadHeader()
        {
            byte[] headerCache = CacheHeader();

            using (var cache = new MemoryStream(headerCache))
            using (var cachedReader = new BinaryReader(cache))
            {
                cachedReader.BaseStream.Position = 1; // skip the header size byte
                ReadBlockFlags(cachedReader);
                ReadFilters(cachedReader);
            }
            HeaderIsLoaded = true;
        }

        private byte ReadHeaderSize()
        {
            var blockHeaderSizeByte = (byte)BaseStream.ReadByte();
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
            var read = BaseStream.Read(blockHeaderWithoutCrc, 1, BlockHeaderSize - 5);
            if (read != BlockHeaderSize - 5)
                throw new EndOfStreamException("Reached end of stream unexectedly");

            uint crc = BaseStream.ReadLittleEndianUInt32();
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            if (!HeaderIsLoaded)
                LoadHeader();
            return bytesRead;
        }
    }
}
