using System;
using System.Collections.Generic;
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
            GvdProcessor.Test();
            //GvdProcessor.Extract(file);
            Console.WriteLine("处理完成");
        }

        static void Main(string[] args)
        {
            new Program().Run(args);
        }
    }
}
