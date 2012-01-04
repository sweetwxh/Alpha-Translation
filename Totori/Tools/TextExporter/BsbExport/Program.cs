/***************************************
 * 作者:sweetwxh
 * 日期：2011-12-28
 * Copyright: Alpha team
 * *************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace BsbExport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===============开始处理===============");

            //读取BSB
            string file = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "balloonseldata.bsb");
            string saveFile = "balloonseldata.bsb.xml";
            using (FileStream reader = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                //创建XML文件
                XDocument doc = new XDocument(
                    new XElement("Texts")
                    );
                //处理文件头（4位）
                byte[] head = new byte[4];
                reader.Read(head, 0, 4);
                //添加Header
                doc.Root.Add(
                    new XElement("Header", Convert.ToBase64String(head))
                    );
                int sentencesCount = BitConverter.ToInt32(head, 0);
                for (int i = 0; i < sentencesCount; i++)
                {
                    //添加Sentences节点
                    XElement sentence = new XElement("Sentence");
                    //读取当前块个数
                    byte[] count = new byte[4];
                    reader.Read(count, 0, 4);
                    int textCount = BitConverter.ToInt32(count, 0);
                    sentence.SetAttributeValue("Length", textCount);
                    for (int j = 0; j < textCount; j++)
                    {
                        //读取文本
                        byte[] tLengthData = new byte[4];
                        reader.Read(tLengthData, 0, 4);
                        int textLength = BitConverter.ToInt32(tLengthData, 0);
                        byte[] textData = new byte[textLength];
                        reader.Read(textData, 0, textLength);
                        //去掉结尾的0x00
                        XElement text = new XElement("Text",
                            Encoding.UTF8.GetString(textData, 0, textData.Length - 1));
                        sentence.Add(text);
                    }
                    doc.Root.Add(sentence);
                }
                doc.Save(saveFile);
            }

            Console.WriteLine("===============处理结束===============");
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
