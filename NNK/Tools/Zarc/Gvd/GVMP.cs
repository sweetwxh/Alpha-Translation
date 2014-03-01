using System.Runtime.InteropServices;
using CSharpUtils.Endian;

namespace Gvd
{
    [StructLayout(LayoutKind.Sequential)]
    struct GVMP
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Magic;

        public uint_be Count;

        public uint_be HeaderLengh;

        public uint_be OriginLength;

        public uint_be OverlappedOffset;

        public uint_be OverlappedLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] NoUse;
    }
}