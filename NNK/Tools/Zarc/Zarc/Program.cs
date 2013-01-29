using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Zarc
{
    class Program
    {
        static void Main(string[] args)
        {
            //参数c表示压缩，d表示接压缩，r表示处理后移除原始文件
            if (args.Length != 2)
            {
                PrintUseage();
                return;
            }

            string option = args[0];
            string path = args[1];

            bool isCompress = option.Contains("c");
            bool removeFile = option.Contains("r");

            ZarcCompress zc = new ZarcCompress();

            bool isFile = File.Exists(path);
            bool isDirectory = false;
            if (!isFile)
                isDirectory = Directory.Exists(path);

            if (!isFile && !isDirectory)
            {
                Console.WriteLine("指定的路径或文件名不存在\r\n按任意键退出");
                Console.ReadKey();
                return;
            }

            if (isFile)
            {
                if (isCompress)
                {
                    zc.Zip(path, removeFile);
                }
                else
                {
                    zc.Unzip(path, removeFile);
                }
            }
            else
            {
                //批量处理
                DirectoryInfo directory = new DirectoryInfo(path);

                if (isCompress)
                {
                    string[] allFiles = GetAllPrepareFiles(directory);
                    foreach (string file in allFiles)
                    {
                        zc.Zip(file, removeFile);
                    }
                }
                else
                {
                    string[] allFiles = GetAllZarcFiles(directory);
                    foreach (string file in allFiles)
                    {
                        zc.Unzip(file, removeFile);
                    }
                }

            }

            Console.WriteLine("处理完毕\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUseage()
        {
            Console.WriteLine("使用说明：");
            Console.WriteLine("例子：Zarc.exe [c]dr Path|File");
            Console.WriteLine("c表示压缩，d表示解压缩，r表示处理完成后移除原始文件");
            Console.WriteLine("Path|File，可以为路径或文件名称，如果路径的话，则为批量处理");
        }

        static string[] GetAllPrepareFiles(DirectoryInfo directory)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    string[] files = GetAllPrepareFiles(single);
                    allFiles.AddRange(files);
                }

                string configFile = Path.Combine(directory.FullName, "files.txt");
                if (File.Exists(configFile))
                {
                    string[] txt = File.ReadAllLines(configFile);
                    foreach (string s in txt)
                    {
                        allFiles.Add(Path.Combine(directory.FullName, s));
                    }
                }

                File.Delete(configFile);

                return allFiles.ToArray();
            }
            else
            {
                string configFile = Path.Combine(directory.FullName, "files.txt");
                if (File.Exists(configFile))
                {
                    string[] txt = File.ReadAllLines(configFile);
                    foreach (string s in txt)
                    {
                        allFiles.Add(Path.Combine(directory.FullName, s));
                    }
                }

                File.Delete(configFile);

                return allFiles.ToArray();
            }
        }

        static string[] GetAllZarcFiles(DirectoryInfo directory)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    string[] files = GetAllZarcFiles(single);
                    allFiles.AddRange(files);
                }
                FileInfo[] fileInfos = directory.GetFiles();
                foreach (FileInfo file in fileInfos)
                {
                    if (file.Extension.ToLower().Equals(".zarc"))
                    {
                        allFiles.Add(file.FullName);
                    }
                }
                return allFiles.ToArray();
            }
            else
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Extension.ToLower().Equals(".zarc"))
                    {
                        allFiles.Add(file.FullName);
                    }
                }
                return allFiles.ToArray();
            }
        }
    }
}
