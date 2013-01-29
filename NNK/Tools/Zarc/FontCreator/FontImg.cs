using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FontCreator
{
    class FontImg
    {
        private Dictionary<string, P3Img> allImgs = new Dictionary<string, P3Img>();
        public FontImg()
        {
            Assembly asm = Assembly.GetExecutingAssembly();//读取嵌入式资源

            using (StreamReader s = new StreamReader(asm.GetManifestResourceStream("FontCreator.p3img.txt")))
            {
                while (!s.EndOfStream)
                {
                    string line = s.ReadLine();
                    string[] info = line.Split(new char[] { ',' });
                    P3Img img = new P3Img
                    {
                        Name = info[0],
                        BaseIdentifier = int.Parse(info[1]),
                        Identifier = int.Parse(info[2])
                    };
                    allImgs.Add(img.Name, img);
                }
            }
        }

        /// <summary>
        /// 获取指定字体的基础标识
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public byte[] BaseIdentifier(string name)
        {
            if (FontExist(name))
                return DataConverter.BigEndian.GetBytes(allImgs[name].BaseIdentifier);
            return null;
        }

        /// <summary>
        /// 获取指定字体的标识
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public byte[] Identifier(string name)
        {
            if (FontExist(name))
                return DataConverter.BigEndian.GetBytes(allImgs[name].Identifier);
            return null;
        }

        /// <summary>
        /// 指定名称的字体是否存在
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool FontExist(string name)
        {
            return allImgs.ContainsKey(name);
        }

        public Dictionary<string, P3Img> AllImg
        {
            get
            {
                return allImgs;
            }
        }

        public byte[] FixHeader
        {
            get
            {
                return new byte[] { 0x69, 0x6D, 0x67, 0x00, 0x00, 0x90, 0x00, 0x00 };
            }
        }

        public byte[] FixFooter
        {
            get
            {
                return new byte[] { 0xA3, 0x01, 0x02, 0x00, 0x00, 0x00, 0xAA, 0xE4 };
            }
        }

        public byte[] FixBody
        {
            get
            {
                byte[] body = new byte[104];
                body[3] = 1;
                body[11] = 0x10;
                body[21] = 1;
                return body;
            }
        }
    }
}
