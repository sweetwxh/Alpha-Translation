/***************************************
 * 作者:sweetwxh
 * 日期：2011-12-27
 * Copyright: Alpha team
 * *************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace TextExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===============开始处理===============");
            string outputDir = AppDomain.CurrentDomain.BaseDirectory + "event_alpha\\";
            //获取所有包含文本资源的文件夹
            string[] dirs = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "event");
            Array.ForEach(dirs, dir =>
            {
                //获取当前目录的文件夹名称
                string dirName = dir.Substring(dir.LastIndexOf('\\') + 1);
                //遍历文件夹下的文本文件
                string[] files = Directory.GetFiles(dir, "*.ebm");
                Array.ForEach(files, file =>
                {
                    //处理文件
                    FileInfo info = new FileInfo(file);
                    Console.WriteLine(string.Format("正在处理文件：{0}", info.Name));
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

                        //添加文本容器
                        XElement sentence = new XElement("Sentence");
                        doc.Root.Add(sentence);
                        int sentenceCount = BitConverter.ToInt32(head, 0);
                        for (int i = 0; i < sentenceCount; i++)
                        {
                            XElement text = new XElement("Text");
                            byte[] controls = new byte[0x24];
                            reader.Read(controls, 0, 0x24);
                            //添加控制信息
                            XElement textHeader = new XElement("TextHeader");
                            textHeader.SetValue(Convert.ToBase64String(controls));
                            text.Add(textHeader);

                            int contentLength = BitConverter.ToInt32(controls, 0x20);
                            byte[] content = new byte[contentLength];
                            reader.Read(content, 0, contentLength);

                            //输出文本
                            //去掉最后一个0x00,打包记得加上
                            XElement contentElement = new XElement("Content",
                                new XCData(Encoding.UTF8.GetString(content, 0, content.Length - 1)));
                            text.Add(contentElement);

                            sentence.Add(text);
                        }
                        //保存XML
                        string saveDir = string.Format("{0}{1}\\", outputDir, dirName);
                        if (!Directory.Exists(saveDir))
                            Directory.CreateDirectory(saveDir);
                        doc.Save(string.Format("{0}{1}.xml", saveDir, info.Name));
                    }
                });
            });
            Console.WriteLine("===============处理结束===============");
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
