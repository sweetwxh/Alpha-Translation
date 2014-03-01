using System.Runtime.InteropServices;
using CSharpUtils.Endian;

namespace Gvd
{
    [StructLayout(LayoutKind.Sequential)]
    struct Entry
    {
        public uint_be GridWidth;

        public uint_be GridHeight;

        public uint_be LayerLevel;

        public uint_be ImageLength;

        public uint_be LengthPadding;

        public uint_be NotUse;

        public uint_be ImageWidth;

        public uint_be ImageHeight;
    }
}