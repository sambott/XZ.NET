using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using NUnit.Framework;
using PortableWikiViewer.Core;

namespace PortableWikiViewer.Core.XZ.Tests
{
    [TestFixture]
    public class Crc32Tests
    {
        private const string SimpleString = @"The quick brown fox jumps over the lazy dog.";
        private readonly byte[] SimpleBytes = Encoding.ASCII.GetBytes(SimpleString);
        private const string SimpleString2 = @"Life moves pretty fast. If you don't stop and look around once in a while, you could miss it.";
        private readonly byte[] SimpleBytes2 = Encoding.ASCII.GetBytes(SimpleString2);

        [Test]
        public void ShortAsciiString()
        {
            var actual = Crc32.Compute(SimpleBytes);

            Assert.AreEqual((UInt32)0x519025e9, actual);
        }

        [Test]
        public void ShortAsciiString2()
        {
            var actual = Crc32.Compute(SimpleBytes2);

            Assert.AreEqual((UInt32)0x6ee3ad88, actual);
        }

    }
}
