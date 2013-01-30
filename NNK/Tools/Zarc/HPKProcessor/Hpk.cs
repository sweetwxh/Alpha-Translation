using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HPKProcessor
{
    /************************************************************
     * 处理HPK要注意的地方：
     * 1.解压后生成一个控制文件，方便打包，控制文件内容为原始的BaseOffset之前的所有数据，在打包时，重新生成每个文件的偏移位置
     * 2.现在无法确定控制每个文件的Hash是否会因为文件修改而出现异常，每个文件的控制数据最后4位意义未知，也按照原始输出（估计是文件名的校验码）
     * **********************************************************/
    class Hpk
    {
        public void Unpack(FileInfo file)
        {
            //将解包出来的文件全部放入这个basePath下
            string basePath = file.Name.Replace(file.Extension, "");

            HpkFile hpkFile = null;
            List<FileData> files = null;
            if (ReadHeader(file, out hpkFile, out files))
            {
                using (FileStream fileReader = file.OpenRead())
                {
                    foreach (var single in files)
                    {
                        uint filePos = single.Offset + hpkFile.BaseOffset;
                        fileReader.Seek(filePos, SeekOrigin.Begin);
                        byte[] data = new byte[single.Size];
                        fileReader.Read(data, 0, (int)single.Size);

                        string savePath = Path.Combine(basePath, single.FilePath);
                        string tempPath = savePath.Substring(0, savePath.LastIndexOf("/"));
                        if (!Directory.Exists(tempPath))
                        {
                            Directory.CreateDirectory(tempPath);
                        }
                        File.WriteAllBytes(savePath, data);
                    }

                    //完成之后，将现有头文件写入解压的根目录
                    fileReader.Seek(0, SeekOrigin.Begin);
                    byte[] header = new byte[hpkFile.BaseOffset];
                    fileReader.Read(header, 0, (int)hpkFile.BaseOffset);
                    File.WriteAllBytes(Path.Combine(basePath, "record.dat"), header);
                }
            }
        }

        public void Pack(DirectoryInfo directory)
        {
            string basePath = directory.Name;

            HpkFile hpkFile = null;
            List<FileData> files = null;

            FileInfo recordFile = new FileInfo(Path.Combine(basePath, "record.dat"));
            if (ReadHeader(recordFile, out hpkFile, out files))
            {
                using (MemoryStream allMemory = new MemoryStream())
                {
                    using (MemoryStream fileMemory = new MemoryStream())
                    {
                        //遍历文件进行读取并写入内存
                        bool isFirst = true;
                        for (int i = 0; i < files.Count; i++)
                        {
                            string savePath = Path.Combine(basePath, files[i].FilePath);
                            byte[] data = File.ReadAllBytes(savePath);
                            fileMemory.Write(data, 0, data.Length);

                            //修改FileData记录的文件大小
                            files[i].Size = (uint)data.Length;
                            //修改偏移位置
                            if (isFirst)
                            {
                                files[i].Offset = 0;
                                isFirst = false;
                            }
                            else
                            {
                                files[i].Offset = files[i - 1].Size + files[i - 1].Offset;
                            }
                        }

                        //开始建立头文件
                        allMemory.Write(hpkFile.Magic, 0, 4);
                        allMemory.Write(DataConverter.BigEndian.GetBytes(hpkFile.FileCount), 0, 4);
                        uint compressedSize = (uint)(hpkFile.BaseOffset + fileMemory.Length);
                        allMemory.Write(DataConverter.BigEndian.GetBytes(compressedSize), 0, 4);
                        allMemory.Write(DataConverter.BigEndian.GetBytes(hpkFile.NameEnd), 0, 4);
                        allMemory.Write(DataConverter.BigEndian.GetBytes(hpkFile.NameStart), 0, 4);
                        allMemory.Write(DataConverter.BigEndian.GetBytes(hpkFile.BaseOffset), 0, 4);
                        allMemory.Write(DataConverter.BigEndian.GetBytes(hpkFile.Version), 0, 4);
                        allMemory.Write(new byte[4], 0, 4);

                        //遍历写入文件控制信息
                        foreach (var single in files)
                        {
                            allMemory.Write(single.HashData, 0, 0x10);
                            allMemory.Write(DataConverter.BigEndian.GetBytes(single.Offset), 0, 4);
                            allMemory.Write(DataConverter.BigEndian.GetBytes(single.Size), 0, 4);
                            allMemory.Write(DataConverter.BigEndian.GetBytes(single.NameOffset), 0, 4);
                            allMemory.Write(single.Unknown, 0, 4);
                        }

                        //写入文件名称
                        foreach (var single in files)
                        {
                            byte[] pathData = Encoding.UTF8.GetBytes(single.FilePath);
                            allMemory.Write(pathData, 0, pathData.Length);
                            allMemory.WriteByte(0);
                        }

                        //补0
                        int zeroCount = (int)(hpkFile.BaseOffset - allMemory.Length);
                        for (int i = 0; i < zeroCount; i++)
                        {
                            allMemory.WriteByte(0);
                        }

                        //最后写入文件数据
                        allMemory.Write(fileMemory.ToArray(), 0, (int)fileMemory.Length);
                        File.WriteAllBytes(basePath + ".hpk", allMemory.ToArray());
                    }
                }
            }
        }

        /// <summary>
        /// 读取HPK头文件并
        /// </summary>
        /// <param name="file"></param>
        /// <param name="hpkFile"></param>
        /// <param name="files"></param>
        /// <returns>如果成功，则返回true</returns>
        private bool ReadHeader(FileInfo file, out HpkFile hpkFile, out List<FileData> files)
        {
            hpkFile = new HpkFile();
            files = new List<FileData>();
            using (FileStream fileReader = file.OpenRead())
            {
                using (BinaryReader binReader = new BinaryReader(fileReader))
                {
                    byte[] magic = binReader.ReadBytes(4);
                    if (!ByteCompare(magic, hpkFile.Magic))
                    {
                        Console.WriteLine("所选文件不是HPK文件");
                        return false;
                    }
                    hpkFile.FileCount = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                    hpkFile.CompressedSize = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                    hpkFile.NameEnd = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                    hpkFile.NameStart = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                    hpkFile.BaseOffset = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                    hpkFile.Version = DataConverter.BigEndian.GetInt32(binReader.ReadBytes(4), 0);


                    //遍历读取每个文件的控制信息
                    binReader.ReadBytes(4);//跳过4个空的数据
                    for (int i = 0; i < hpkFile.FileCount; i++)
                    {
                        FileData fileData = new FileData();
                        fileData.HashData = binReader.ReadBytes(0x10);
                        fileData.Offset = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                        fileData.Size = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                        fileData.NameOffset = DataConverter.BigEndian.GetUInt32(binReader.ReadBytes(4), 0);
                        fileData.Unknown = binReader.ReadBytes(4);
                        files.Add(fileData);
                        long currentPosition = fileReader.Position;//记录当前位置
                        uint namePosition = hpkFile.NameStart + fileData.NameOffset;
                        fileReader.Seek(namePosition, SeekOrigin.Begin);
                        List<byte> nameData = new List<byte>();
                        while (binReader.PeekChar() != 0)
                        {
                            nameData.Add(binReader.ReadByte());
                        }
                        fileData.FilePath = Encoding.UTF8.GetString(nameData.ToArray());
                        //还原位置
                        fileReader.Seek(currentPosition, SeekOrigin.Begin);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个Byte数组是否相等
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool ByteCompare(byte[] source, byte[] target)
        {
            if (source.Length != target.Length)
                return false;
            return !source.Where((t, i) => t != target[i]).Any();
        }
    }

    class FileData
    {
        /// <summary>
        /// 估计是文件的Hash数据，不知道文件内容改变后，是否会影响数据的解包
        /// </summary>
        public byte[] HashData { get; set; }

        /// <summary>
        /// 文件数据的偏移位置，实际位置为BaseOffset+Offset
        /// </summary>
        public uint Offset { get; set; }

        public uint Size { get; set; }

        /// <summary>
        /// 文件名的偏移位置，实际位置为NameStart+NameOffset 
        /// </summary>
        public uint NameOffset { get; set; }

        /// <summary>
        /// 估计为文件名字的校验值
        /// </summary>
        public byte[] Unknown { get; set; }

        /// <summary>
        /// 文件解包后输出的路径
        /// </summary>
        public string FilePath { get; set; }
    }

    internal class HpkFile
    {
        public byte[] Magic
        {
            get
            {
                return new byte[] { 0x30, 0x31, 0x50, 0x48 };
            }
        }

        public uint FileCount { get; set; }

        public uint CompressedSize { get; set; }

        public uint NameEnd { get; set; }

        public uint NameStart { get; set; }

        public uint NameLength
        {
            get { return NameEnd - NameStart; }
        }

        /// <summary>
        /// 实际上为文件数据的开始位置
        /// </summary>
        public uint BaseOffset { get; set; }

        public int Version { get; set; }
    }
}
