using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TableProduce
{
    class Program
    {
        static void Main(string[] args)
        {
            OriginTable();
            Console.WriteLine("处理完成");
            Console.ReadKey();
        }

        static void OriginTable()
        {
            List<CharInfo> charList = new List<CharInfo>();
            string[] allLines = File.ReadAllLines("ft_gaku.cfg");
            for (int i = 2; i < 7468; i++)
            {
                string single = allLines[i];
                single = single.Substring(0, single.LastIndexOf(";"));
                string[] line = single.Split(new char[] { ',' });
                line[0] = line[0].Replace("CHR", "");

                CharInfo c = new CharInfo
                {
                    ShiftJISCode = int.Parse(line[0].Trim()),
                    UTF8Code = int.Parse(line[1].Trim()),
                    X = int.Parse(line[2].Trim()),
                    Y = int.Parse(line[3].Trim()),
                    Width = int.Parse(line[4].Trim()),
                    TW = int.Parse(line[5].Trim()),
                    TH = int.Parse(line[6].Trim()),
                    CS = (Channel)int.Parse(line[7].Trim())
                };

                charList.Add(c);
            }

            List<string> writeTxt = new List<string>();
            foreach (CharInfo c in charList)
            {
                writeTxt.Add(string.Format("{0},{1},{2}", c.UTF8XCode, c.UTF8Code, c.Character));
            }

            File.WriteAllLines("origin_table.txt", writeTxt.ToArray());
        }
    }

    public struct CharInfo
    {
        public int ShiftJISCode { get; set; }

        public int UTF8Code { get; set; }

        public string Character
        {
            get
            {
                return Encoding.UTF8.GetString(Utils.RemoveEmptyStart(DataConverter.BigEndian.GetBytes(UTF8Code)));
            }
        }

        public string UTF8XCode
        {
            get
            {
                return string.Format("0x{0:x}", UTF8Code);
            }
        }

        public string UnicodeXCode
        {
            get
            {
                byte[] data = Encoding.GetEncoding(1201).GetBytes(Character);
                return string.Format("0x{0:x}", DataConverter.BigEndian.GetInt16(data, 0));
            }
        }

        public int X { get; set; }

        public int Y { get; set; }

        public int Width { get; set; }

        public int TW { get; set; }

        public int TH { get; set; }

        /// <summary>
        /// 所在通道
        /// </summary>
        public Channel CS { get; set; }
    }

    public enum Channel
    {
        Blue = 0,
        Alpha = 1,
        Red = 2,
        Green = 3
    }

    public class Utils
    {
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
    }
}
