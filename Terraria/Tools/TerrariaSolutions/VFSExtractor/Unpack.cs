using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpUtils;
using System.Runtime.InteropServices;
using System.IO;
using Ionic.Zlib;

namespace VFSExtractor
{
    public class Unpack
    {
        /// <summary>
        /// 文件个数
        /// </summary>
        private const int FileCount = 263;

        /// <summary>
        /// 文件控制数据起始偏移量
        /// </summary>
        private const uint FileCtlOffset = 0x100;

        /// <summary>
        /// 数据偏移量
        /// </summary>
        private const uint BaseOffset = 0x19a8;

        /// <summary>
        /// 文件列表偏移
        /// </summary>
        private const uint TableOffset = 0x2a252ec;


        private FileStream stream;
        private string directoryName;

        public Unpack(FileStream stream, string directoryName = "data")
        {
            this.directoryName = directoryName;
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            this.stream = stream;
        }

        public void Extract()
        {
            var files = ReadFileInfo();
            var nameTable = ReadFileNameTable();
            foreach (var file in files)
            {
                stream.Seek(BaseOffset + file.Offset, SeekOrigin.Begin);
                byte[] data = new byte[file.Length];
                stream.Read(data, 0, data.Length);

                if (file.Compressed)
                {
                    data = Decompress(data);
                }

                //写出文件
                string processedFile = nameTable[(int)file.Index];
                File.WriteAllBytes(Path.Combine(directoryName, processedFile), data);
                Console.WriteLine("输出：{0}", processedFile);
            }
        }

        /// <summary>
        /// 解压缩数据
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private byte[] Decompress(byte[] source)
        {
            byte[] decompressed = null;
            using (MemoryStream writer = new MemoryStream())
            {
                using (MemoryStream stream = new MemoryStream(source))
                {
                    var header = ReadCompressHeader(stream);
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
                        writer.Write(dtemp, 0, dtemp.Length);
                    }
                }
                decompressed = writer.ToArray();
            }
            return decompressed;
        }

        /// <summary>
        /// 读取压缩头文件
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private CompressHeader ReadCompressHeader(MemoryStream stream)
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

        /// <summary>
        /// 读取文件名列表
        /// </summary>
        /// <returns></returns>
        private List<string> ReadFileNameTable()
        {
            List<string> names = new List<string>();

            stream.Seek(TableOffset, SeekOrigin.Begin);

            BinaryReader reader = new BinaryReader(stream);
            for (int i = 0; i < FileCount; i++)
            {
                uint nameLength = reader.ReadUInt32();
                string name = stream.ReadString((int)nameLength, Encoding.UTF8);
                names.Add(name);
            }

            return names;
        }

        /// <summary>
        /// 填充文件信息
        /// </summary>
        /// <returns></returns>
        private List<FileStruct> ReadFileInfo()
        {
            List<FileStruct> files = new List<FileStruct>();

            stream.Seek(FileCtlOffset, SeekOrigin.Begin);

            for (int i = 0; i < FileCount; i++)
            {
                var file = stream.ReadStruct<FileStruct>();
                files.Add(file);
            }

            return files;
        }
    }

    public struct CompressHeader
    {
        public uint TotalSize;

        public List<uint> BlockOffset { get; set; }
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 24)]
    public struct FileStruct
    {
        [FieldOffset(0x00)]
        public uint Index;

        [FieldOffset(0x04)]
        public int CompressType;

        [FieldOffset(0x08)]
        public int DirectoryIndex;

        [FieldOffset(0x0C)]
        public uint Offset;

        [FieldOffset(0x10)]
        public uint Length;

        [FieldOffset(0x14)]
        public uint Hash;

        public bool Compressed
        {
            get
            {
                return CompressType == 2;
            }
        }
    }
}
