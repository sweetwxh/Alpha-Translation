#region 引用

using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zlib;
using Mono;

#endregion

namespace TerrariaSolutions
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ExtractBlock(0x29a684e, 0x829, "credits.txt.c");
            using (FileStream stream = File.OpenRead("credits.txt.c"))
            {
                byte[] d = DecompressData(stream);
                File.WriteAllBytes("credits.txt", d);
            }
            Console.WriteLine("process complete");
            Console.ReadKey();
        }

        public static byte[] DecompressData(FileStream stream)
        {
            List<byte> decompressed = new List<byte>();
            var header = ReadHeader(stream);
            for (int i = 0; i < header.BlockOffset.Count; i++)
            {
                stream.Seek(header.BlockOffset[i], SeekOrigin.Begin);
                uint count = 0;
                if (i == header.BlockOffset.Count - 1) //处理最后一个
                {
                    count = (uint)stream.Length - header.BlockOffset[i];
                }
                else
                {
                    count = header.BlockOffset[i + 1] - header.BlockOffset[i];
                }
                byte[] temp = new byte[count];
                stream.Read(temp, 0, temp.Length);
                byte[] dtemp = ZlibStream.UncompressBuffer(temp);
                decompressed.AddRange(dtemp);
            }
            return decompressed.ToArray();
        }

        public static CompressHeader ReadHeader(FileStream stream)
        {
            BinaryReader reader = new BinaryReader(stream);

            stream.Seek(0, SeekOrigin.Begin);
            CompressHeader header = new CompressHeader();
            header.TotalSize = reader.ReadUInt32();
            //读取第一个Block的偏移位置
            header.BlockOffset = new List<uint>();
            header.BlockOffset.Add(reader.ReadUInt32());
            while (stream.Position < header.BlockOffset[0])
            {
                header.BlockOffset.Add(reader.ReadUInt32());
            }
            return header;

        }


        public static void ExtractBlock(int offset, int length, string fileName)
        {
            int baseOffset = 0x19a8;
            int fileOffset = offset;
            using (FileStream inStream = File.OpenRead("data.vfs"))
            {
                inStream.Seek(baseOffset + fileOffset, SeekOrigin.Begin);
                byte[] buffer = new byte[length];
                inStream.Read(buffer, 0, length);
                File.WriteAllBytes(fileName, buffer);
            }
        }
    }

    public class CompressHeader
    {
        public uint TotalSize { get; set; }

        public List<uint> BlockOffset { get; set; }
    }
}