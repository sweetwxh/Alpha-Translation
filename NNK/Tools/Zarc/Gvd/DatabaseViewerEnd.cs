using System.Runtime.InteropServices;
using CSharpUtils.Endian;

namespace Gvd
{
    [StructLayout(LayoutKind.Sequential)]
    struct DatabaseViewerEnd
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] EndBLK;

        public uint_be ImageTotalLength;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] End;
    }
}