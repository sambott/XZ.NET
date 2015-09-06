using System;
using System.IO;

namespace PortableWikiViewer.Core
{
    public static class BinaryUtils
    {
        public static int ReadLittleEndianInt32(this BinaryReader reader)
        {
            return (reader.ReadByte() + (reader.ReadByte() << 8) + (reader.ReadByte() << 16) + (reader.ReadByte() << 24));
        }

        public static uint ReadLittleEndianUInt32(this BinaryReader reader)
        {
            return unchecked((uint)ReadLittleEndianInt32(reader));
        }
    }
}
