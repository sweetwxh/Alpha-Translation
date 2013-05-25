using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TextWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("使用说明：TextWrapper.exe [e]w [path]");
                Console.WriteLine("e：导出文本为txt\r\nw：将文件还原，w操作将删除导出的文本文件");
                Console.WriteLine("注意，指定目录一定要保证是通过Zarc解压并且通过FileFilter过滤后的文件");
                return;
            }

            bool isExtract = args[0].Trim().Equals("e");
            DirectoryInfo directory = new DirectoryInfo(args[1]);
            if (!directory.Exists)
            {
                Console.WriteLine("指定的目录不存在");
                return;
            }

            TextProcessor tp = new TextProcessor();
            if (isExtract)
            {
                //解压的话，需要获取所有文件，注意，这里一定要保证是通过Zarc解压并且通过FileFilter过滤后的文件
                string[] allFiles = GetAllFiles(directory);
                foreach (string file in allFiles)
                    tp.Extract(new FileInfo(file));
            }
            else
            {
                string[] justTxts = GetAllFiles(directory, true);
                foreach (string file in justTxts)
                    tp.Wrap(new FileInfo(file));
            }

            Console.WriteLine("处理完成\r\n请按任意键退出");
            Console.ReadKey();
        }

        /// <summary>
        /// 获取指定目录下的所有文件
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        static string[] GetAllFiles(DirectoryInfo directory, bool justTxt = false)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    string[] files = GetAllFiles(single, justTxt);
                    allFiles.AddRange(files);
                }
                FileInfo[] fileInfos = directory.GetFiles();
                foreach (FileInfo file in fileInfos)
                {
                    if (justTxt)
                    {
                        if (file.Extension.ToLower().Equals(".txt"))
                        {
                            allFiles.Add(file.FullName);
                        }
                    }
                    else
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
                    if (justTxt)
                    {
                        if (file.Extension.ToLower().Equals(".txt"))
                        {
                            allFiles.Add(file.FullName);
                        }
                    }
                    else
                    {
                        allFiles.Add(file.FullName);
                    }
                }
                return allFiles.ToArray();
            }
        }
    }


}
