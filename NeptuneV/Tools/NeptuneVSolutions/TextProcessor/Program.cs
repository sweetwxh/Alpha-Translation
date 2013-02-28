using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextProcessor
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

            if (!Directory.Exists(args[1]))
            {
                Console.WriteLine("指定的路径不存在");
                return;
            }
            string[] all = GetAllCL3Files(new DirectoryInfo(args[1]));

            switch (args[0].ToLower())
            {
                case "e":
                    foreach (var es in all)
                    {
                        MainProcessor proc = new MainProcessor(new FileInfo(es), args[2].ToLower().Equals("s"));
                        proc.ExtractText();
                    }
                    break;
                case "i":
                    foreach (var ims in all)
                    {
                        MainProcessor proc = new MainProcessor(new FileInfo(ims), args[2].ToLower().Equals("s"));
                        proc.ImportText();
                    }
                    break;
            }

            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("NeptuneV Event 文本导出工具（测试版），正式版需要自制码表");
            Console.WriteLine("TextProcessor.exe [i|e] [path] [s|o]");
            Console.WriteLine("例子：TextProcessor.exe i Game s");
            Console.WriteLine("其中e表示导出文本，i表示导入文本，s表示静默，不输出当前正在处理的文件的状态(方便导入时查看错误信息)");
        }

        static string[] GetAllCL3Files(DirectoryInfo directory)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    string[] files = GetAllCL3Files(single);
                    allFiles.AddRange(files);
                }
                FileInfo[] fileInfos = directory.GetFiles();
                allFiles.AddRange(from file in fileInfos where file.Extension.ToLower().Equals(".cl3") select file.FullName);
                return allFiles.ToArray();
            }
            else
            {
                FileInfo[] files = directory.GetFiles();
                allFiles.AddRange(from file in files where file.Extension.ToLower().Equals(".cl3") select file.FullName);
                return allFiles.ToArray();
            }
        }
    }
}
