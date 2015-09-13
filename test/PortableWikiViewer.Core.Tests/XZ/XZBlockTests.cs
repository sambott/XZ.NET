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
        [Test]
        public void RecordsStreamStartOnInit()
        {
            using (Stream badStream = new MemoryStream(new byte[] { 1, 2, 3, 4, 5 }))
            {
                var block = new XZBlock(badStream);
                Assert.That(block.StreamStartPosition, Is.EqualTo(0));
            }
        }

        [Test]
        public void OnProcess()
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
    }
}
