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
    public class XZStreamReaderTests : XZTestsBase
    {

        [Test]
        public void CanReadStream()
        {
            XZStream xz = new XZStream(CompressedStream);
            using (var sr = new StreamReader(xz))
            {
                string uncompressed = sr.ReadToEnd();
                Assert.That(uncompressed, Is.EqualTo(Original));
            }
        }
    }
}
