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

namespace FontWrapper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
                return;
            Console.WriteLine("开始处理字库");
            Bitmap bmp = new Bitmap("白骑士字库(混合).bmp");
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                PixelFormat.Format16bppRgb565);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] data = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(ptr, data, 0, bytes);
            bmp.UnlockBits(bmpData);
            bmp.Dispose();

            string savePath = string.Format("{0}\\{1}", Environment.CurrentDirectory, "ft_gaku.p3igg");
            using (FileStream writer = new FileStream(savePath, FileMode.Create, FileAccess.Write))
            {
                writer.Write(data, 0, data.Length);
                writer.Flush();
            }
            Console.WriteLine(savePath);
            Console.WriteLine("处理完毕");
            Console.ReadKey();
        }
    }
}
