using System.Collections.Generic;
using System.IO;

namespace HexSearch
{
    public class KMP
    {
        public static long[] Match(Stream source, byte[] pattern, int[] next)
        {
            var result = new List<long>();
            int i = 0, k = 0;
            int currentData = source.ReadByte();
            while (i < source.Length && k < pattern.Length)
            {
                if (k == -1 || currentData == pattern[k])
                {
                    ++i;
                    ++k;
                    currentData = source.ReadByte();
                }
                else
                {
                    k = next[k];
                }

                if (k == pattern.Length)
                {
                    result.Add(i - pattern.Length);
                    k = 0;
                }
            }
            return result.ToArray();
        }

        /// <summary>
        ///     匹配模式
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pattern"></param>
        /// <param name="next"></param>
        /// <returns>所有匹配模式的索引</returns>
        public static long[] Match(byte[] source, byte[] pattern, int[] next)
        {
            var result = new List<long>();
            int i = 0;
            int k = 0;
            while (i < source.Length && k < pattern.Length)
            {
                if (k == -1 || source[i] == pattern[k])
                {
                    ++i;
                    ++k;
                }
                else
                {
                    k = next[k];
                }

                if (k == pattern.Length)
                {
                    //找到匹配了
                    result.Add(i - pattern.Length);
                    k = 0;
                }
            }
            return result.ToArray();
        }
    }

    public class Preprocess
    {
        /// <summary>
        ///     获取预处理的数组
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int[] Next(byte[] pattern)
        {
            var next = new int[pattern.Length];
            next[0] = -1;
            int j = 0, k = -1;
            while (j < pattern.Length - 1)
            {
                if (k == -1 || pattern[j] == pattern[k])
                {
                    ++j;
                    ++k;
                    if (pattern[j] != pattern[k])
                    {
                        next[j] = k;
                    }
                    else
                    {
                        next[j] = next[k];
                    }
                }
                else
                {
                    k = next[k];
                }
            }
            return next;
        }
    }
}