using CSharpUtils.Endian;

namespace Gvd
{
    struct Page
    {
        public uint_be FileNameOffset;

        public uint_be FileNameLength;

        public uint_be DBVOffset;

        public uint_be DBVLength;
    }
}