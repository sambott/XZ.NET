using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;
using PortableWikiViewer.Core;

namespace PortableWikiViewer.Core.Tests
{
    [TestFixture]
    public class Crc64Tests
    {
        private const string SimpleString = @"The quick brown fox jumps over the lazy dog.";
        private readonly byte[] SimpleBytes = Encoding.ASCII.GetBytes(SimpleString);
        private const string SimpleString2 = @"Life moves pretty fast. If you don't stop and look around once in a while, you could miss it.";
        private readonly byte[] SimpleBytes2 = Encoding.ASCII.GetBytes(SimpleString2);

        [Test]
        public void ShortAsciiString()
        {
            var actual = Crc64.Compute(SimpleBytes);

            Assert.AreEqual((UInt64)0x7E210EB1B03E5A1D, actual);
        }

        [Test]
        public void ShortAsciiString2()
        {
            var actual = Crc64.Compute(SimpleBytes2);

            Assert.AreEqual((UInt64)0x416B4150508661EE, actual);
        }

    }
}
