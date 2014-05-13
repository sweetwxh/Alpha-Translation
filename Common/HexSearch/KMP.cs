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

            int currentData;
            while ((currentData = source.ReadByte()) != -1)
            {
                if (currentData == pattern[k])
                {
                    k++;
                }
                else
                {
                    if (next[k] != -1)
                    {
                        k = next[k];
                    }
                    else
                    {
                        k = 0;
                    }
                }

                if (k == pattern.Length)
                {
                    result.Add(i - (pattern.Length - 1));
                    k = 0;
                }
                ++i;
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
            int k = 0;
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == pattern[k])
                {
                    k++;
                }
                else
                {
                    if (next[k] != -1)
                    {
                        k = next[k];
                    }
                    else
                    {
                        k = 0;
                    }
                }

                if (k == pattern.Length)
                {
                    //找到匹配了
                    result.Add(i - (pattern.Length - 1));
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
                if (k != -1 && pattern[k] != pattern[j])
                    k = next[k];
                ++j;
                ++k;
                if (pattern[k] == pattern[j])
                    next[j] = next[k];
                else
                    next[j] = k;
            }
            return next;
        }
    }
}