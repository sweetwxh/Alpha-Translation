/**************************************
 * 作者：sweetwxh
 * 日期：2011-12-29
 * Copyright: Alpha team
 * ************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Core;

namespace PSSGImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("==============开始处理==============");
                //string file = AppDomain.CurrentDomain.BaseDirectory + "a12_title.PSSG";
                string file = args[0];
                //获取文件夹名称
                FileInfo info = new FileInfo(file);
                string dirName = info.Name.Remove(info.Name.LastIndexOf('.'));

                //获取导入文件列表
                string[] imports = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory + dirName);
                //打开被导入文件
                byte[] all;
                using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    all = new byte[reader.Length];
                    reader.Read(all, 0, all.Length);
                }
                //重命名原始文件
                File.Move(file, file + ".bak");
                Array.ForEach(imports, ddsFile =>
                {
                    using (FileStream ddsReader = new FileStream(ddsFile, FileMode.Open, FileAccess.Read))
                    {
                        FileInfo ddsName = new FileInfo(ddsFile);
                        if (ddsReader.Length > 0x80)
                        {
                            byte[] toImport = new byte[ddsReader.Length - 0x80];
                            ddsReader.Seek(0x80, SeekOrigin.Begin);
                            ddsReader.Read(toImport, 0, toImport.Length);
                            //查找该数据偏移位置
                            byte[] nameData = Encoding.UTF8.GetBytes(ddsName.Name);
                            int searched = SearchByte(all, nameData, 0);
                            if (searched != -1)
                            {
                                Console.WriteLine("正在处理文件：" + ddsName.Name);
                                int index = searched + nameData.Length + 0x33;
                                //将新数据拷贝进去
                                Array.Copy(toImport, 0, all, index, toImport.Length);
                            }
                        }
                    }
                });
                //输出新的数据
                using (FileStream pssgWriter = new FileStream(file, FileMode.Create, FileAccess.Write))
                {
                    pssgWriter.Write(all, 0, all.Length);
                    pssgWriter.Flush();
                }
                //压缩文件
                GZip(new FileInfo(file));

                File.Delete(file);

                Console.WriteLine(string.Format("==============处理完毕=============={0}按任意键退出", Environment.NewLine));
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        public static void GZip(FileInfo fi)
        {
            byte[] dataBuffer = new byte[4096];
            using (Stream s = new GZipOutputStream(File.Create(fi.FullName + ".gz")))
            using (FileStream fs = fi.OpenRead())
            {
                StreamUtils.Copy(fs, s, dataBuffer);
            }
        }

        public static void Compress(FileInfo fi)
        {
            using (FileStream inFile = fi.OpenRead())
            {
                if ((File.GetAttributes(fi.FullName)
                    & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".gz")
                {
                    using (FileStream outFile =
                                File.Create(fi.FullName + ".gz"))
                    {
                        using (GZipStream Compress =
                            new GZipStream(outFile,
                            CompressionMode.Compress))
                        {
                            inFile.CopyTo(Compress);
                        }
                    }
                }
            }
        }

        public static int SearchByte(byte[] searched, byte[] find, int start)
        {
            bool matched = false;

            for (int index = start; index <= searched.Length - find.Length; ++index)
            {
                matched = true;

                for (int subIndex = 0; subIndex < find.Length; ++subIndex)
                {
                    if (find[subIndex] != searched[index + subIndex])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return index;
                }
            }
            return -1;
        }
    }
}
