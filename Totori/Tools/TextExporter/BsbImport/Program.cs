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

namespace BsbImport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===============开始处理===============");

            //读取XML
            string file = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory, "balloonseldata.bsb.xml");
            string saveFile = "balloonseldata.replace.bsb";
            using (FileStream writer = new FileStream(saveFile, FileMode.Create, FileAccess.Write))
            {
                XDocument doc = XDocument.Load(file);
                //输出Header
                byte[] head = Convert.FromBase64String(doc.Root.Element("Header").Value);
                writer.Write(head, 0, 4);

                IEnumerable<XElement> sentences = doc.Root.Elements("Sentence");
                //依次输出文本
                foreach (XElement sentence in sentences)
                {
                    //输出总长度
                    int textCount = int.Parse(sentence.Attribute("Length").Value);
                    byte[] countData = BitConverter.GetBytes(textCount);
                    writer.Write(countData, 0, 4);
                    IEnumerable<XElement> texts = sentence.Elements("Text");
                    foreach (XElement text in texts)
                    {
                        //获取文本
                        byte[] content = Encoding.UTF8.GetBytes(text.Value);
                        //最后一位补0x00
                        byte[] ipContent = new byte[content.Length + 1];
                        Array.Copy(content, ipContent, content.Length);
                        ipContent[ipContent.Length - 1] = 0x00;
                        //获取文本长度
                        byte[] tLength = BitConverter.GetBytes(ipContent.Length);
                        writer.Write(tLength, 0, 4);
                        writer.Write(ipContent, 0, ipContent.Length);
                    }
                }
                writer.Flush();
            }

            Console.WriteLine("===============处理结束===============");
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
