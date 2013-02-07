#region 引用

using System;
using System.IO;

#endregion

namespace CtxrProcessor
{
    internal class Ctxr
    {
        public void ToGtf(FileInfo file)
        {
            string baseName = file.Name.Replace(file.Extension, "");
            string rcdPath = string.Empty;
            string gtfPath = string.Empty;
            if (file.DirectoryName != null)
            {
                rcdPath = Path.Combine(file.DirectoryName, baseName + ".dat");
                gtfPath = Path.Combine(file.DirectoryName, baseName + ".gtf");
            }
            Console.WriteLine("正在处理:{0}", file.FullName);
            using (FileStream fileReader = file.OpenRead())
            {
                using (BinaryReader binReader = new BinaryReader(fileReader))
                {
                    using (MemoryStream fileData = new MemoryStream())
                    {
                        byte[] fixHead = binReader.ReadBytes(0x18);

                        //因为前4个字节不能采用通用的方法还原，所以记录到dat中
                        byte[] rcdHead = new byte[4];
                        Array.Copy(fixHead, 0, rcdHead, 0, 4);

                        fixHead[1] = 1;
                        fixHead[2] = 1;
                        fileData.Write(fixHead, 0, fixHead.Length);

                        //需要记录的数据
                        byte[] recordData = new byte[24];
                        byte[] rcd1 = binReader.ReadBytes(0xc);

                        byte[] imgData = binReader.ReadBytes(0x14);
                        fileData.Write(imgData, 0, imgData.Length);

                        byte[] rcd2 = binReader.ReadBytes(8);//与ICO不同之处在于多了8位要记录的
                        Array.Copy(rcdHead, recordData, 4);
                        Array.Copy(rcd1, 0, recordData, 4, 0xc);
                        Array.Copy(rcd2, 0, recordData, 0x10, 8);
                        File.WriteAllBytes(rcdPath, recordData);

                        byte[] zeroData = new byte[0x80 - fileData.Length];
                        fileData.Write(zeroData, 0, zeroData.Length);

                        //写入基础数据
                        fileReader.Seek(0x80, SeekOrigin.Begin);
                        int bodyDataLength = (int)(fileReader.Length - 0x80);
                        byte[] bodyData = binReader.ReadBytes(bodyDataLength);
                        fileData.Write(bodyData, 0, bodyData.Length);

                        File.WriteAllBytes(gtfPath, fileData.ToArray());
                    }
                }
            }
            file.Delete();
        }

        public void ToCtxr(FileInfo file)
        {
            string baseName = file.Name.Replace(file.Extension, "");
            string rcdPath = string.Empty;
            string cxtrPath = string.Empty;
            if (file.DirectoryName != null)
            {
                rcdPath = Path.Combine(file.DirectoryName, baseName + ".dat");
                cxtrPath = Path.Combine(file.DirectoryName, baseName + ".ctxr");
            }
            Console.WriteLine("正在处理:{0}", file.FullName);
            using (FileStream fileReader = file.OpenRead())
            {
                using (BinaryReader binReader = new BinaryReader(fileReader))
                {
                    using (MemoryStream fileData = new MemoryStream())
                    {
                        byte[] recordData = File.ReadAllBytes(rcdPath);

                        byte[] fixHead = binReader.ReadBytes(0x18);
                        fixHead[0] = recordData[0];
                        fixHead[1] = recordData[1];
                        fixHead[2] = recordData[2];
                        fixHead[3] = recordData[3];
                        fileData.Write(fixHead, 0, fixHead.Length);


                        fileData.Write(recordData, 4, 0xc);//写入记录的12位

                        byte[] imgData = binReader.ReadBytes(0x14);
                        fileData.Write(imgData, 0, imgData.Length);

                        fileData.Write(recordData, 0x10, 8);//写入剩下的8位

                        byte[] zeroData = new byte[0x80 - fileData.Length];
                        fileData.Write(zeroData, 0, zeroData.Length);

                        //写入基础数据
                        fileReader.Seek(0x80, SeekOrigin.Begin);
                        int bodyDataLength = (int)(fileReader.Length - 0x80);
                        byte[] bodyData = binReader.ReadBytes(bodyDataLength);
                        fileData.Write(bodyData, 0, bodyData.Length);

                        File.WriteAllBytes(cxtrPath, fileData.ToArray());
                    }
                }
            }
            file.Delete();
            File.Delete(rcdPath);
        }
    }
}