using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PortableWikiViewer.Core.Tests
{
    [TestFixture]
    public class Class1
    {
        [Test]
        public void TestTest() {
            Assert.AreEqual(1, Core.Class1.Test);
        }
    }
}
