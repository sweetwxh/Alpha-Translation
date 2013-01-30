using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HPKProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
            }

            Hpk hpk = new Hpk();
            switch (args[0].ToLower())
            {
                case "u":
                    FileInfo file = new FileInfo(args[1]);
                    hpk.Unpack(file);
                    break;
                case "p":
                    DirectoryInfo directory = new DirectoryInfo(args[1]);
                    hpk.Pack(directory);
                    break;
                default:
                    PrintUsage();
                    break;
            }
            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("使用说明：HPKProcessor.exe u[p] [file|path]");
            Console.WriteLine("u解包，p封包");
            Console.WriteLine("使用例子：HPKProcessor.exe u system_base.hpk");
        }
    }
}
