using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FontCreator
{
    public enum ConvertState
    {
        NameOnly = 0, NameCode = 1, All = 2
    }

    /// <summary>
    /// 通过p3Igg文件生成p3Img文件
    /// </summary>
    class Igg2Img
    {
        public void Produce(string name, FileInfo igg, ConvertState state)
        {
            string originName = igg.Name.Replace(igg.Extension, "");
            short width = 0, height = 0;
            switch (igg.Length)
            {
                case 4194304:
                    width = 2048; height = 1024;
                    break;
                case 2097152:
                    width = 1024; height = 1024;
                    break;
                case 524288:
                    width = 512; height = 512;
                    break;
                case 65536:
                    width = 256; height = 128;
                    break;
                case 32768:
                    width = 128; height = 128;
                    break;
            }
            FontImg img = new FontImg();
            if (!img.FontExist(name))
            {
                Console.WriteLine("指定的字体名称在游戏中不存在");
                return;
            }
            using (MemoryStream writer = new MemoryStream())
            {
                //写入头
                writer.Write(img.FixHeader, 0, img.FixHeader.Length);
                int totalLength = (int)(igg.Length + 0x100);
                writer.Write(DataConverter.BigEndian.GetBytes(totalLength), 0, 4);
                writer.Write(new byte[4], 0, 4);
                writer.Write(DataConverter.BigEndian.GetBytes(0x90), 0, 4);
                if (state == ConvertState.NameCode || state == ConvertState.All)
                    writer.Write(img.BaseIdentifier(name), 0, 4);
                else
                    writer.Write(img.BaseIdentifier(originName), 0, 4);

                writer.Write(new byte[8], 0, 8);
                writer.Write(DataConverter.BigEndian.GetBytes(0x100), 0, 4);
                writer.Write(DataConverter.BigEndian.GetBytes((int)igg.Length), 0, 4);
                writer.Write(img.FixBody, 0, img.FixBody.Length);

                if (state == ConvertState.All)
                    writer.Write(img.Identifier(name), 0, 4);
                else
                    writer.Write(img.Identifier(originName), 0, 4);
                writer.Write(new byte[12], 0, 12);
                byte[] fix = new byte[8];
                fix[3] = 0x44;
                writer.Write(fix, 0, 8);
                writer.Write(DataConverter.BigEndian.GetBytes((int)igg.Length), 0, 4);
                writer.Write(new byte[4], 0, 4);
                writer.Write(img.FixFooter, 0, img.FixFooter.Length);
                writer.Write(DataConverter.BigEndian.GetBytes(width), 0, 2);
                writer.Write(DataConverter.BigEndian.GetBytes(height), 0, 2);
                byte[] whFix = new byte[4];
                whFix[1] = 1;
                writer.Write(whFix, 0, 4);
                int doubleWidth = width * 2;
                writer.Write(DataConverter.BigEndian.GetBytes(doubleWidth), 0, 4);
                writer.Write(new byte[12], 0, 12);

                //写入结尾
                writer.Write(new byte[4], 0, 4);
                byte[] nameData = Encoding.UTF8.GetBytes(name);
                writer.Write(nameData, 0, nameData.Length);
                int fixZero = 48 - nameData.Length - 4;
                writer.Write(new byte[fixZero], 0, fixZero);

                File.WriteAllBytes(name + ".p3img", writer.ToArray());
                File.WriteAllBytes(name + ".p3igg", File.ReadAllBytes(igg.FullName));
            }
        }
    }
}
