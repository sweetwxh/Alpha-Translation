using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace FontProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("将文件拖放到应用程序图标上即可，自动识别DDS转换\r\n或输入FontProcessor.exe width height file\r\n默认高宽为2048*1024，目前仅支持ARGB4444");
                return;
            }

            int width = 2048;
            int height = 1024;
            string fileName = string.Empty;
            if (args.Length == 3)
            {
                width = int.Parse(args[0]);
                height = int.Parse(args[1]);
                fileName = args[2];
            }
            else
            {
                fileName = args[0];
            }

            FileInfo file = new FileInfo(fileName);
            if (!file.Exists)
            {
                Console.WriteLine("文件不存在");
                return;
            }

            bool toOrigin = file.Extension.ToLower().Equals(".dds");

            if (!toOrigin)
                ConvertToDDS(file, width, height);
            else
                ConvertToOrigin(file);

            Console.WriteLine("处理完毕,请自行修改文件名");
            Console.ReadKey();
        }

        static void ConvertToOrigin(FileInfo file)
        {
            using (FileStream reader = file.OpenRead())
            {
                reader.Seek(0x80, SeekOrigin.Begin);
                int fileLength = (int)(reader.Length - 0x80);
                byte[] data = new byte[fileLength];

                reader.Read(data, 0, data.Length);

                string currentFileName = file.FullName;
                string newFileName = currentFileName.Remove(currentFileName.Length - file.Extension.Length);
                string savePath = Path.Combine(file.DirectoryName, newFileName);
                File.WriteAllBytes(savePath, data);
                Console.WriteLine(savePath);
            }
        }

        //static void ConvertToOrigin(FileInfo file)
        //{
        //    Bitmap bmp = new Bitmap(file.FullName);
        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
        //    System.Drawing.Imaging.BitmapData bmpData =
        //        bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
        //        PixelFormat.Format16bppRgb565);

        //    IntPtr ptr = bmpData.Scan0;

        //    int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
        //    byte[] data = new byte[bytes];
        //    System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, bytes);
        //    bmp.UnlockBits(bmpData);
        //    bmp.Dispose();

        //    string savePath = Path.Combine(file.DirectoryName, "data.origin");//string.Format("{0}\\{1}", Environment.CurrentDirectory, "ft_gaku.p3igg");
        //    using (FileStream writer = new FileStream(savePath, FileMode.Create, FileAccess.Write))
        //    {
        //        writer.Write(data, 0, data.Length);
        //        writer.Flush();
        //    }
        //    Console.WriteLine(savePath);
        //}

        static void ConvertToDDS(FileInfo file, int width, int height)
        {
            Console.WriteLine("开始转换为DDS");
            using (FileStream reader = file.OpenRead())
            {
                byte[] data = new byte[reader.Length];
                reader.Read(data, 0, (int)reader.Length);

                byte[] start = new byte[] { 
                    0x44,0x44,0x53,0x20,0x7C,0,0,0,0x07,0x10,0x08,0
                };

                using (MemoryStream temp = new MemoryStream())
                {
                    temp.Write(start, 0, start.Length);
                    temp.Write(BitConverter.GetBytes(height), 0, 4);
                    temp.Write(BitConverter.GetBytes(width), 0, 4);
                    temp.Write(new byte[] { 0, 0, 0x40, 0 }, 0, 4);
                    temp.Write(new byte[52], 0, 52);
                    temp.WriteByte(0x20);
                    temp.Write(new byte[3], 0, 3);
                    temp.WriteByte(0x41);
                    temp.Write(new byte[7], 0, 7);
                    temp.WriteByte(0x10);
                    temp.Write(new byte[4], 0, 4);
                    temp.WriteByte(0xf);
                    temp.Write(new byte[2], 0, 2);
                    temp.WriteByte(0xf0);
                    temp.Write(new byte[3], 0, 3);
                    temp.WriteByte(0xf);
                    temp.Write(new byte[4], 0, 4);
                    temp.WriteByte(0xf0);
                    temp.Write(new byte[3], 0, 3);
                    temp.WriteByte(0x10);
                    temp.Write(new byte[18], 0, 18);
                    temp.Write(data, 0, data.Length);

                    string savePath = Path.Combine(file.DirectoryName, file.FullName + ".dds");
                    File.WriteAllBytes(savePath, temp.ToArray());
                    Console.WriteLine(savePath);
                }
            }
        }

        static void ConvertToBMP(FileInfo file, int width, int height)
        {
            Console.WriteLine("开始转换为BMP");
            using (FileStream reader = file.OpenRead())
            {
                byte[] data = new byte[reader.Length];
                reader.Read(data, 0, (int)reader.Length);


                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;

                int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

                System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, bytes);
                bmp.UnlockBits(bmpData);

                string savePath = Path.Combine(file.DirectoryName, "export.bmp");// string.Format("{0}\\{1}", file.DirectoryName, "Export.bmp");
                bmp.Save(savePath, ImageFormat.Bmp);
                bmp.Dispose();
                Console.WriteLine(savePath);
            }
        }
    }
}
