/**************************************
 * 作者：吴兴华
 * 日期：2013-02-01
 * 说明：网上找了很久，都没有找到一款合适的16进制搜索工具，总之都会有一些限制，对于游戏文本的搜索
 * 十分不方便，所以就自己开发了。后期会不断丰富这个小工具的功能，包括相对搜索等。
 * Copyright: alpha game localization team
 * ************************************/

#region 引用

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

#endregion

namespace HexSearch
{
    public partial class MainForm : Form
    {
        private readonly Stopwatch watch = new Stopwatch(); //计时器
        private int completeCount; //已经完成任务的线程个数
        private int processorCount = 2; //准备使用多个线程来处理任务
        private int totalFound; //总共找到匹配的文件个数

        public MainForm()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            InitEncoding();
            InitThreading();
        }

        private void InitThreading()
        {
            processorCount = Environment.ProcessorCount + 2;
        }

        /// <summary>
        ///     初始化文本编码
        /// </summary>
        private void InitEncoding()
        {
            EncodingInfo[] encodings = Encoding.GetEncodings();

            EncodingItem[] items = encodings.Select(e => new EncodingItem
                {
                    CodePage = e.CodePage,
                    DisplayName = e.DisplayName + " - " + e.CodePage.ToString()
                }).ToArray();

            cmbTextEncoding.Items.AddRange(items);
            cmbTextEncoding.SelectedIndex = items.Length - 1;
        }

        /// <summary>
        ///     打开要搜索的路径，也可以手动在文本框输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowser_Click(object sender, EventArgs e)
        {
            if (openFolder.ShowDialog() == DialogResult.OK)
            {
                tbSearchPath.Text = openFolder.SelectedPath;
            }
        }

        /// <summary>
        ///     当文本变化时，获取文本的16进制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearchText_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchText.Text.Length > 0)
            {
                EncodingItem selectedEncoding = cmbTextEncoding.SelectedItem as EncodingItem;
                byte[] textData = Encoding.GetEncoding(selectedEncoding.CodePage).
                                           GetBytes(tbSearchText.Text);

                StringBuilder hexBuilder = new StringBuilder();
                foreach (byte data in textData)
                {
                    hexBuilder.Append(string.Format("{0:X}", data));
                }

                tbSearchHex.Text = hexBuilder.ToString();
            }
            else
            {
                tbSearchHex.Text = string.Empty;
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (tbSearchPath.Text.Trim().Equals(string.Empty))
            {
                ShowError("请选择目录", "错误");
                return;
            }

            if (!Directory.Exists(tbSearchPath.Text.Trim()))
            {
                ShowError("指定的目录不存在", "错误");
                return;
            }

            if (tbSearchHex.Text.Trim().Equals(string.Empty))
            {
                ShowError("请输入要输入的16进制数据", "错误");
                return;
            }

            //转换数据
            byte[] searchData;
            try
            {
                searchData = SearchUtils.HexStringToBytes(tbSearchHex.Text.Trim().ToUpper());
            }
            catch (ArgumentException error)
            {
                ShowError(error.Message, "错误");
                return;
            }

            //获取要处理的所有文件
            FileInfo[] files;
            try
            {
                files = SearchUtils.GetSearchFiles(new DirectoryInfo(tbSearchPath.Text.Trim()), tbFilePattern.Text);
            }
            catch (ArgumentException argumentError)
            {
                ShowError("指定的模式匹配无效", "错误");
                return;
            }
            catch (Exception error)
            {
                ShowError("搜索文件时出现异常：" + error.Message, "错误");
                return;
            }

            //清空列表
            lbResult.Items.Clear();
            watch.Start();
            StartSearch(files, searchData);
            statusText.Text = string.Format("开始搜索,共准备文件：{0}个", files.Length);
        }

        /// <summary>
        ///     开始搜索数据
        /// </summary>
        /// <param name="files"></param>
        /// <param name="searchData"></param>
        private void StartSearch(FileInfo[] files, byte[] searchData)
        {
            for (int i = 0; i < processorCount; i++)
            {
                WorkerParameter param = new WorkerParameter
                    {
                        StartIndex = i,
                        Files = files,
                        SearchData = searchData
                    };
                ParameterizedThreadStart pts = Worker;
                new Thread(pts).Start(param);
            }
        }

        /// <summary>
        ///     工作线程
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="files"></param>
        /// <param name="searchData"></param>
        private void Worker(object param)
        {
            WorkerParameter wp = (WorkerParameter) param;
            for (int i = wp.StartIndex; i < wp.Files.Length; i += processorCount)
            {
                FileDataSearch searcher = new FileDataSearch();
                long foundIndex = searcher.SearchFor(wp.Files[i], wp.SearchData);
                if (foundIndex != -1)
                {
                    FileResult result = new FileResult
                        {
                            Display = string.Format("{0}(偏移: 0x{1:X})", wp.Files[i].Name, foundIndex),
                            FullPath = wp.Files[i].FullName
                        };
                    BeginInvoke(new Action(() => lbResult.Items.Add(result)));
                    Interlocked.Increment(ref totalFound);
                }
            }
            Interlocked.Increment(ref completeCount);
            //判断是否所有线程都工作完毕
            if (completeCount == processorCount)
            {
                BeginInvoke(new Action(() =>
                    {
                        watch.Stop();
                        statusText.Text = string.Format("搜索完毕，在{0}个文件中共找到匹配文件：{1}个(耗时：{2}ms)", wp.Files.Length,
                                                        totalFound,
                                                        watch.ElapsedMilliseconds);
                        watch.Reset();
                        totalFound = 0;
                    }));
                completeCount = 0;
            }
        }

        private void ShowError(string text, string caption)
        {
            MessageBox.Show(text, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        /// <summary>
        ///     双击打开文件位置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbResult_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lbResult.SelectedIndex != -1)
            {
                FileResult result = lbResult.SelectedItem as FileResult;
                ProcessStartInfo processStart = new ProcessStartInfo("explorer",
                                                                     string.Format("/e,/select,{0}", result.FullPath));
                Process.Start(processStart);
            }
        }
    }
}