using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VFSExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("开始处理");
            using (FileStream reader = File.OpenRead("data.vfs"))
            {
                Unpack unpack = new Unpack(reader);
                unpack.Extract();
            }
            Console.WriteLine("处理完成，按任意键退出");
            Console.ReadKey();
        }
    }
}
