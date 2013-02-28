using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace TextProcessor
{
    class MainProcessor
    {
        private FileInfo fileToProc;

        public MainProcessor(FileInfo fileToProc, bool silent)
        {
            this.fileToProc = fileToProc;
            if (!silent)
                Console.WriteLine("开始处理：{0}", fileToProc.FullName);
        }

        public void ImportText()
        {
            if (!File.Exists(fileToProc.FullName + ".gbnb.xml"))
            {
                Console.WriteLine("文件：{0}不包含文本，不处理", fileToProc.FullName);
                return;
            }
            List<string> text = GetAllText();
            using (FileStream reader = new FileStream(fileToProc.FullName + ".gbnb", FileMode.Open, FileAccess.Read))
            {
                GBNB gbnb = GetGBNBInfo(reader);
                if (text.Count != gbnb.SentenceCount)
                {
                    Console.WriteLine("{0}的文本句子长度与原始文本长度不相等，无法处理", fileToProc.FullName + ".gbnb.txt");
                }
                else
                {
                    reader.Seek(0, SeekOrigin.Begin);
                    using (BinaryReader binReader = new BinaryReader(reader))
                    {
                        byte[] configData = binReader.ReadBytes(gbnb.TextOffset);
                        using (MemoryStream gbnbStream = new MemoryStream())
                        {
                            using (MemoryStream textStream = new MemoryStream())
                            {
                                int lastSentenceLength = 0;
                                for (int i = 0; i < gbnb.SentenceCount; i++)
                                {
                                    byte[] singleData = Encoding.GetEncoding(932).GetBytes(text[i]);//正式转换，这里需要用自制码表
                                    textStream.Write(singleData, 0, singleData.Length);
                                    textStream.WriteByte(0);

                                    if (i != 0)
                                    {
                                        //写入文本偏移位置
                                        byte[] offsetData = DataConverter.BigEndian.GetBytes(lastSentenceLength);
                                        Array.Copy(offsetData, 0, configData, i * 0x24 + 0x20, 4);
                                    }

                                    lastSentenceLength += (singleData.Length + 1);
                                }
                                //判断新生成的文本数据是否大于原始数据
                                if (textStream.Length > gbnb.TextDataLength)
                                {
                                    Console.WriteLine("{0}文件的文本数据长度大于原始文本数据长度，无法处理", fileToProc.FullName + ".gbnb.txt");
                                }
                                else
                                {
                                    int fixZero = (int)(gbnb.TextDataLength - textStream.Length);
                                    textStream.Write(new byte[fixZero], 0, fixZero);
                                }

                                //写入控制数据
                                gbnbStream.Write(configData, 0, configData.Length);

                                //写入文本数据
                                gbnbStream.Write(textStream.ToArray(), 0, (int)textStream.Length);

                                //写入GBNB数据
                                reader.Seek(-0x40, SeekOrigin.End);
                                gbnbStream.Write(binReader.ReadBytes(0x40), 0, 0x40);
                            }

                            string strOffset = File.ReadAllText(fileToProc.FullName + ".dat");
                            int offset = int.Parse(strOffset);
                            using (FileStream writer = fileToProc.OpenWrite())
                            {
                                writer.Seek(offset, SeekOrigin.Begin);
                                writer.Write(gbnbStream.ToArray(), 0, (int)gbnbStream.Length);
                            }
                        }
                    }
                }
            }

            //删除其他文件
            if (File.Exists(fileToProc.FullName + ".dat"))
            {
                File.Delete(fileToProc.FullName + ".dat");
            }

            if (File.Exists(fileToProc.FullName + ".gbnb"))
            {
                File.Delete(fileToProc.FullName + ".gbnb");
            }

            if (File.Exists(fileToProc.FullName + ".gbnb.xml"))
            {
                File.Delete(fileToProc.FullName + ".gbnb.xml");
            }
        }

        private List<string> GetAllText()
        {
            XDocument doc = XDocument.Load(fileToProc.FullName + ".gbnb.xml");
            return doc.Root.Elements("sentence").Select(element => element.Value).ToList();
        }

        private void WriteAllText(List<string> text, string path)
        {
            XDocument doc = new XDocument();
            var root = new XElement("text");

            foreach (var single in text)
            {
                root.Add(new XElement("sentence", single));
            }

            doc.Add(root);
            doc.Save(path);
        }

        /// <summary>
        /// 导出文本
        /// </summary>
        public void ExtractText()
        {
            string exportedFile = ExportGBNB(true);
            if (exportedFile.Equals(string.Empty))
            {
                Console.WriteLine("文件：{0}不包含文本", fileToProc.FullName);
                return;
            }
            List<string> text = new List<string>();
            using (FileStream reader = new FileStream(exportedFile, FileMode.Open, FileAccess.Read))
            {
                GBNB gbnb = GetGBNBInfo(reader);
                reader.Seek(gbnb.TextOffset, SeekOrigin.Begin);
                for (int i = 0; i < gbnb.SentenceCount; i++)
                {
                    List<byte> textData = new List<byte>();
                    byte singleData = (byte)reader.ReadByte();
                    while (singleData != 0)
                    {
                        textData.Add(singleData);
                        singleData = (byte)reader.ReadByte();
                    }
                    text.Add(Encoding.GetEncoding(932).GetString(textData.ToArray()));
                }
                WriteAllText(text, exportedFile + ".xml");
                //File.WriteAllLines(exportedFile + ".txt", text.ToArray(), Encoding.UTF8);
            }
        }

        private GBNB GetGBNBInfo(FileStream reader)
        {
            GBNB result = new GBNB();
            reader.Seek(reader.Length - 0x18, SeekOrigin.Begin);
            byte[] countData = new byte[4];
            reader.Read(countData, 0, 4);
            result.SentenceCount = DataConverter.BigEndian.GetInt32(countData, 0);
            byte[] offsetData = new byte[4];
            reader.Read(offsetData, 0, 4);
            result.TextOffset = DataConverter.BigEndian.GetInt32(offsetData, 0);
            result.TextDataLength = (int)reader.Length - result.TextOffset - 0x40;
            return result;
        }

        /// <summary>
        /// 导出GBNB段
        /// </summary>
        private string ExportGBNB(bool recordOffset)
        {
            using (FileStream reader = fileToProc.OpenRead())
            {
                uint offset = GetGlobalDataOffset(reader);
                using (BinaryReader binReader = new BinaryReader(reader))
                {
                    //跳过magic来处理数据
                    reader.Seek(offset + 0x10, SeekOrigin.Begin);

                    while (reader.Position < reader.Length)
                    {
                        byte[] sectionConfig = binReader.ReadBytes(0x10);
                        //判断是否为控制长度的config
                        ulong first = DataConverter.BigEndian.GetUInt64(sectionConfig, 0);
                        uint second = DataConverter.BigEndian.GetUInt32(sectionConfig, 0x8);

                        if (first != 1 || second != 1)
                        {
                            continue;
                        }
                        uint nextSectionLength = DataConverter.BigEndian.GetUInt32(sectionConfig, 0xc);
                        if (nextSectionLength != 4 && nextSectionLength > 0x40)
                        {
                            byte[] sectionData = binReader.ReadBytes((int)nextSectionLength);
                            //判断是否为GBNB
                            byte[] gbnbMagic = new byte[] { 0x47, 0x42, 0x4e, 0x42 };
                            byte[] readMagic = new byte[4];
                            Array.Copy(sectionData, sectionData.Length - 0x40, readMagic, 0, 4);
                            if (ByteCompare(gbnbMagic, readMagic))
                            {
                                File.WriteAllBytes(fileToProc.FullName + ".gbnb", sectionData);
                                if (recordOffset)
                                {
                                    //记录一下GBNB在CL3中的偏移位置
                                    int gbnbOffset = (int)reader.Position - sectionData.Length;
                                    File.WriteAllText(fileToProc.FullName + ".dat", gbnbOffset.ToString());
                                }
                                return fileToProc.FullName + ".gbnb";
                            }
                            //判断nextSectionData是否是16的整倍数
                            if (nextSectionLength % 16 != 0)
                            {
                                int skipData = (int)((nextSectionLength / 16 + 1) * 16 - nextSectionLength);
                                binReader.ReadBytes(skipData);
                            }
                        }
                    }
                    return string.Empty;
                }
            }
        }

        private bool ByteCompare(byte[] source, byte[] target)
        {
            if (source.Length != target.Length)
                return false;
            return !source.Where((t, i) => t != target[i]).Any();
        }

        private uint GetGlobalDataOffset(FileStream reader)
        {
            reader.Seek(0x304, SeekOrigin.Begin);
            byte[] offsetData = new byte[4];
            reader.Read(offsetData, 0, 4);
            return DataConverter.BigEndian.GetUInt32(offsetData, 0) + 0x150;
        }
    }

    class GBNB
    {
        public int SentenceCount { get; set; }

        public int TextOffset { get; set; }

        public int TextDataLength { get; set; }
    }
}
