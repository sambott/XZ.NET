using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;

namespace XZ.Net.Tests
{
    [TestFixture]
    public class XZHeaderTests : XZTestsBase
    {
        [Test]
        public void RecordsStreamStartOnInit()
        {
            using (Stream badStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            {
                BinaryReader br = new BinaryReader(badStream);
                var header = new XZHeader(br);
                Assert.That(header.StreamStartPosition, Is.EqualTo(0));
            }
        }

        [Test]
        public void ChecksMagicNumber()
        {
            var bytes = Compressed.Clone() as byte[];
            bytes[3]++;
            using (Stream badMagicNumberStream = new MemoryStream(bytes))
            {
                BinaryReader br = new BinaryReader(badMagicNumberStream);
                var header = new XZHeader(br);
                var ex = Assert.Throws<InvalidDataException>(() => { header.Process(); });
                Assert.That(ex.Message, Is.EqualTo("Invalid XZ Stream"));
            }
        }

        [Test]
        public void CorruptHeaderThrows()
        {
            var bytes = Compressed.Clone() as byte[];
            bytes[8]++;
            using (Stream badCrcStream = new MemoryStream(bytes))
            {
                BinaryReader br = new BinaryReader(badCrcStream);
                var header = new XZHeader(br);
                var ex = Assert.Throws<InvalidDataException>(() => { header.Process(); });
                Assert.That(ex.Message, Is.EqualTo("Stream header corrupt"));
            }
        }

        [Test]
        public void BadVersionIfCrcOkButStreamFlagUnknown() { 
            var bytes = Compressed.Clone() as byte[];
            byte[] streamFlags = { 0x00, 0xF4 };
            byte[] crc = Crc32.Compute(streamFlags).ToLittleEndianBytes();
            streamFlags.CopyTo(bytes, 6);
            crc.CopyTo(bytes, 8);
            using (Stream badFlagStream = new MemoryStream(bytes))
            {
                BinaryReader br = new BinaryReader(badFlagStream);
                var header = new XZHeader(br);
                var ex = Assert.Throws<InvalidDataException>(() => { header.Process(); });
                Assert.That(ex.Message, Is.EqualTo("Unknown XZ Stream Version"));
            }
}

        [Test]
        public void ProcessesBlockCheckType()
        {
            BinaryReader br = new BinaryReader(CompressedStream);
            var header = new XZHeader(br);
            header.Process();
            Assert.That(header.BlockCheckType, Is.EqualTo(CheckType.CRC64));
        }

        [Test]
        public void CanCalculateBlockCheckSize()
        {
            BinaryReader br = new BinaryReader(CompressedStream);
            var header = new XZHeader(br);
            header.Process();
            Assert.That(header.BlockCheckSize, Is.EqualTo(8));
        }

        [Test]
        public void ProcessesStreamHeaderFromFactory()
        {
            var header = XZHeader.FromStream(CompressedStream);
            Assert.That(header.BlockCheckType, Is.EqualTo(CheckType.CRC64));
        }
    }
}
