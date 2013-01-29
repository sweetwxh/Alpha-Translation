using Mono;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FontCreator
{
    class ImgFileReader
    {
        public void CreateRecord()
        {
            DirectoryInfo directory = new DirectoryInfo("font");
            FileInfo[] allImg = directory.GetFiles("*.p3img");
            List<P3Img> imgInfo = new List<P3Img>();
            foreach (FileInfo single in allImg)
            {
                //跳过这个奇葩文件
                if (single.Name.Equals("ft_gaiji"))
                    continue;

                using (FileStream reader = single.OpenRead())
                {
                    using (BinaryReader br = new BinaryReader(reader))
                    {
                        P3Img p = new P3Img();
                        p.Name = single.Name.Replace(single.Extension, "");
                        reader.Seek(0x14, SeekOrigin.Begin);
                        p.BaseIdentifier = DataConverter.BigEndian.GetInt32(br.ReadBytes(4), 0);
                        reader.Seek(0x90, SeekOrigin.Begin);
                        p.Identifier = DataConverter.BigEndian.GetInt32(br.ReadBytes(4), 0);
                        imgInfo.Add(p);
                    }
                }
            }

            List<string> txts = new List<string>();
            foreach (P3Img s in imgInfo)
            {
                txts.Add(string.Format("{0},{1},{2}", s.Name, s.BaseIdentifier, s.Identifier));
            }

            File.WriteAllLines("p3img.txt", txts.ToArray());
            
        }
    }

    struct P3Img
    {
        /// <summary>
        /// 字库的名字（文件名称）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 基础字库的标识（0x50或0x60）
        /// </summary>
        public int BaseIdentifier { get; set; }

        /// <summary>
        /// 如果没猜错，0x90后的4个bytes是标识，转换成BE的int输出
        /// </summary>
        public int Identifier { get; set; }
    }
}
