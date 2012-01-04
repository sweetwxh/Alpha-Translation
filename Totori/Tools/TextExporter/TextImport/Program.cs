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

namespace TextImport
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===============开始处理===============");
            string outputDir = AppDomain.CurrentDomain.BaseDirectory + "event_replace\\";
            //获取所有包含翻译文本资源的文件夹
            string[] dirs = Directory.GetDirectories(AppDomain.CurrentDomain.BaseDirectory + "event_alpha");
            Array.ForEach(dirs, dir =>
            {
                //获取当前目录的文件夹名称
                string dirName = dir.Substring(dir.LastIndexOf('\\') + 1);
                //遍历文件夹下的文本文件
                string[] files = Directory.GetFiles(dir, "*.ebm.xml");
                Array.ForEach(files, file =>
                {
                    //处理文件
                    FileInfo info = new FileInfo(file);
                    Console.WriteLine(string.Format("正在处理文件：{0}", info.Name));
                    string replacFile = info.Name.Remove(info.Name.LastIndexOf('.'));
                    string saveDir = string.Format("{0}{1}\\", outputDir, dirName);
                    if (!Directory.Exists(saveDir))
                        Directory.CreateDirectory(saveDir);
                    using (FileStream writer = new FileStream(string.Format("{0}{1}", saveDir, replacFile),
                        FileMode.Create, FileAccess.Write))
                    {
                        try
                        {
                            XDocument doc = XDocument.Load(file);
                            //获取Header
                            byte[] head = Convert.FromBase64String(doc.Root.Element("Header").Value);
                            writer.Write(head, 0, 4);
                            //获取文本节点
                            IEnumerable<XElement> sentences = doc.Root.Element("Sentence").Elements("Text");
                            foreach (XElement text in sentences)
                            {
                                //获取控制位
                                byte[] controls = Convert.FromBase64String(text.Element("TextHeader").Value);
                                //获取文本
                                byte[] content = Encoding.UTF8.GetBytes(text.Element("Content").Value);
                                //最后一位补0x00
                                byte[] ipContent = new byte[content.Length + 1];
                                Array.Copy(content, ipContent, content.Length);
                                ipContent[ipContent.Length - 1] = 0x00;

                                //修改controls最后4位的长度位
                                byte[] length = BitConverter.GetBytes(ipContent.Length);
                                Array.Copy(length, 0, controls, controls.Length - 4, 4);

                                //输出
                                writer.Write(controls, 0, 0x24);
                                writer.Write(ipContent, 0, ipContent.Length);
                                writer.Flush();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            Console.ReadKey();
                        }
                    }
                });
            });
            Console.WriteLine("===============处理结束===============");
            Console.WriteLine("按任意键退出");
            Console.ReadKey();
        }
    }
}
