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
                var block = new XZBlock(badStream);
                Assert.That(block.StreamStartPosition, Is.EqualTo(0));
            }
        }

        [Test]
        public void OnFindIndexBlockThrow()
        {
            var bytes = new byte[] { 0 };
            using (Stream indexBlockStream = new MemoryStream(bytes))
            {
                var XZBlock = new XZBlock(indexBlockStream);
                Assert.Throws<XZIndexMarkerReachedException>(() => { ReadBytes(XZBlock, 1); });
            }
        }
    }
}
