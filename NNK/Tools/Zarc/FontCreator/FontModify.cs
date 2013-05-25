using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;

namespace FontCreator
{
    class FontModify
    {
        public void Modify(FileInfo cfgFile, FileInfo imgFile, int height, int fontHeight, int fontSize, string fontName)
        {
            FontConfig config = new FontConfig();
            config.FromCfg(cfgFile);

            if (config.Chr.Count == 0)
            {
                Console.WriteLine("未找到要修改的字体");
                return;
            }

            using (Bitmap img = new Bitmap(imgFile.FullName))
            {
                using (Graphics g = Graphics.FromImage(img))
                {
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;
                    //定义字体，背景色，文字颜色，文字样式
                    Font font = new Font(new FontFamily(fontName), fontSize, GraphicsUnit.Pixel);
                    Brush back = new SolidBrush(Color.Black);
                    Brush fontColor = new SolidBrush(Color.White);

                    StringFormat format = StringFormat.GenericTypographic;
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    //遍历CHR，修改字体
                    foreach (CHR c in config.Chr)
                    {
                        string word = config.GetChar(c.UTF8);
                        //先清空原始位置的文字
                        g.FillRectangle(back, new Rectangle((int)c.X, (int)c.Y, c.Width, height));
                        //绘制文字
                        g.DrawString(word, font, fontColor, new RectangleF(c.X, c.Y, c.Width, fontHeight), format);
                    }
                }

                //保存图片为BMP格式
                string newFileName = imgFile.FullName.Replace(imgFile.Extension, "") + "_modify.bmp";
                img.Save(newFileName, ImageFormat.Bmp);
            }

        }
    }
}
