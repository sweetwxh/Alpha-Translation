using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;

namespace FontTester
{
    class Program
    {
        static void Main(string[] args)
        {
            Font font = new Font(new FontFamily("宋体"), 22, GraphicsUnit.Pixel);
            Bitmap img = new Bitmap(512,512);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, 512, 512);


            Brush brush = new SolidBrush(Color.White);
            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;


            SizeF width = g.MeasureString("京", font, 0, format);
            Console.WriteLine("{0} {1}", width.Width, width.Height);
            g.DrawString("京", font, brush, new RectangleF(1, 1, width.Width, 28), format);
            g.Dispose();
            img.Save("test.bmp", ImageFormat.Bmp);
            img.Dispose();
            Console.WriteLine("complete");
            Console.ReadKey();
        }
    }
}
