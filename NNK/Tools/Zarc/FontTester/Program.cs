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
            Font font = new Font(new FontFamily("DFPYuanMedium-B5"), 22, GraphicsUnit.Pixel);
            Bitmap img = new Bitmap("0.bmp");
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(new SolidBrush(Color.Black), 0, 0, 2048, 1024);


            Brush brush = new SolidBrush(Color.White);
            StringFormat format = StringFormat.GenericTypographic;
            format.Alignment = StringAlignment.Center;
            format.LineAlignment = StringAlignment.Center;


            SizeF width = g.MeasureString("京", font, 0, format);
            Console.WriteLine("{0} {1}", width.Width, width.Height);
            g.DrawString("京", font, brush, new RectangleF(1, 271, width.Width, 28), format);
            g.Dispose();
            img.Save("test.bmp", ImageFormat.Bmp);
            img.Dispose();
            Console.WriteLine("complete");
            Console.ReadKey();
        }
    }
}
