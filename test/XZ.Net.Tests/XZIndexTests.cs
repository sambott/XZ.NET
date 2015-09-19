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
    public class XZIndexTests : XZTestsBase
    {
        protected override void Rewind(Stream stream)
        {
            stream.Position = 356;
        }

        [Test]
        public void RecordsStreamStartOnInit()
        {
            using (Stream badStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            {
                BinaryReader br = new BinaryReader(badStream);
                var index = new XZIndex(br, false);
                Assert.That(index.StreamStartPosition, Is.EqualTo(0));
            }
        }

        [Test]
        public void ThrowsIfHasNoIndexMarker()
        {
            using (Stream badStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            {
                BinaryReader br = new BinaryReader(badStream);
                var index = new XZIndex(br, false);
                Assert.That(index.Process, Throws.InstanceOf<InvalidDataException>());
            }
        }

        [Test]
        public void ReadsNumberOfRecords()
        {
            BinaryReader br = new BinaryReader(CompressedStream);
            var index = new XZIndex(br, false);
            index.Process();
            Assert.That(index.NumberOfRecords, Is.EqualTo(1));
        }
    }
}
