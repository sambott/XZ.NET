﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace PortableWikiViewer.Core.XZ.Filters
{
    internal abstract class BlockFilter
    {
        public enum FilterTypes : ulong
        {
            DELTA = 0x03,
            ARCH_x86_FILTER = 0x04,
            ARCH_PowerPC_FILTER = 0x05,
            ARCH_IA64_FILTER = 0x06,
            ARCH_ARM_FILTER = 0x07,
            ARCH_ARMTHUMB_FILTER = 0x08,
            ARCH_SPARC_FILTER = 0x09,
            LZMA2 = 0x21,
        }

        public abstract bool AllowAsLast { get; }
        public abstract bool AllowAsNonLast { get; }
        public abstract bool ChangesDataSize { get; }

        public BlockFilter() { }

        public abstract void Init(byte[] properties);
        public abstract void ValidateFilter();

        static Dictionary<FilterTypes, Type> FilterMap = new Dictionary<FilterTypes, Type>()
        {
            {FilterTypes.LZMA2, typeof(Lzma2Filter) }
        };

        public FilterTypes FilterType { get; set; }
        internal static BlockFilter Read(BinaryReader reader)
        {
            var filterType = (FilterTypes)reader.ReadXZInteger();
            if (!FilterMap.ContainsKey(filterType))
                throw new NotImplementedException($"Filter {filterType} has not yet been implemented");
            var filter = Activator.CreateInstance(FilterMap[filterType]) as BlockFilter;

            var sizeOfProperties = reader.ReadXZInteger();
            if (sizeOfProperties > int.MaxValue)
                throw new InvalidDataException("Block filter information too large");
            byte[] properties = reader.ReadBytes((int)sizeOfProperties);
            filter.Init(properties);
            return filter;
        }
    }
}
