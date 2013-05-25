using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TextWrapper
{
    public class TextProcessor
    {
        public const uint HeaderLength = 0x10;

        /// <summary>
        /// 移除数组中的空字符串
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string[] RemoveEmptyString(string[] source)
        {
            List<string> temp = new List<string>();
            foreach (string single in source)
            {
                if (!single.Equals(string.Empty))
                {
                    temp.Add(single);
                }
            }
            return temp.ToArray();
        }

        public void Wrap(FileInfo file)
        {
            //判断是否为需要处理的文本文件
            if (!Regex.IsMatch(file.Name, "_text_ja"))
            {
                Console.WriteLine("跳过文件：{0}",file.FullName);
                return;
            }
            string currentFileName = file.FullName;
            string originFileName = currentFileName.Remove(currentFileName.Length - file.Extension.Length);//txt对应的原始文件名称

            Console.WriteLine("正在处理：{0}", currentFileName);
            //读取所有的句子
            string[] sentences = File.ReadAllLines(currentFileName);
            sentences = RemoveEmptyString(sentences);

            FileHeader header = new FileHeader();
            header.SentenceCount = (uint)sentences.Length;

            byte[] textData = null;
            using (MemoryStream textMemory = new MemoryStream())
            {
                foreach (string single in sentences)
                {
                    byte[] singleData = Encoding.UTF8.GetBytes(single);
                    textMemory.Write(singleData, 0, singleData.Length);
                    textMemory.WriteByte(0);//以0做结尾
                }

                header.Length = (uint)textMemory.Length;
                uint writeLength = header.Length;
                if (writeLength % 16 != 0)
                {
                    writeLength = (writeLength / 16 + 1) * 16;
                }
                uint space = writeLength - header.Length;
                for (uint i = 0; i < space; i++)//填充FF
                    textMemory.WriteByte(0xff);

                textData = textMemory.ToArray();
            }

            //通过原始文件来生成新的文件
            using (MemoryStream fileMemory = new MemoryStream())
            {
                using (FileStream originReader = new FileStream(originFileName, FileMode.Open, FileAccess.Read))
                {
                    byte[] fixData = new byte[8];
                    originReader.Read(fixData, 0, 8);
                    fileMemory.Write(fixData, 0, 8);

                    //写入译文文本长度
                    fileMemory.Write(DataConverter.BigEndian.GetBytes(header.Length), 0, 4);
                    //写入句子的数量
                    fileMemory.Write(DataConverter.BigEndian.GetBytes(header.SentenceCount), 0, 4);

                    //处理头部索引
                    uint offset = DataConverter.BigEndian.GetUInt32(fixData, 4);
                    uint indexLength = offset - HeaderLength;
                    byte[] hIndex = new byte[indexLength];
                    originReader.Seek(HeaderLength, SeekOrigin.Begin);
                    originReader.Read(hIndex, 0, hIndex.Length);

                    fileMemory.Write(hIndex, 0, hIndex.Length);//写入索引

                    fileMemory.Write(textData, 0, textData.Length);//写入文本

                    //处理尾部索引
                    //获取原始文本长度
                    originReader.Seek(8, SeekOrigin.Begin);
                    byte[] bLength = new byte[4];
                    originReader.Read(bLength, 0, 4);
                    uint originLength = DataConverter.BigEndian.GetUInt32(bLength, 0);

                    uint skipLength = originLength + offset;
                    if (skipLength % 16 != 0)
                    {
                        skipLength = (skipLength / 16 + 1) * 16;
                    }

                    originReader.Seek(skipLength, SeekOrigin.Begin);
                    uint readLength = (uint)(originReader.Length - skipLength);
                    byte[] fIndex = new byte[readLength];
                    originReader.Read(fIndex, 0, (int)readLength);

                    //写入尾索引
                    fileMemory.Write(fIndex, 0, fIndex.Length);
                }
                //覆盖原始文件
                File.WriteAllBytes(originFileName, fileMemory.ToArray());
            }

            file.Delete();
        }

        public void Extract(FileInfo file)
        {
            //判断是否为需要处理的文本文件
            if (!Regex.IsMatch(file.Name, "_text_ja"))
            {
                Console.WriteLine("跳过文件：{0}", file.FullName);
                return;
            }
            Console.WriteLine("正在处理：{0}", file.FullName);
            using (FileStream reader = file.OpenRead())
            {
                FileHeader header = new FileHeader();
                using (BinaryReader dataReader = new BinaryReader(reader))
                {
                    dataReader.ReadInt32();
                    header.Offset = DataConverter.BigEndian.GetUInt32(dataReader.ReadBytes(4), 0);
                    header.Length = DataConverter.BigEndian.GetUInt32(dataReader.ReadBytes(4), 0);
                    header.SentenceCount = DataConverter.BigEndian.GetUInt32(dataReader.ReadBytes(4), 0);

                    //读取文本数据
                    reader.Seek(header.Offset, SeekOrigin.Begin);
                    byte[] textData = new byte[header.Length];
                    reader.Read(textData, 0, textData.Length);

                    //处理文本
                    List<string> sentence = new List<string>();
                    List<byte> singleSentence = new List<byte>();
                    for (int i = 0; i < textData.Length; i++)
                    {
                        if (textData[i] != 0)
                        {
                            singleSentence.Add(textData[i]);
                        }
                        else
                        {
                            sentence.Add(Encoding.UTF8.GetString(singleSentence.ToArray()));
                            singleSentence.Clear();
                        }
                    }

                    //输出文本
                    string newFileName = file.FullName + ".txt";
                    File.WriteAllLines(newFileName, sentence.ToArray());
                }
            }
        }
    }

    public struct FileHeader
    {
        /// <summary>
        /// 文本偏移地址
        /// </summary>
        public uint Offset { get; set; }

        /// <summary>
        /// 文本长度
        /// </summary>
        public uint Length { get; set; }

        /// <summary>
        /// 句子的数量
        /// </summary>
        public uint SentenceCount { get; set; }
    }
}
