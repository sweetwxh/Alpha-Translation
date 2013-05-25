#region 引用

using System.IO;

#endregion

namespace HexSearch
{
    /// <summary>
    ///     文件内容搜索，暂时未优化效率
    /// </summary>
    internal class FileDataSearch
    {
        /// <summary>
        ///     搜索指定文件的指定内容
        /// </summary>
        /// <param name="file">要搜索的文件</param>
        /// <param name="data">要搜索的内容</param>
        /// <returns>如果找到内容，则返回内容的索引，否则返回-1</returns>
        public long SearchFor(FileInfo file, byte[] data)
        {
            long foundCount = 0; //已经找到的计数器
            long foundIndex = -1; //匹配到数据的索引
            using (FileStream fileReader = file.OpenRead())
            {
                int currentData;
                while ((currentData = fileReader.ReadByte()) != -1)
                {
                    if (foundCount == data.Length) //已经找到完全相同的数据
                        break;

                    if (currentData == data[foundCount]) //找到相等的数据
                    {
                        if (foundCount == 0)
                        {
                            foundIndex = fileReader.Position - 1;
                        }
                        foundCount++;
                    }
                    else
                    {
                        if (foundCount > 0)
                        {
                            if (foundIndex + data.Length >= fileReader.Length)
                            {
                                foundIndex = -1;
                                break;
                            }
                            //还原数据
                            foundCount = 0;
                            fileReader.Seek(foundIndex + 1, SeekOrigin.Begin);
                            foundIndex = -1;
                        }
                        else
                        {
                            if (fileReader.Position + data.Length > fileReader.Length)
                                break;
                        }
                    }
                }
            }
            return foundIndex;
        }
    }
}