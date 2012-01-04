/**************************************
 * 作者：sweetwxh
 * 日期：2011-12-24
 * Copyright: Alpha team
 * ************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace FontConventer
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;
            Console.WriteLine("开始处理字库");
            ushort rMask = 0xF800;
            ushort gMask = 0x07E0;
            ushort bMask = 0x001F;
            using (FileStream reader = new FileStream(args[0], FileMode.Open, FileAccess.Read))
            {
                byte[] data = new byte[reader.Length];
                reader.Read(data, 0, (int)reader.Length);

                //List<string> repeat = new List<string>();
                //repeat.Add(string.Format("R:{0},G:{1},B:{2}", 0, 28, 128));
                //repeat.Add(string.Format("R:{0},G:{1},B:{2}", 0, 28, 248));
                /*
                for (int i = 0; i < reader.Length; i += 2)
                {
                    ushort pixel = (ushort)((data[i + 1] << 4) | data[i]);
                    ushort redPipeline = (ushort)(pixel & gMask);
                    data[i] = (byte)redPipeline;
                    data[i + 1] = (byte)(redPipeline >> 8);
                    byte r = (byte)((pixel & red) >> 11);
                    byte g = (byte)((pixel & green) >> 5);
                    byte b = (byte)((pixel & blue));
                    r <<= 3;
                    g <<= 2;
                    b <<= 3;
                    string fmt = string.Format("R:{0},G:{1},B:{2}", r, g, b);
                    if (!repeat.Contains(fmt))
                    {
                        //将其他颜色转换
                        //data[i] = 0xF0;
                        //data[i + 1] = 0x00;
                        //repeat.Add(fmt);
                        //Console.WriteLine(fmt);
                    }

                }*/

                Bitmap bmp = new Bitmap(2048, 1024, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

                IntPtr ptr = bmpData.Scan0;

                int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

                System.Runtime.InteropServices.Marshal.Copy(data, 0, ptr, bytes);
                bmp.UnlockBits(bmpData);

                string savePath = string.Format("{0}\\{1}", Environment.CurrentDirectory, "白骑士字库(混合).bmp");
                bmp.Save(savePath, ImageFormat.Bmp);
                bmp.Dispose();
                Console.WriteLine(savePath);
            }
            Console.WriteLine("处理完毕");
            Console.ReadKey();
        }
    }
}
