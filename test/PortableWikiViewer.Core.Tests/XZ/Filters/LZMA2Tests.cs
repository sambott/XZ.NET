using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using PortableWikiViewer.Core.XZ.Filters;

namespace PortableWikiViewer.Core.Tests.XZ.Filters
{
    [TestFixture]
    public class Lzma2Tests
    {
        Lzma2Filter filter;

        [SetUp]
        public void BeforeEach()
        {
            filter = new Lzma2Filter();
        }

        [Test]
        public void IsOnlyAllowedLast()
        {
            Assert.That(filter.AllowAsLast, Is.True);
            Assert.That(filter.AllowAsNonLast, Is.False);
        }

        [Test]
        public void ChangesStreamSize()
        {
            Assert.That(filter.ChangesDataSize, Is.True);
        }

        [Test]
        [TestCase(0, (uint)4 * 1024)]
        [TestCase(1, (uint)6 * 1024)]
        [TestCase(2, (uint)8 * 1024)]
        [TestCase(3, (uint)12 * 1024)]
        [TestCase(38, (uint)2 * 1024 * 1024 * 1024)]
        [TestCase(39, (uint)3 * 1024 * 1024 * 1024)]
        [TestCase(40, (uint)(1024 * 1024 * 1024 - 1) * 4 + 3)]
        public void CalculatesDictionarySize(byte inByte, uint dicSize)
        {
            filter.Init(new[] { inByte });
            Assert.That(filter.DictionarySize, Is.EqualTo(dicSize));
        }

        [Test]
        public void CalculatesDictionarySizeError()
        {
            uint temp;
            filter.Init(new byte[] { 41 });
            var ex = Assert.Throws<OverflowException>(() =>
            {
                temp = filter.DictionarySize;
            });
            Assert.That(ex.Message, Is.EqualTo("Dictionary size greater than UInt32.Max"));
        }

        [Test]
        [TestCase(new byte[] { })]
        [TestCase(new byte[] { 0, 0 })]
        public void OnlyAcceptsOneByte(byte[] bytes)
        {
            var ex = Assert.Throws<InvalidDataException>(() => filter.Init(bytes));
            Assert.That(ex.Message, Is.EqualTo("LZMA properties unexpected length"));
        }

        [Test]
        public void ReservedBytesThrow()
        {
            var ex = Assert.Throws<InvalidDataException>(() => filter.Init(new byte[] { 0xC0 }));
            Assert.That(ex.Message, Is.EqualTo("Reserved bits used in LZMA properties"));
        }
    }
}
