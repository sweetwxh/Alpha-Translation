using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WordCount
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                PrintUsage();
                return;
            }
           
            DirectoryInfo directory=new DirectoryInfo(args[0]);
            if (!directory.Exists)
            {
                Console.WriteLine("指定的目录不存在");
                return;
            }

            Counter c = new Counter();
            Console.WriteLine(c.CountWord(directory));

            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("WordCount.exe [path]");
            Console.WriteLine("path: 要统计的文本所在的路径，可以嵌套路径");
        }
    }
}
