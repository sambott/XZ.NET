using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PortableWikiViewer.Core;

namespace PortableWikiViewer.Core.XZ
{
    public abstract class XZReadOnlyStream : ReadOnlyStream
    {
        public long StreamStartPosition { get; protected set; }

        public XZReadOnlyStream(Stream stream)
        {
            BaseStream = stream;
            if (!BaseStream.CanRead)
                throw new InvalidDataException("Must be able to read from stream");
            StreamStartPosition = BaseStream.Position;
        }
    }
}
