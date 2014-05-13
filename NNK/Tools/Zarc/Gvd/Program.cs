using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpUtils.Getopt;
namespace Gvd
{
    class Program : GetoptCommandLineProgram
    {
        [Command("-eg", "--extractgvd")]
        [Description("将gvd.dat文件解包，并放入gvd目录下")]
        [Example("-eg gvd.dat")]
        public void ExtractToGVDs(string file)
        {
            GvdProcessor.Extract(file);
            Console.WriteLine("处理完成");
        }

        [Command("-gi", "--gvd2image")]
        [Description("将指定目录下的所有gvd文件转换为图像文件")]
        [Example("-gi GVD")]
        public void ExtractImage(string path)
        {
            GvdProcessor.Gvd2Img(path);
            Console.WriteLine("处理完成");
        }

        [Command("-e", "--extract")]
        [Description("解包gvd.dat并转换图像")]
        [Example("-e gvd.dat")]
        public void ExtractAll(string file)
        {
            GvdProcessor.Extract(file);
            Console.WriteLine("gvd.dat解包完成，开始进行转换");
            FileInfo currentFile = new FileInfo(file);
            string targetPath = Path.Combine(currentFile.Directory.FullName, "GVD");
            GvdProcessor.Gvd2Img(targetPath);
            Console.WriteLine("全部处理完成");
        }

        static void Main(string[] args)
        {
            new Program().Run(args);
        }
    }
}
