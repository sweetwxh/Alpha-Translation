using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace NeptuneVSolutions
{
    /// <summary>
    /// This is for testing
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            Bitmap img = new Bitmap(200, 100, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, 200, 100);
            g.DrawString("测试", new Font(new FontFamily("宋体"), 24), new SolidBrush(Color.White), 2, 2);
            g.Dispose();

            Bitmap converted = BitmapToGrayscale4bpp(img);

            converted.Save("test.bmp", ImageFormat.Bmp);
            converted.Dispose();
            img.Dispose();

        }

        public static Bitmap BitmapToGrayscale4bpp(Bitmap source)
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
}
