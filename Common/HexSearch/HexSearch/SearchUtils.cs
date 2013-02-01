#region 引用

using System;
using System.Collections.Generic;
using System.IO;

#endregion

namespace HexSearch
{
    /// <summary>
    ///     16进制的辅助工具类
    /// </summary>
    internal class SearchUtils
    {
        /// <summary>
        ///     获取指定目录指定模式的所有文件
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static FileInfo[] GetSearchFiles(DirectoryInfo directory, string pattern)
        {
            List<FileInfo> matchedFiles = new List<FileInfo>();
            DirectoryInfo[] allDirectory = directory.GetDirectories();
            if (allDirectory.Length > 0)
            {
                foreach (DirectoryInfo single in allDirectory)
                {
                    FileInfo[] files = GetSearchFiles(single, pattern);
                    matchedFiles.AddRange(files);
                }

                FileInfo[] currentPathFiles = pattern.Trim().Equals(string.Empty)
                                                  ? directory.GetFiles()
                                                  : directory.GetFiles(pattern);
                matchedFiles.AddRange(currentPathFiles);
            }
            else
            {
                FileInfo[] files = pattern.Trim().Equals(string.Empty)
                                       ? directory.GetFiles()
                                       : directory.GetFiles(pattern);
                matchedFiles.AddRange(files);
            }

            return matchedFiles.ToArray();
        }

        /// <summary>
        ///     将16进制字符串转换为byte数组
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static byte[] HexStringToBytes(string hex)
        {
            if (hex.Length%2 != 0)
                throw new ArgumentException("16进制输入有误，无法转换");

            byte[] result = new byte[hex.Length >> 1];

            int halfLength = hex.Length >> 1;
            for (int i = 0; i < halfLength; ++i)
            {
                result[i] = (byte) ((HexToByte(hex[i << 1]) << 4) + (HexToByte(hex[(i << 1) + 1])));
            }
            return result;
        }

        private static int HexToByte(char hex)
        {
            int val = hex;
            return val - (val < 58 ? 48 : 55);
        }
    }
}