using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                PrintUsage();
                return;
            }

            if (!Directory.Exists(args[0]))
            {
                Console.WriteLine("指定from path不存在");
                return;
            }

            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("指定to path不存在");
                return;
            }

            if (!File.Exists(args[2]))
            {
                Console.WriteLine("指定的file list不存在");
                return;
            }

            string fromPath = args[0];
            string toPath = args[1];
            string fileList = args[2];

            string[] extractFiles = File.ReadAllLines(fileList);
            foreach (var single in extractFiles)
            {
                string realPath = Path.Combine(fromPath, single);
                if (File.Exists(realPath))
                {
                    Console.WriteLine("正在移动：{0}文件", realPath);
                    string toRealPath = Path.Combine(toPath, single);
                    string toPathDirectory = toRealPath.Substring(0, toRealPath.LastIndexOf("\\"));
                    if (!Directory.Exists(toPathDirectory))
                    {
                        Directory.CreateDirectory(toPathDirectory);
                    }
                    File.Copy(realPath, toRealPath, true);
                }
                else
                {
                    Console.WriteLine("文件：{0}不存在", realPath);
                }
            }

            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("FileExtract.exe [from path] [to path] [file list]");
            Console.WriteLine("from path:从哪个目录抽取");
            Console.WriteLine("to path:放到哪个目录下");
            Console.WriteLine("file list:需要抽取的文件列表");
            Console.WriteLine("Example:FileExtract.exe nico nico_modify translation-files.txt");
            Console.WriteLine("注意：上面的列子中，nico必须为psarc包的解压后的根路径");
        }
    }
}
