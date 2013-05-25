using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FontCreator
{
    class FontConfig
    {
        public FontConfig()
        {
            Chr = new List<CHR>();
            Kern = new List<KERN>();
        }


        /// <summary>
        /// 输出为bin文件
        /// </summary>
        /// <param name="name"></param>
        public void ToBin(string name)
        {
            if (Inf == null)
            {
                Console.WriteLine("文件缺少INF段，无法生成bin文件");
                return;
            }

            if (KernInfo == null)
            {
                Console.WriteLine("文件缺少KERNINF段，无法生成bin文件");
                return;
            }

            using (MemoryStream fileMemory = new MemoryStream())
            {
                using (MemoryStream bodyMemory = new MemoryStream())
                {
                    //写入INF段
                    bodyMemory.Write(INF.Identifier, 0, INF.Identifier.Length);
                    bodyMemory.Write(DataConverter.BigEndian.GetBytes(Inf.TileHeight), 0, 4);
                    bodyMemory.Write(DataConverter.BigEndian.GetBytes(Inf.Number), 0, 4);
                    bodyMemory.Write(DataConverter.BigEndian.GetBytes(Inf.Width), 0, 4);
                    bodyMemory.Write(DataConverter.BigEndian.GetBytes(Inf.Height), 0, 4);
                    bodyMemory.Write(DataConverter.BigEndian.GetBytes(Inf.CS), 0, 4);

                    //循环写入CHR

                    foreach (CHR c in Chr)
                    {
                        bodyMemory.Write(CHR.Identifier, 0, CHR.Identifier.Length);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.SJS), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.UTF8), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.X), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.Y), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.Width), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.Unknown1), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.Unknown2), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(c.Channel), 0, 4);
                    }

                    //写入KERNINF
                    bodyMemory.Write(KERNINF.Identifier, 0, KERNINF.Identifier.Length);
                    bodyMemory.Write(DataConverter.BigEndian.GetBytes(KernInfo.Number), 0, 4);

                    //循环写入KERN
                    foreach (KERN k in Kern)
                    {
                        bodyMemory.Write(KERN.Identifier, 0, KERN.Identifier.Length);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(k.Unknown1), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(k.Unknown2), 0, 4);
                        bodyMemory.Write(DataConverter.BigEndian.GetBytes(k.Unknown3), 0, 4);
                    }

                    //写入End
                    bodyMemory.Write(END.Identifier, 0, END.Identifier.Length);

                    //判断是否需要补FF
                    int ffCount = 0;
                    if (bodyMemory.Length % 16 != 0)
                    {
                        ffCount = (int)((bodyMemory.Length / 16 + 1) * 16 - bodyMemory.Length);
                    }

                    if (ffCount > 0)
                    {
                        for (int i = 0; i < ffCount; i++)
                        {
                            bodyMemory.WriteByte(0xff);
                        }
                    }

                    int totalIdentifier = Kern.Count + Chr.Count + 3;
                    EndOffset = (uint)(bodyMemory.Length + 0x10);

                    fileMemory.Write(DataConverter.BigEndian.GetBytes(totalIdentifier), 0, 4);
                    fileMemory.Write(DataConverter.BigEndian.GetBytes(EndOffset), 0, 4);
                    fileMemory.Write(new byte[8], 0, 8);
                    fileMemory.Write(bodyMemory.ToArray(), 0, (int)bodyMemory.Length);
                }

                //写入结尾段落1
                if (Kern.Count == 0)
                {
                    fileMemory.Write(EndWithoutKERN, 0, EndWithoutKERN.Length);
                }
                else
                {
                    fileMemory.Write(EndWithKERN, 0, EndWithKERN.Length);
                }

                //写入段落2
                int endCount = 0;
                byte[] dataToWrite = Encoding.UTF8.GetBytes(INF.Name);
                fileMemory.Write(dataToWrite, 0, dataToWrite.Length);
                fileMemory.WriteByte(0);
                endCount += (dataToWrite.Length + 1);

                dataToWrite = Encoding.UTF8.GetBytes(CHR.Name);
                fileMemory.Write(dataToWrite, 0, dataToWrite.Length);
                fileMemory.WriteByte(0);
                endCount += (dataToWrite.Length + 1);

                dataToWrite = Encoding.UTF8.GetBytes(KERNINF.Name);
                fileMemory.Write(dataToWrite, 0, dataToWrite.Length);
                fileMemory.WriteByte(0);
                endCount += (dataToWrite.Length + 1);

                if (Kern.Count != 0)
                {
                    dataToWrite = Encoding.UTF8.GetBytes(KERN.Name);
                    fileMemory.Write(dataToWrite, 0, dataToWrite.Length);
                    fileMemory.WriteByte(0);
                    endCount += (dataToWrite.Length + 1);
                }

                dataToWrite = Encoding.UTF8.GetBytes(END.Name);
                fileMemory.Write(dataToWrite, 0, dataToWrite.Length);
                fileMemory.WriteByte(0);
                endCount += (dataToWrite.Length + 1);

                int endFF = 0;
                if (endCount % 16 != 0)
                {
                    endFF = (endCount / 16 + 1) * 16 - endCount;
                }

                if (endFF != 0)
                {
                    for (int i = 0; i < endFF; i++)
                    {
                        fileMemory.WriteByte(0xff);
                    }
                }

                //最后写入结尾
                fileMemory.Write(EndData, 0, EndData.Length);
                File.WriteAllBytes(name, fileMemory.ToArray());
            }
        }

        /// <summary>
        /// 输出为cfg文件
        /// </summary>
        /// <param name="name"></param>
        public void ToCfg(string name)
        {
            if (Inf == null)
            {
                Console.WriteLine("文件缺少INF段，无法生成bin文件");
                return;
            }

            if (KernInfo == null)
            {
                Console.WriteLine("文件缺少KERNINF段，无法生成bin文件");
                return;
            }

            List<string> txts = new List<string>();
            txts.Add(string.Format("INF {0},{1},{2},{3},{4}; // H,NUM,TW,TH,CS", Inf.TileHeight, Inf.Number, Inf.Width, Inf.Height, Inf.CS));
            txts.Add("");
            //循环写入CHR
            foreach (CHR c in Chr)
            {
                txts.Add(string.Format("CHR{0,6},{1,10},{2,4},{3,4},{4,2},{5,2},{6,2},{7}; // {8} {9}",
                    c.SJS, c.UTF8, c.X, c.Y, c.Width, c.Unknown1, c.Unknown2, c.Channel, GetUTF8XCode(c.UTF8), GetUnicodeXCode(GetChar(c.UTF8))));
            }
            txts.Add("");
            txts.Add(string.Format("KERNINF {0};", KernInfo.Number));
            if (Kern.Count != 0)
            {
                //循环写入Kern
                foreach (KERN k in Kern)
                {
                    if (k.Unknown3 < 0)
                    {
                        txts.Add(string.Format("KERN{0,9},{1,9},{2,3};", k.Unknown1, k.Unknown2, k.Unknown3));
                    }
                    else
                    {
                        txts.Add(string.Format("KERN{0,9},{1,9},{2,2};", k.Unknown1, k.Unknown2, k.Unknown3));
                    }
                }
            }
            txts.Add("");
            txts.Add("END;");
            File.WriteAllLines(name, txts.ToArray());
        }

        /// <summary>
        /// 从bin文件读取数据构造FontConfig
        /// </summary>
        /// <param name="file"></param>
        public void FromBin(FileInfo file)
        {
            using (FileStream stream = file.OpenRead())
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    IdentifierCount = DataConverter.BigEndian.GetUInt32(reader.ReadBytes(4), 0);
                    EndOffset = DataConverter.BigEndian.GetUInt32(reader.ReadBytes(4), 0);
                    stream.Seek(0x10, SeekOrigin.Begin);
                    //INF是肯定存在的
                    byte[] id = reader.ReadBytes(8);
                    while (!ByteCompare(END.Identifier, id))
                    {
                        switch (GetId(id))
                        {
                            case CurrentId.INF:
                                Inf = new INF
                                {
                                    TileHeight = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Number = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Width = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Height = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    CS = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0)
                                };
                                break;
                            case CurrentId.CHR:
                                CHR c = new CHR
                                {
                                    SJS = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    UTF8 = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    X = DataConverter.BigEndian.GetUInt32(reader.ReadBytes(4), 0),
                                    Y = DataConverter.BigEndian.GetUInt32(reader.ReadBytes(4), 0),
                                    Width = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Unknown1 = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Unknown2 = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Channel = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0)
                                };
                                Chr.Add(c);
                                break;
                            case CurrentId.KERNINF:
                                KernInfo = new KERNINF
                                {
                                    Number = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0)
                                };
                                break;
                            case CurrentId.KERN:
                                KERN k = new KERN
                                {
                                    Unknown1 = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Unknown2 = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0),
                                    Unknown3 = DataConverter.BigEndian.GetInt32(reader.ReadBytes(4), 0)
                                };
                                Kern.Add(k);
                                break;
                            case CurrentId.END:
                                break;
                        }
                        id = reader.ReadBytes(8);
                    }
                }
            }
        }

        /// <summary>
        /// 从cfg文件读取数据构造FontConfig
        /// </summary>
        /// <param name="file"></param>
        public void FromCfg(FileInfo file)
        {
            string[] allLines = File.ReadAllLines(file.FullName);
            foreach (string line in allLines)
            {
                if (string.IsNullOrEmpty(line.Trim()))//不处理空行
                    continue;

                string temp = line;
                temp = temp.Substring(0, temp.LastIndexOf(";"));
                string[] words = temp.Split(new char[] { ',' });
                switch (GetId(line))
                {
                    case CurrentId.INF:
                        words[0] = words[0].Replace(INF.Name, "");
                        Inf = new INF
                        {
                            TileHeight = int.Parse(words[0].Trim()),
                            Number = int.Parse(words[1].Trim()),
                            Width = int.Parse(words[2].Trim()),
                            Height = int.Parse(words[3].Trim()),
                            CS = int.Parse(words[4].Trim())
                        };
                        break;
                    case CurrentId.CHR:
                        words[0] = words[0].Replace(CHR.Name, "");
                        CHR c = new CHR
                        {
                            SJS = int.Parse(words[0].Trim()),
                            UTF8 = int.Parse(words[1].Trim()),
                            X = uint.Parse(words[2].Trim()),
                            Y = uint.Parse(words[3].Trim()),
                            Width = int.Parse(words[4].Trim()),
                            Unknown1 = int.Parse(words[5].Trim()),
                            Unknown2 = int.Parse(words[6].Trim()),
                            Channel = int.Parse(words[7].Trim())
                        };
                        Chr.Add(c);
                        break;
                    case CurrentId.KERNINF:
                        words[0] = words[0].Replace(KERNINF.Name, "");
                        KernInfo = new KERNINF
                        {
                            Number = int.Parse(words[0].Trim())
                        };
                        break;
                    case CurrentId.KERN:
                        words[0] = words[0].Replace(KERN.Name, "");
                        KERN k = new KERN
                        {
                            Unknown1 = int.Parse(words[0].Trim()),
                            Unknown2 = int.Parse(words[1].Trim()),
                            Unknown3 = int.Parse(words[2].Trim())
                        };
                        Kern.Add(k);
                        break;
                    case CurrentId.END:
                        break;

                }
            }
        }

        /// <summary>
        /// 根据Int32获取UTF8对应的字符
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetChar(int code)
        {
            return Encoding.UTF8.GetString(RemoveEmptyStart(DataConverter.BigEndian.GetBytes(code)));
        }

        /// <summary>
        /// 根据Int32获取UTF8对应的16进制
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetUTF8XCode(int code)
        {
            return string.Format("0x{0:x}", code);
        }

        /// <summary>
        /// 根据字符获得对应的UnicodeBE(1201)的16进制
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public string GetUnicodeXCode(string c)
        {
            byte[] data = Encoding.GetEncoding(1201).GetBytes(c);
            return string.Format("0x{0:x}", DataConverter.BigEndian.GetInt16(data, 0));
        }

        public static byte[] RemoveEmptyStart(byte[] source)
        {
            int nonZeroIndex = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != 0)
                {
                    nonZeroIndex = i;
                    break;
                }
            }
            byte[] rtv = new byte[source.Length - nonZeroIndex];
            Array.Copy(source, nonZeroIndex, rtv, 0, rtv.Length);
            return rtv;
        }

        public CurrentId GetId(string line)
        {
            if (line.Trim().StartsWith("INF"))
                return CurrentId.INF;

            if (line.Trim().StartsWith("CHR"))
                return CurrentId.CHR;

            if (line.Trim().StartsWith("KERNINF"))
                return CurrentId.KERNINF;

            if (line.Trim().StartsWith("KERN"))
                return CurrentId.KERN;

            return CurrentId.END;
        }

        public CurrentId GetId(byte[] id)
        {
            if (ByteCompare(INF.Identifier, id))
                return CurrentId.INF;

            if (ByteCompare(CHR.Identifier, id))
                return CurrentId.CHR;

            if (ByteCompare(KERNINF.Identifier, id))
                return CurrentId.KERNINF;

            if (ByteCompare(KERN.Identifier, id))
                return CurrentId.KERN;

            return CurrentId.END;
        }


        /// <summary>
        /// 比较两个Byte数组是否相等
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool ByteCompare(byte[] source, byte[] target)
        {
            bool equal = true;
            if (source.Length != target.Length)
                return false;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] != target[i])
                {
                    equal = false;
                    break;
                }
            }
            return equal;
        }

        public INF Inf { get; set; }

        public List<CHR> Chr { get; set; }

        public KERNINF KernInfo { get; set; }

        public List<KERN> Kern { get; set; }

        public END End { get; set; }

        /// <summary>
        /// 标识个数，0x0 4
        /// </summary>
        public uint IdentifierCount { get; set; }

        /// <summary>
        /// 结尾的偏移地址，0x4 4
        /// </summary>
        public uint EndOffset { get; set; }

        public byte[] EndWithKERN
        {
            get
            {
                return new byte[] {0x00,0x00,0x00,0x60,0x00,0x00,0x00,0x05,0x00,0x00,0x00,0x40,0x00,0x00,0x00,0x19,
                                    0x8E,0xB0,0x16,0x8C,0x00,0x00,0x00,0x00,0xCF,0xA7,0xE0,0xA1,0x00,0x00,0x00,0x04,
                                    0xCC,0x1C,0x19,0x19,0x00,0x00,0x00,0x08,0xFD,0x16,0xE3,0xB5,0x00,0x00,0x00,0x10,
                                    0x69,0xA4,0x8E,0xC4,0x00,0x00,0x00,0x15,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF};
            }
        }

        public byte[] EndWithoutKERN
        {
            get
            {
                return new byte[] {0x00,0x00,0x00,0x50,0x00,0x00,0x00,0x04,0x00,0x00,0x00,0x30,0x00,0x00,0x00,0x14,
                                    0x8E,0xB0,0x16,0x8C,0x00,0x00,0x00,0x00,0xCF,0xA7,0xE0,0xA1,0x00,0x00,0x00,0x04,
                                    0xCC,0x1C,0x19,0x19,0x00,0x00,0x00,0x08,0x69,0xA4,0x8E,0xC4,0x00,0x00,0x00,0x10 };
            }
        }

        public byte[] EndData
        {
            get
            {
                return new byte[] { 0x01, 0x74, 0x32, 0x62, 0xFE, 0x01, 0x00, 0x01, 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
            }
        }

        public enum CurrentId
        {
            INF, CHR, KERNINF, KERN, END
        }

    }

    #region 5种标识
    class INF
    {
        public static byte[] Identifier
        {
            get
            {
                return new byte[] { 0x8E, 0xB0, 0x16, 0x8C, 0x05, 0x55, 0x01, 0xFF };
            }
        }

        public static string Name
        {
            get
            {
                return "INF";
            }
        }

        /// <summary>
        /// H
        /// </summary>
        public int TileHeight { get; set; }

        /// <summary>
        /// NUM
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// TW
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// TH
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// CS
        /// </summary>
        public int CS { get; set; }
    }

    class CHR
    {
        public static byte[] Identifier
        {
            get
            {
                return new byte[] { 0xCF, 0xA7, 0xE0, 0xA1, 0x08, 0x55, 0x55, 0xFF };
            }
        }

        public static string Name
        {
            get
            {
                return "CHR";
            }
        }

        public int SJS { get; set; }

        public int UTF8 { get; set; }

        public uint X { get; set; }

        public uint Y { get; set; }

        public int Width { get; set; }

        public int Unknown1 { get; set; }

        public int Unknown2 { get; set; }

        public int Channel { get; set; }
    }

    class KERNINF
    {
        public static byte[] Identifier
        {
            get
            {
                return new byte[] { 0xCC, 0x1C, 0x19, 0x19, 0x01, 0x01, 0xFF, 0xFF };
            }
        }

        public static string Name
        {
            get
            {
                return "KERNINF";
            }
        }

        public int Number { get; set; }
    }



    class KERN
    {
        public static byte[] Identifier
        {
            get
            {
                return new byte[] { 0xFD, 0x16, 0xE3, 0xB5, 0x03, 0x15, 0xFF, 0xFF };
            }
        }

        public static string Name
        {
            get
            {
                return "KERN";
            }
        }

        public int Unknown1 { get; set; }

        public int Unknown2 { get; set; }

        public int Unknown3 { get; set; }
    }

    class END
    {
        public static byte[] Identifier
        {
            get
            {
                return new byte[] { 0x69, 0xA4, 0x8E, 0xC4, 0x00, 0xFF, 0xFF, 0xFF };
            }
        }

        public static string Name
        {
            get
            {
                return "END";
            }
        }
    }
    #endregion

}
