using System.Runtime.InteropServices;
using CSharpUtils.Endian;

namespace Gvd
{
    [StructLayout(LayoutKind.Sequential)]
    struct DatabaseViewerStart
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] Magic;

        public uint_be LayerWidth;

        public uint_be LayerHeight;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] StartBLK;

        public uint_be DataBaseLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Start;
    }
}