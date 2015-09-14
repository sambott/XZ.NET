using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;

namespace PortableWikiViewer.Core.XZ.Tests
{
    [TestFixture]
    public class XZBlockTests : XZTestsBase
    {
        protected override void Rewind(Stream stream)
        {
            stream.Position = 12;
        }

        private byte[] ReadBytes(XZBlock block, int bytesToRead)
        {
            byte[] buffer = new byte[bytesToRead];
            var read = block.Read(buffer, 0, bytesToRead);
            if (read != bytesToRead)
                throw new EndOfStreamException();
            return buffer;
        }

        [Test]
        public void RecordsStreamStartOnInit()
        {
            using (Stream badStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            {
                var block = new XZBlock(badStream, CheckType.CRC64, 8);
                Assert.That(block.StreamStartPosition, Is.EqualTo(0));
            }
        }

        [Test]
        public void OnFindIndexBlockThrow()
        {
            var bytes = new byte[] { 0 };
            using (Stream indexBlockStream = new MemoryStream(bytes))
            {
                var XZBlock = new XZBlock(indexBlockStream, CheckType.CRC64, 8);
                Assert.Throws<XZIndexMarkerReachedException>(() => { ReadBytes(XZBlock, 1); });
            }
        }

        [Test]
        public void CrcIncorrectThrows()
        {
            var bytes = Compressed.Clone() as byte[];
            bytes[20]++;
            using (Stream badCrcStream = new MemoryStream(bytes))
            {
                Rewind(badCrcStream);
                var XZBlock = new XZBlock(badCrcStream, CheckType.CRC64, 8);
                var ex = Assert.Throws<InvalidDataException>(() => { ReadBytes(XZBlock, 1); });
                Assert.That(ex.Message, Is.EqualTo("Block header corrupt"));
            }
        }

        [Test]
        public void CanReadM()
        {
            var XZBlock = new XZBlock(CompressedStream, CheckType.CRC64, 8);
            Assert.That(Encoding.ASCII.GetBytes("M"), Is.EqualTo(ReadBytes(XZBlock, 1)));
        }

        [Test]
        public void CanReadMary()
        {
            var XZBlock = new XZBlock(CompressedStream, CheckType.CRC64, 8);
            var sr = new StreamReader(XZBlock);
            Assert.That(sr.ReadToEnd(), Is.EqualTo(Original));
        }
    }
}
