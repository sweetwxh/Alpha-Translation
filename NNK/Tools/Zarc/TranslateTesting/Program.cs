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
            byte[] d = new byte[] { 0xe9,0xbc,0xbb };
            Console.WriteLine(Encoding.UTF8.GetString(d));

            Console.ReadKey();
        }
    }
}
