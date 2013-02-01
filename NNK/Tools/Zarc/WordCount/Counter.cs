using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WordCount
{
    class Counter
    {
        public int CountWord(DirectoryInfo directory)
        {
            Dictionary<string, string> wordList = new Dictionary<string, string>();
            string[] allFiles = GetPreparedFiles(directory);
            foreach (var single in allFiles)
            {
                string text = File.ReadAllText(single);
                for (int i = 0; i < text.Length; i++)
                {
                    string chr = text[i].ToString();
                    if (chr.Equals(Environment.NewLine))
                        continue;

                    if (!wordList.ContainsKey(chr))
                    {
                        wordList.Add(chr, chr);
                    }
                }
            }
            return wordList.Count;
        }

        private string[] GetPreparedFiles(DirectoryInfo directory)
        {
            List<string> allFiles = new List<string>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    string[] files = GetPreparedFiles(single);
                    allFiles.AddRange(files);
                }
                FileInfo[] fileInfos = directory.GetFiles();
                allFiles.AddRange(from file in fileInfos
                                  where
                                      file.Extension.ToLower().Equals(".txt") && !file.Name.Equals("files.txt")
                                  select file.FullName);
                return allFiles.ToArray();
            }
            else
            {
                FileInfo[] files = directory.GetFiles();
                allFiles.AddRange(from file in files
                                  where
                                      file.Extension.ToLower().Equals(".txt") && !file.Name.Equals("files.txt")
                                  select file.FullName);
                return allFiles.ToArray();
            }
        }
    }
}
