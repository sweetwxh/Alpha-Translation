using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OriginTranslationFiles
{
    class Program
    {
        static void Main(string[] args)
        {
            string strip = @"D:\GitHub\Alpha-Translation\NICO\Tools\NICOSolutions\OriginTranslationFiles\bin\Release\dds\";
            DirectoryInfo directory = new DirectoryInfo("dds");
            FileInfo[] all = GetAllFiles(directory);
            List<string> filepath = new List<string>();
            foreach (var single in all)
            {
                string path = single.FullName.Replace(strip, "").Replace(".dds", ".ctxr");
                filepath.Add(path);
            }
            File.WriteAllLines("dds-files.txt", filepath.ToArray());
            Console.WriteLine("Complete");
        }

        static FileInfo[] GetAllFiles(DirectoryInfo directory)
        {
            List<FileInfo> matchedFiles = new List<FileInfo>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    FileInfo[] files = GetAllFiles(single);
                    matchedFiles.AddRange(files);
                }

                FileInfo[] currentPathFiles = directory.GetFiles();
                matchedFiles.AddRange(currentPathFiles);
            }
            else
            {
                FileInfo[] files = directory.GetFiles();
                matchedFiles.AddRange(files);
            }

            return matchedFiles.ToArray();
        }
    }


}
