using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PortableWikiViewer.Core.XZ.Filters;

namespace PortableWikiViewer.Core.XZ
{
    internal class BlockInfo
    {
        public long BlockHeaderStart { get; set; }
        public int BlockHeaderSize { get; set; }
        public ulong? CompressedSize { get; set; }
        public ulong? UncompressedSize { get; set; }
        public List<BlockFilter> Filters { get; set; } = new List<BlockFilter>();
    }
}
