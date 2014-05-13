using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gvd
{
    class GvdProcessor
    {
        /*************************************
         * 参考资料：http://vitadevwiki.com/index.php?title=PlayView
         * ***********************************/

        public static void Gvd2Img(string targetPath)
        {
            //获取所有文件
            string[] files = Directory.GetFiles(targetPath);

            //依次处理每个文件
            foreach (var file in files)
            {
                Console.WriteLine("正在处理：{0}", file);

                using (var reader = File.OpenRead(file))
                {
                    var dbvStart = reader.ReadStruct<DatabaseViewerStart>();
                    var dbHeader = reader.ReadStruct<DatabaseHeader>();
                    uint entryConunt = (dbvStart.DataBaseLength - 8) / dbHeader.EntryLength;
                    List<Entry> entries = new List<Entry>();
                    for (int i = 0; i < entryConunt; i++)
                    {
                        entries.Add(reader.ReadStruct<Entry>());
                    }
                    var dbvEnd = reader.ReadStruct<DatabaseViewerEnd>();



                    //依次处理每个图片
                    FileInfo fileInfo = new FileInfo(file);
                    string imagePath = Path.Combine(fileInfo.DirectoryName,
                        Path.GetFileNameWithoutExtension(fileInfo.Name));

                    if (!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }

                    if (Encoding.ASCII.GetString(dbvStart.Magic).Equals("GVEW0100JPEG0100"))
                    {
                        JPEG(entries, imagePath, reader);
                    }
                    else
                    {
                        GVMP(entries, imagePath, reader);
                    }
                }
            }
        }

        private static void GVMP(List<Entry> entries, string imagePath, Stream reader)
        {
            int startLayer = 0;
            var images = CalcImage(entries);
            Graphics g = Graphics.FromImage(images[startLayer]);//默认两张图，不知道是否GVMP是动态替换的

            //处理第二张图的
            Bitmap another = new Bitmap(images[startLayer].Width, images[startLayer].Height);
            Graphics ag = Graphics.FromImage(another);

            int x = 0;
            int y = 0;
            Entry lastEntry = entries[0];
            bool isStart = true;

            foreach (var entry in entries)
            {
                //合成图像
                if (entry.LayerLevel == startLayer)
                {
                    if (isStart)
                    {
                        isStart = false;
                    }
                    else
                    {
                        if (entry.GridHeight == lastEntry.GridHeight)
                        {
                            x += (int)lastEntry.ImageWidth.NativeValue;
                        }
                        else
                        {
                            //换行
                            x = 0;
                            y += (int)lastEntry.ImageHeight.NativeValue;
                        }
                    }
                }
                else
                {
                    //换层
                    //先存储已经完成的图像
                    g.Dispose();
                    images[startLayer].Save(Path.Combine(imagePath, string.Format("{0}-0.jfif", startLayer)));
                    images[startLayer].Dispose();

                    ag.Dispose();
                    another.Save(Path.Combine(imagePath, string.Format("{0}-1.jfif", startLayer)));
                    another.Dispose();

                    Console.WriteLine("图层:{0}处理完成", startLayer);

                    startLayer = (int)entry.LayerLevel.NativeValue;
                    g = Graphics.FromImage(images[startLayer]);

                    another = new Bitmap(images[startLayer].Width, images[startLayer].Height);
                    ag = Graphics.FromImage(another);

                    x = 0;
                    y = 0;
                }

                lastEntry = entry;

                byte[] data = reader.ReadBytes((int)entry.ImageLength.NativeValue);

                using (
                  MemoryStream gvmpStream =
                      new MemoryStream(data))
                {
                    var header = gvmpStream.ReadStruct<GVMP>();

                    var originData = gvmpStream.SliceWithLength(header.HeaderLengh, header.OriginLength);
                    Bitmap originTile = new Bitmap(originData);
                    g.DrawImage(originTile, new Point(x, y));
                    originTile.Dispose();

                    var overlappedData = gvmpStream.SliceWithLength(header.OverlappedOffset, header.OverlappedLength);
                    Bitmap overlappedTile = new Bitmap(overlappedData);
                    ag.DrawImage(overlappedTile, new Point(x, y));
                    overlappedTile.Dispose();
                    //Bitmap tile = new Bitmap(tempStream);
                    //g.DrawImage(tile, new Point(x, y));
                    //reader.Skip(entry.LengthPadding);
                    //tile.Dispose();
                }
            }
            g.Dispose();
            images[startLayer].Save(Path.Combine(imagePath, string.Format("{0}.jfif", startLayer)));
            images[startLayer].Dispose();

            ag.Dispose();
            another.Save(Path.Combine(imagePath, string.Format("{0}-1.jfif", startLayer)));
            another.Dispose();
            Console.WriteLine("图层:{0}处理完成", startLayer);
        }

        private static void JPEG(List<Entry> entries, string imagePath, Stream reader)
        {
            int startLayer = 0;
            var images = CalcImage(entries);
            Graphics g = Graphics.FromImage(images[startLayer]);

            int x = 0;
            int y = 0;
            Entry lastEntry = entries[0];
            bool isStart = true;
            foreach (var entry in entries)
            {
                //合成图像
                if (entry.LayerLevel == startLayer)
                {
                    if (isStart)
                    {
                        isStart = false;
                    }
                    else
                    {
                        if (entry.GridHeight == lastEntry.GridHeight)
                        {
                            x += (int)lastEntry.ImageWidth.NativeValue;
                        }
                        else
                        {
                            //换行
                            x = 0;
                            y += (int)lastEntry.ImageHeight.NativeValue;
                        }
                    }
                }
                else
                {
                    //换层
                    //先存储已经完成的图像
                    g.Dispose();
                    images[startLayer].Save(Path.Combine(imagePath, string.Format("{0}.jfif", startLayer)));
                    images[startLayer].Dispose();
                    Console.WriteLine("图层:{0}处理完成", startLayer);

                    startLayer = (int)entry.LayerLevel.NativeValue;
                    g = Graphics.FromImage(images[startLayer]);
                    x = 0;
                    y = 0;
                }

                lastEntry = entry;

                using (
                  MemoryStream tempStream =
                      new MemoryStream(reader.ReadBytes((int)entry.ImageLength.NativeValue)))
                {
                    Bitmap tile = new Bitmap(tempStream);
                    g.DrawImage(tile, new Point(x, y));
                    reader.Skip(entry.LengthPadding);
                    tile.Dispose();
                }
            }
            g.Dispose();
            images[startLayer].Save(Path.Combine(imagePath, string.Format("{0}.jfif", startLayer)));
            images[startLayer].Dispose();
            Console.WriteLine("图层:{0}处理完成", startLayer);
        }

        private static List<Bitmap> CalcImage(List<Entry> entries)
        {
            List<Bitmap> images = new List<Bitmap>();

            var layerCount = entries.Distinct(new LayerCompare()).Count();

            for (int i = 0; i < layerCount; i++)
            {
                int width = (int)entries.Where(e => e.LayerLevel == i && e.GridHeight == 0).Sum(e => e.ImageWidth);
                int height = (int)entries.Where(e => e.LayerLevel == i && e.GridWidth == 0).Sum(e => e.ImageHeight);

                Bitmap image = new Bitmap(width, height);
                images.Add(image);
            }

            return images;
        }

        public static void Extract(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("文件不存在");
                return;
            }

            using (var stream = File.OpenRead(filePath))
            {
                TGDT0100 header = stream.ReadStruct<TGDT0100>();
#if DEBUG
                Console.WriteLine("文件数：{0},头文件信息长度：{1}", header.Total, header.HeaderLength);
#endif
                List<Page> pages = new List<Page>();
                for (int i = 0; i < header.Total; i++)
                {
                    pages.Add(stream.ReadStruct<Page>());
                }

                //解包GVD文件
                FileInfo currentFile = new FileInfo(filePath);
                string targetPath = Path.Combine(currentFile.Directory.FullName, "GVD");
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                foreach (var page in pages)
                {
                    stream.Seek(page.FileNameOffset + header.HeaderLength, SeekOrigin.Begin);
                    string tfilePath = Path.Combine(targetPath,
                        stream.ReadString((int)page.FileNameLength.NativeValue, Encoding.ASCII));
                    using (var writter = File.OpenWrite(tfilePath))
                    {
                        stream.Seek(header.HeaderLength + page.DBVOffset, SeekOrigin.Begin);
                        byte[] data = stream.ReadBytes((int)page.DBVLength.NativeValue);
                        writter.Write(data, 0, data.Length);
                    }
                }
            }
        }
    }

    class LayerCompare : IEqualityComparer<Entry>
    {

        public bool Equals(Entry x, Entry y)
        {
            return x.LayerLevel == y.LayerLevel;
        }

        public int GetHashCode(Entry obj)
        {
            return obj.LayerLevel.GetHashCode();
        }
    }
}
