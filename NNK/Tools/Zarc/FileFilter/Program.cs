using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FileFilter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                PrintUsage();
                return;
            }

            DirectoryInfo directory = new DirectoryInfo(args[1]);
            if (!directory.Exists)
            {
                Console.WriteLine("指定的路径不存在");
                return;
            }

            string[] files = GetAllFiles(directory);

            //验证文件名称是否符合要求，如果不符合，则删除
            foreach (string single in files)
            {
                FileInfo file = new FileInfo(single);
                if (!Regex.IsMatch(file.Name, args[0]))
                {
                    Console.WriteLine("删除文件：{0}", file.FullName);
                    file.Delete();
                }
            }

            DeleteEmptyDirectory(directory);

            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("FileFilter.exe [regex] [path]");
            Console.WriteLine("regex:正则表达式，path：要过滤的路径");
            Console.WriteLine("例子：FileFilter _text_ja data，表示保留文件名中包含_text_ja的文件，其他文件则删除");
        }

        /// <summary>
        /// 删除空文件夹
        /// </summary>
        /// <param name="directory"></param>
        static void DeleteEmptyDirectory(DirectoryInfo directory)
        {
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    DeleteEmptyDirectory(single);
                }

                if (directory.GetFiles().Length == 0 && directory.GetDirectories().Length == 0)
                {
                    directory.Delete();
                }
            }
            else
            {
                if (directory.GetFiles().Length == 0 && directory.GetDirectories().Length == 0)
                {
                    directory.Delete();
                }
            }
        }

        /// <summary>
        /// 获取指定目录下的所有文件
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        static string[] GetAllFiles(DirectoryInfo directory)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    string[] files = GetAllFiles(single);
                    allFiles.AddRange(files);
                }
                FileInfo[] fileInfos = directory.GetFiles();
                foreach (FileInfo file in fileInfos)
                {

                    allFiles.Add(file.FullName);

                }
                return allFiles.ToArray();
            }
            else
            {
                FileInfo[] files = directory.GetFiles();
                foreach (FileInfo file in files)
                {
                    allFiles.Add(file.FullName);
                }
                return allFiles.ToArray();
            }
        }
    }
}
