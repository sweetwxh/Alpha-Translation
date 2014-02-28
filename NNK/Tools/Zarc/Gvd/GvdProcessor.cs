using System;
using System.Collections.Generic;
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

        public static void Test()
        {
            using (var reader = File.OpenRead("GVD/LR0001.gvd"))
            {
                reader.Seek(0xa00, SeekOrigin.Begin);
                byte[] data = reader.ReadBytes(0x1c97);
                using (var writer = File.OpenWrite("test.jfif"))
                {
                    writer.Write(data, 0, data.Length);
                }
            }
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

                //读取文件名列表
                List<string> fileNames = new List<string>();
                foreach (var page in pages)
                {
                    stream.Seek(page.FileNameOffset + header.HeaderLength, SeekOrigin.Begin);
                    fileNames.Add(stream.ReadString((int)page.FileNameLength.NativeValue, Encoding.ASCII));
                }

                //解包GVD文件
                FileInfo currentFile = new FileInfo(filePath);
                string targetPath = Path.Combine(currentFile.Directory.FullName, "GVD");
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }

                for (int i = 0; i < header.Total; i++)
                {
                    Page page = pages[i];
                    string fileName = fileNames[i];
                    using (var writter = File.OpenWrite(Path.Combine(targetPath, fileName)))
                    {
                        stream.Seek(header.HeaderLength + page.DBVOffset, SeekOrigin.Begin);
                        byte[] data = stream.ReadBytes((int)page.DBVLength.NativeValue);
                        writter.Write(data, 0, data.Length);
                    }
                }
            }
        }
    }
}
