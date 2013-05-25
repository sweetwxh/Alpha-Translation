/***************************************
 * 记录：
 * 段的头文件如果非偶数，则需要补8位0。
 * 数据压缩后，如果与16求模不为0，则需要补 (length/16+1)*16-length个0
 * 压缩的时候，如果数据大于0x10000，则每次读取0x10000进行分段压缩，并注意补0
 * 
 * ************************************/
using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Zarc
{
    public class ZarcCompress
    {
        private const uint SectionMaxLength = 0x10000;

        /// <summary>
        /// 压缩文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="deleteOrigin"></param>
        public void Zip(string file, bool deleteOrigin = false)
        {
            FileInfo fileInfo = new FileInfo(file);
            Console.WriteLine("正在处理：{0}", fileInfo.Name);
            using (FileStream fileReader = fileInfo.OpenRead())
            {
                ZarcHeader header = new ZarcHeader();
                uint lastSectionLength = SectionMaxLength;
                header.Magic = new byte[] { 
                    0x7a,0x61,0x72,0x63
                };
                if (fileInfo.Length <= SectionMaxLength)
                {
                    header.SectionCount = 1;
                    lastSectionLength = (uint)fileInfo.Length;
                }
                else
                {
                    //计算段的大小
                    header.SectionCount = (ushort)(fileInfo.Length / SectionMaxLength);
                    if (fileInfo.Length % SectionMaxLength != 0)
                    {
                        lastSectionLength = (uint)(fileInfo.Length - header.SectionCount * SectionMaxLength);
                        header.SectionCount += 1;
                    }
                }
                header.OriginSize = (uint)fileInfo.Length;

                SectionHeader[] sections = new SectionHeader[header.SectionCount];

                //计算数据的初始偏移量
                int evenSection = header.SectionCount;
                if (header.SectionCount % 2 != 0)
                {
                    evenSection += 1;
                }
                uint realOffset = (uint)(0x10 + evenSection * 8);
                header.CompressedSize = realOffset;

                //开始按照分段循环进行压缩
                using (MemoryStream cMemory = new MemoryStream())
                {
                    for (int i = 0; i < header.SectionCount; i++)
                    {
                        using (MemoryStream swapMemory = new MemoryStream())
                        {
                            sections[i] = new SectionHeader();
                            using (DeflateStream compressionStream = new DeflateStream(swapMemory, CompressionMode.Compress, true))
                            {
                                uint currentLength = (i == header.SectionCount - 1) ? lastSectionLength : SectionMaxLength;
                                byte[] data = new byte[currentLength];
                                fileReader.Read(data, 0, (int)currentLength);
                                compressionStream.Write(data, 0, data.Length);
                                if (currentLength != SectionMaxLength)
                                    sections[i].OriginSize = (ushort)currentLength;
                            }
                            //对压缩的数据补0并写入压缩流中
                            sections[i].CompressedSize = (ushort)swapMemory.Length;
                            uint writeLength = (uint)swapMemory.Length;
                            if (writeLength % 16 != 0)
                            {
                                writeLength = (writeLength / 16 + 1) * 16;
                            }
                            byte[] cdata = swapMemory.ToArray();//new byte[writeLength];
                            //swapMemory.Read(cdata, 0, sections[i].CompressedSize);
                            cMemory.Write(cdata, 0, cdata.Length);
                            if (writeLength != swapMemory.Length)
                            {
                                int space = (int)(writeLength - swapMemory.Length);
                                cMemory.Write(new byte[space], 0, space);
                            }

                            header.CompressedSize += writeLength;

                            sections[i].Offset = realOffset + 1;
                            realOffset += writeLength;
                        }
                    }
                    //开始写出压缩文件
                    string currentFileName = fileInfo.FullName;
                    string newFileName = currentFileName + ".zarc";
                    using (FileStream writer = new FileStream(newFileName, FileMode.Create, FileAccess.Write))
                    {
                        //先写Header
                        writer.Write(header.Magic, 0, 4);
                        writer.Write(new byte[] { 0, 1 }, 0, 2);
                        writer.Write(DataConverter.BigEndian.GetBytes(header.SectionCount), 0, 2);
                        writer.Write(DataConverter.BigEndian.GetBytes(header.OriginSize), 0, 4);
                        writer.Write(DataConverter.BigEndian.GetBytes(header.CompressedSize), 0, 4);

                        //循环写入段信息
                        foreach (SectionHeader section in sections)
                        {
                            writer.Write(DataConverter.BigEndian.GetBytes(section.CompressedSize), 0, 2);
                            writer.Write(DataConverter.BigEndian.GetBytes(section.OriginSize), 0, 2);
                            writer.Write(DataConverter.BigEndian.GetBytes(section.Offset), 0, 4);
                        }

                        if (header.SectionCount != evenSection)
                        {
                            writer.Write(new byte[8], 0, 8);
                        }

                        writer.Write(cMemory.ToArray(), 0, (int)cMemory.Length);
                    }
                }
            }

            if (deleteOrigin)
            {
                fileInfo.Delete();
            }
        }

        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public void Unzip(string file, bool deleteOrigin = false)
        {
            FileInfo fileInfo = new FileInfo(file);
            Console.WriteLine("正在处理：{0}", fileInfo.Name);
            using (FileStream fileReader = fileInfo.OpenRead())
            {
                using (BinaryReader dataReader = new BinaryReader(fileReader))
                {
                    //处理文件头
                    ZarcHeader header = new ZarcHeader();
                    header.Magic = dataReader.ReadBytes(4);
                    dataReader.ReadInt16();//useless
                    Console.WriteLine("======================Header=========================");
                    header.SectionCount = DataConverter.BigEndian.GetUInt16(dataReader.ReadBytes(2), 0);
                    Console.WriteLine("段数量：{0}", header.SectionCount);
                    header.OriginSize = DataConverter.BigEndian.GetUInt32(dataReader.ReadBytes(4), 0);
                    Console.WriteLine("原始文件大小：{0}", header.OriginSize);
                    header.CompressedSize = DataConverter.BigEndian.GetUInt32(dataReader.ReadBytes(4), 0);
                    Console.WriteLine("压缩文件大小：{0}", header.CompressedSize);
                    Console.WriteLine("====================================================");

                    //处理段信息
                    SectionHeader[] sections = new SectionHeader[header.SectionCount];
                    for (int i = 0; i < header.SectionCount; i++)
                    {
                        sections[i] = new SectionHeader();
                        sections[i].CompressedSize = DataConverter.BigEndian.GetUInt16(dataReader.ReadBytes(2), 0);
                        sections[i].OriginSize = DataConverter.BigEndian.GetUInt16(dataReader.ReadBytes(2), 0);
                        sections[i].Offset = DataConverter.BigEndian.GetUInt32(dataReader.ReadBytes(4), 0);
#if DEBUG
                        Console.WriteLine("===================Section {0}========================", i);
                        Console.WriteLine("段的压缩大小：{0}", sections[i].CompressedSize);
                        Console.WriteLine("段的实际偏移量大小：{0}", sections[i].RealOffset);
                        Console.WriteLine("===============================================");
#endif

                    }

                    //开始解压缩
                    string currentFileName = fileInfo.FullName;
                    string newFileName = currentFileName.Remove(currentFileName.Length - fileInfo.Extension.Length);
#if DEBUG
                    string configFileName = newFileName + ".config";
#endif
                    using (MemoryStream dMemory = new MemoryStream())
                    {
#if DEBUG
                        int sectionIndex = 0;
                        long currentPosition = 0;
                        List<string> configText = new List<string>();
#endif
                        foreach (SectionHeader section in sections)
                        {
                            fileReader.Seek(section.RealOffset, SeekOrigin.Begin);
                            byte[] cdata = new byte[section.CompressedSize];
                            fileReader.Read(cdata, 0, section.CompressedSize);

                            if (section.CompressedSize != section.OriginSize)
                            {
                                //解压数据
                                using (MemoryStream cMemory = new MemoryStream(cdata))
                                {
                                    using (MemoryStream swapMemory = new MemoryStream())
                                    {
                                        using (DeflateStream decompressionStream = new DeflateStream(cMemory, CompressionMode.Decompress))
                                        {
                                            decompressionStream.CopyTo(swapMemory);
                                        }
                                        dMemory.Write(swapMemory.ToArray(), 0, (int)swapMemory.Length);
#if DEBUG
                                    //段索引 解压后的长度 在解压文件中的偏移位置
                                    configText.Add(string.Format("{0} {1} {2}", sectionIndex, swapMemory.Length, currentPosition));
                                    sectionIndex++;
                                    currentPosition += swapMemory.Length;
#endif
                                    }
                                }
                            }
                            else
                            {
                                dMemory.Write(cdata, 0, cdata.Length);
                            }
                        }

                        //写出解压后的数据和控制文件
#if DEBUG
                        File.WriteAllLines(configFileName, configText.ToArray());
#endif
                        File.WriteAllBytes(newFileName, dMemory.ToArray());

                        //在当前文件夹下留一个列表，表示原始文件全名称
                        File.AppendAllLines(fileInfo.DirectoryName + "\\files.txt", new string[] { newFileName.Substring(newFileName.LastIndexOf("\\") + 1) });
                    }
                }
            }

            if (deleteOrigin)
            {
                fileInfo.Delete();
            }


        }

        private static void CopyStream(System.IO.Stream input, System.IO.Stream output)
        {
            byte[] buffer = new byte[2000];
            int len;
            while ((len = input.Read(buffer, 0, 2000)) > 0)
            {
                output.Write(buffer, 0, len);
            }
            output.Flush();
        }
    }

    public struct ZarcHeader//Big Endian
    {
        public byte[] Magic { get; set; }

        public ushort SectionCount { get; set; }

        public uint OriginSize { get; set; }

        public uint CompressedSize { get; set; }
    }

    public struct SectionHeader
    {
        public ushort CompressedSize { get; set; }

        public ushort OriginSize { get; set; }

        //读取出来后要 -1
        public uint Offset { get; set; }

        //在文件中段的偏移地址
        public uint RealOffset
        {
            get
            {
                return Offset - 1;
            }
        }
    }
}
