using System.Runtime.InteropServices;
using CSharpUtils.Endian;

namespace Gvd
{
    [StructLayout(LayoutKind.Sequential)]
    struct DatabaseHeader
    {
        public uint_be EntryLength;

        public uint_be ParamLength;
    }
}