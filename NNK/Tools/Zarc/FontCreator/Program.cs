using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FontCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUseage();
                return;
            }

            switch (args[0].ToLower())
            {
                case "igg2img"://处理img的生成
                    if (args.Length != 4)
                    {
                        PrintUseage();
                        return;
                    }
                    FileInfo igg = new FileInfo(args[2]);
                    if (!igg.Exists)
                    {
                        Console.WriteLine("指定的p3igg文件不存在");
                        return;
                    }
                    Igg2Img p = new Igg2Img();
                    p.Produce(args[1], igg, (ConvertState)int.Parse(args[3]));
                    break;
                case "cfg2bin":
                    if (args.Length != 3)
                    {
                        PrintUseage();
                        return;
                    }
                    FileInfo cfgFile = new FileInfo(args[1]);
                    FontConfig c2b = new FontConfig();
                    c2b.FromCfg(cfgFile);
                    c2b.ToBin(args[2]);
                    break;
                case "bin2cfg":
                    if (args.Length != 3)
                    {
                        PrintUseage();
                        return;
                    }
                    FileInfo binFile = new FileInfo(args[1]);
                    FontConfig b2c = new FontConfig();
                    b2c.FromBin(binFile);
                    b2c.ToCfg(args[2]);
                    break;
                case "mf":
                    if (args.Length != 7)
                    {
                        PrintUseage();
                        return;
                    }
                    FileInfo mfCfgFile = new FileInfo(args[1]);
                    FileInfo mfImgFile = new FileInfo(args[2]);
                    int tileHeight = int.Parse(args[3]);
                    int fontHeight = int.Parse(args[4]);
                    int fontSize = int.Parse(args[5]);
                    string fontName = args[6];
                    FontModify fontModify = new FontModify();
                    fontModify.Modify(mfCfgFile, mfImgFile, tileHeight,fontHeight, fontSize, fontName);
                    break;
                default:
                    PrintUseage();
                    break;
            }
            Console.WriteLine("处理完成\r\n按任意键退出");
            Console.ReadKey();
        }

        static void PrintUseage()
        {
            Console.WriteLine("FontCreator.exe [verb] [option]");
            Console.WriteLine("verb:");
            //igg2img
            Console.WriteLine("igg2img:根据p3igg文件生成对应的p3img文件，其option为：要生成的font名称，p3igg文件路径，转换状态[0|1|2]");
            Console.WriteLine("igg2img 例子：FontCreator.exe igg2img ft_cap_gaku36p ft_gaku.p3igg 0。");
            Console.WriteLine("转换状态：0 表示仅改名名称（0xd4），1表示改变名称和代码（0x17，0xd4），2表示全部替换（0x17，0x90，0xd4）");
            Console.WriteLine("执行之后，将根据ft_gaku.p3igg生成对应名称ft_cap_gaku36p的p3img和p3igg文件\r\n");
            //cfg2bin
            Console.WriteLine("cfg2bin:将cfg文件转换为bin文件，其option为c：cfg文件路径，生成的bin文件路径");
            Console.WriteLine("cfg2bin 例子：FontCreator.exe cfg2bin ft_gaku.cfg ft_gaku.cfg.bin\r\n");
            //bin2cfg
            Console.WriteLine("bin2cfg:将bin文件转换为cfg文件，其option为c：bin文件路径，生成的cfg文件路径");
            Console.WriteLine("bin2cfg 例子：FontCreator.exe bin2cfg ft_gaku.cfg.bin ft_gaku.cfg\r\n");
            //mf
            Console.WriteLine("mf:修改字库图片，其option为：cfg文件路径(仅包含要修改的文字，并且SJIS和UTF8Code已经替换)，字库图片路径（单通道bmp），Tile高度（INF的28或36），字体高度，根据情况进行调整，字体大小，字体名称");
            Console.WriteLine("mf 例子：FontCreator.exe mf ft_caption_modify.cfg 0.bmp 28 30 22 方正准圆繁体");
        }
    }
}
