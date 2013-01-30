using ICSharpCode.SharpZipLib.Zip;
using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TranslateTesting
{
    class Program
    {
        static void Main(string[] args)
        {
            //string s = "000000600000000500000040000000198EB0168C00000000CFA7E0A100000004CC1C191900000008FD16E3B50000001069A48EC400000015FFFFFFFFFFFFFFFF";
            //for (int i = 0; i < s.Length; i += 2)
            //{
            //    string output = s[i].ToString() + s[i + 1].ToString();
            //    Console.Write("0x{0},", output);
            //}
            string s = "奧巴還快點兒";
            foreach (char c in s)
            {
                byte[] b = Encoding.UTF8.GetBytes(c.ToString());
                byte[] n = new byte[4];
                Array.Copy(b, 0, n, 1, 3);
                int i = DataConverter.BigEndian.GetInt32(n, 0);

                b = Encoding.GetEncoding(932).GetBytes(c.ToString());
                ushort jis = DataConverter.BigEndian.GetUInt16(b, 0);
                Console.WriteLine("{0},{1},{2}", jis, i,c);
            }

            Console.ReadKey();
        }
    }
}
