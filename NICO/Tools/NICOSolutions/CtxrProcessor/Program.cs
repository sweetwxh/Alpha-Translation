#region 引用

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#endregion

namespace CtxrProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            string option = args[0];
            string path = args[1];

            bool isFile = File.Exists(path);
            bool isDirectory = false;
            if (!isFile)
                isDirectory = Directory.Exists(path);

            if (!isFile && !isDirectory)
            {
                Console.WriteLine("指定的文件或路径不存在");
                Console.ReadKey();
                return;
            }

            Ctxr c = new Ctxr();
            if (isFile)
            {
                FileInfo file = new FileInfo(path);
                switch (option.ToLower())
                {
                    case "c2g":
                        c.ToGtf(file);
                        break;
                    case "g2c":
                        c.ToCtxr(file);
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }
            else
            {
                switch (option.ToLower())
                {
                    case "c2g":
                        string[] cFiles = GetAllFiles(new DirectoryInfo(path), ".ctxr");
                        foreach (string cSingle in cFiles)
                        {
                            FileInfo cFile = new FileInfo(cSingle);
                            c.ToGtf(cFile);
                        }
                        break;
                    case "g2c":
                        string[] gFiles = GetAllFiles(new DirectoryInfo(path), ".gtf");
                        foreach (string gSingle in gFiles)
                        {
                            FileInfo gFile = new FileInfo(gSingle);
                            c.ToCtxr(gFile);
                        }
                        break;
                    default:
                        PrintUsage();
                        break;
                }
            }

            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        private static void PrintUsage()
        {
            Console.WriteLine("CtxrProcessor.exe c2g[g2c] [file|path],(NICO专用版)");
            Console.WriteLine("c2g: ctxr转换为gtf\r\ng2c: gtf转换为ctxr");
            Console.WriteLine("可以指定文件或路径（指定路径为批量处理）");
        }

        private static string[] GetAllFiles(DirectoryInfo directory, string extension)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (string[] files in allDirectory.Select(single => GetAllFiles(single, extension)))
                {
                    allFiles.AddRange(files);
                }
                FileInfo[] fileInfos = directory.GetFiles();
                allFiles.AddRange(from file in fileInfos
                                  where file.Extension.ToLower().Equals(extension)
                                  select file.FullName);
                return allFiles.ToArray();
            }
            else
            {
                FileInfo[] files = directory.GetFiles();
                allFiles.AddRange(from file in files
                                  where file.Extension.ToLower().Equals(extension)
                                  select file.FullName);
                return allFiles.ToArray();
            }
        }
    }
}