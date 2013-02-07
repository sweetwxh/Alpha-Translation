using Mono;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

namespace OriginFontCreator
{
    /*************************************************
     * 用一张大图来放所有文字，准备一个每行放80个字，共100行的大图。
     * 文字的高度都是2A，宽度都统一按40算，则宽度至少3200个像素，高度4200像素
     * 一共7110个字
     * ***********************************************/
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                PrintUsage();
                return;
            }

            FileInfo file = new FileInfo(args[0]);
            if (!file.Exists)
            {
                Console.WriteLine("指定的文件不存在");
                return;
            }

            string outputName = args[1];

            FFUFile ffu = ReadFile(file);
            Config[] cfgs = FillConfig(file, ffu);

            int outputWidth = int.Parse(args[2]);
            int outputHeight = int.Parse(args[3]);
            //建立一张大图
            Bitmap bmp = new Bitmap(outputWidth, outputHeight, PixelFormat.Format24bppRgb);

            Graphics g = Graphics.FromImage(bmp);

            //设定绘制参数
            int lineMaxHeight = 0, currentX = 0, currentY = 0;

            //循环控制参数来绘制图片
            foreach (var config in cfgs)
            {
                lineMaxHeight = config.Height > lineMaxHeight ? config.Height : lineMaxHeight;
                //计算是否需要换行
                if ((currentX + config.Width) > outputWidth)
                {
                    currentY += lineMaxHeight;
                    lineMaxHeight = 0;
                    currentX = 0;
                }

                //开始绘制
                MemoryStream img = FontBmp(ffu.ImageData, config.Offset, config.Length, config.Width, config.Height);
                g.DrawImage(Image.FromStream(img), currentX, currentY);
                img.Dispose();
                currentX += config.Width;
            }

            //绘制完成保存图片
            Bitmap reduce = BitmapToGrayscale4bpp(bmp);
            reduce.Save(outputName, ImageFormat.Bmp);
            bmp.Dispose();
            reduce.Dispose();

            Console.WriteLine("处理完成，请按任意键退出");
            Console.ReadKey();
        }

        static void PrintUsage()
        {
            Console.WriteLine("OriginFontCreator.exe [ffufile] [outputfile] [imgwidth] [imgheight]");
            Console.WriteLine("Example:OriginFontCreator.exe advfont.ffu advfont.bmp 2500 3900");
            Console.WriteLine("特别说明：在指定输出图像高度和宽度时，一定要确保大小能放下那么多字，否则会显示不完所有字");
        }

        static FFUFile ReadFile(FileInfo file)
        {
            FFUFile ffu = new FFUFile();
            using (FileStream reader = file.OpenRead())
            {
                using (BinaryReader binReader = new BinaryReader(reader))
                {
                    reader.Seek(4, SeekOrigin.Begin);
                    ffu.WordCount = DataConverter.BigEndian.GetInt32(binReader.ReadBytes(4), 0);

                    reader.Seek(0x10, SeekOrigin.Begin);
                    ffu.EndOffset = DataConverter.BigEndian.GetInt32(binReader.ReadBytes(4), 0);

                    reader.Seek(0x18, SeekOrigin.Begin);
                    ffu.ConfigOffset = DataConverter.BigEndian.GetInt32(binReader.ReadBytes(4), 0);

                    ffu.ImageOffset = DataConverter.BigEndian.GetInt32(binReader.ReadBytes(4), 0);

                    reader.Seek(ffu.ImageOffset, SeekOrigin.Begin);
                    ffu.ImageData = new byte[ffu.ImageLength];
                    reader.Read(ffu.ImageData, 0, ffu.ImageLength);
                }
            }
            return ffu;
        }

        /// <summary>
        /// 读取控制文件并建立控制数据列表
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        static Config[] FillConfig(FileInfo file, FFUFile ffu)
        {
            List<Config> cfgs = new List<Config>();
            using (FileStream reader = file.OpenRead())
            {
                using (BinaryReader binReader = new BinaryReader(reader))
                {
                    reader.Seek(ffu.ConfigOffset, SeekOrigin.Begin);
                    for (int i = 0; i < ffu.WordCount; i++)
                    {
                        Config cfg = new Config
                            {
                                Width = binReader.ReadByte(),
                                Height = binReader.ReadByte(),
                                Length = DataConverter.BigEndian.GetUInt16(binReader.ReadBytes(2), 0),
                                Offset = DataConverter.BigEndian.GetInt32(binReader.ReadBytes(4), 0)
                            };

                        cfgs.Add(cfg);
                    }
                }
            }

            return cfgs.ToArray();
        }

        /// <summary>
        /// 将指定数据的指定区域转换为Bmp
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        static MemoryStream FontBmp(byte[] data, int offset, int length, int width, int height)
        {
            byte[] fontData = new byte[length];
            Array.Copy(data, offset, fontData, 0, length);

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
            ColorPalette palette = bmp.Palette;
            for (int i = 0; i < palette.Entries.Length; i++)
            {
                int cval = 17 * i;
                palette.Entries[i] = Color.FromArgb(0, cval, cval, cval);
            }
            bmp.Palette = palette;
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

            System.Runtime.InteropServices.Marshal.Copy(fontData, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);

            MemoryStream bmpStream = new MemoryStream();
            bmp.Save(bmpStream, ImageFormat.Bmp);
            bmp.Dispose();
            return bmpStream;
        }

        /// <summary>
        /// 将24bpprgb转换为4bpp索引bmp
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        static Bitmap BitmapToGrayscale4bpp(Bitmap source)
        {
            int width = source.Width;
            int height = source.Height;
            Bitmap target = new Bitmap(width, height, PixelFormat.Format4bppIndexed);

            ColorPalette palette = target.Palette;
            for (int i = 0; i < palette.Entries.Length; i++)
            {
                int cval = 17 * i;
                palette.Entries[i] = Color.FromArgb(0, cval, cval, cval);
            }
            target.Palette = palette;


            BitmapData targetData = target.LockBits(new Rectangle(0, 0, width, height),
                                                    ImageLockMode.ReadWrite, PixelFormat.Format4bppIndexed);
            BitmapData sourceData = source.LockBits(new Rectangle(0, 0, width, height),
                                                    ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            unsafe
            {
                for (int r = 0; r < height; r++)
                {
                    byte* pTarget = (byte*)(targetData.Scan0 + r * targetData.Stride);
                    byte* pSource = (byte*)(sourceData.Scan0 + r * sourceData.Stride);
                    byte prevValue = 0;
                    for (int c = 0; c < width; c++)
                    {
                        byte colorIndex = (byte)((((*pSource) * 0.3 + *(pSource + 1) * 0.59 + *(pSource + 2) * 0.11)) / 16);
                        if (c % 2 == 0)
                            prevValue = colorIndex;
                        else
                            *(pTarget++) = (byte)(prevValue << 4 | colorIndex);//海王星中的像素是顺序排列的

                        pSource += 3;
                    }
                }
            }

            target.UnlockBits(targetData);
            source.UnlockBits(sourceData);
            return target;
        }
    }

    class Config
    {
        public int Width { get; set; }

        public int Height { get; set; }

        public int Length { get; set; }

        public int Offset { get; set; }
    }

    class FFUFile
    {
        public int WordCount { get; set; }

        public int ConfigOffset { get; set; }

        public int ImageOffset { get; set; }

        public int EndOffset { get; set; }

        public int ImageLength
        {
            get { return EndOffset - ImageOffset; }
        }

        public int ConfigLength
        {
            get { return ImageOffset - ConfigOffset; }
        }

        public byte[] ImageData { get; set; }
    }

}
