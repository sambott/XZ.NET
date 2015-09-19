using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace XZ.Net
{
    public class XZIndexRecord
    {
        public ulong UnpaddedSize { get; private set; }
        public ulong UncompressedSize { get; private set; }

        protected XZIndexRecord() { }

        public static XZIndexRecord FromBinaryReader(BinaryReader br)
        {
            var record = new XZIndexRecord();
            record.UnpaddedSize = br.ReadXZInteger();
            record.UncompressedSize = br.ReadXZInteger();
            return record;
        }
    }
}
