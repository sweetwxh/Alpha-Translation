namespace HexSearch
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.tbSearchPath = new System.Windows.Forms.TextBox();
            this.btnBrowser = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbTextEncoding = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbSearchText = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbSearchHex = new System.Windows.Forms.TextBox();
            this.lbResult = new System.Windows.Forms.ListBox();
            this.btnSearch = new System.Windows.Forms.Button();
            this.statusBar = new System.Windows.Forms.StatusStrip();
            this.statusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.tbFilePattern = new System.Windows.Forms.TextBox();
            this.openFolder = new System.Windows.Forms.FolderBrowserDialog();
            this.label5 = new System.Windows.Forms.Label();
            this.statusBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "文件路径";
            // 
            // tbSearchPath
            // 
            this.tbSearchPath.Location = new System.Drawing.Point(71, 6);
            this.tbSearchPath.Name = "tbSearchPath";
            this.tbSearchPath.Size = new System.Drawing.Size(362, 21);
            this.tbSearchPath.TabIndex = 1;
            // 
            // btnBrowser
            // 
            this.btnBrowser.Location = new System.Drawing.Point(439, 4);
            this.btnBrowser.Name = "btnBrowser";
            this.btnBrowser.Size = new System.Drawing.Size(75, 23);
            this.btnBrowser.TabIndex = 2;
            this.btnBrowser.Text = "浏览";
            this.btnBrowser.UseVisualStyleBackColor = true;
            this.btnBrowser.Click += new System.EventHandler(this.btnBrowser_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "文本编码";
            // 
            // cmbTextEncoding
            // 
            this.cmbTextEncoding.DisplayMember = "DisplayName";
            this.cmbTextEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTextEncoding.FormattingEnabled = true;
            this.cmbTextEncoding.Location = new System.Drawing.Point(71, 70);
            this.cmbTextEncoding.Name = "cmbTextEncoding";
            this.cmbTextEncoding.Size = new System.Drawing.Size(443, 20);
            this.cmbTextEncoding.TabIndex = 4;
            this.cmbTextEncoding.ValueMember = "CodePage";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 105);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "文    本";
            // 
            // tbSearchText
            // 
            this.tbSearchText.Location = new System.Drawing.Point(71, 102);
            this.tbSearchText.Name = "tbSearchText";
            this.tbSearchText.Size = new System.Drawing.Size(443, 21);
            this.tbSearchText.TabIndex = 5;
            this.toolTips.SetToolTip(this.tbSearchText, "文本将自动转换为指定编码的16进制");
            this.tbSearchText.TextChanged += new System.EventHandler(this.tbSearchText_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 137);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "16  进制";
            // 
            // tbSearchHex
            // 
            this.tbSearchHex.Location = new System.Drawing.Point(71, 134);
            this.tbSearchHex.Name = "tbSearchHex";
            this.tbSearchHex.Size = new System.Drawing.Size(443, 21);
            this.tbSearchHex.TabIndex = 6;
            this.toolTips.SetToolTip(this.tbSearchHex, "输入要搜索的16进制，例如：\r\nE8283F4D6C22 (不区分大小写)");
            // 
            // lbResult
            // 
            this.lbResult.DisplayMember = "Display";
            this.lbResult.FormattingEnabled = true;
            this.lbResult.ItemHeight = 12;
            this.lbResult.Location = new System.Drawing.Point(14, 167);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(500, 280);
            this.lbResult.TabIndex = 9;
            this.toolTips.SetToolTip(this.lbResult, "双击可以打开文件位置");
            this.lbResult.ValueMember = "FullPath";
            this.lbResult.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbResult_MouseDoubleClick);
            // 
            // btnSearch
            // 
            this.btnSearch.Location = new System.Drawing.Point(229, 458);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(75, 23);
            this.btnSearch.TabIndex = 10;
            this.btnSearch.Text = "搜索";
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // statusBar
            // 
            this.statusBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusText});
            this.statusBar.Location = new System.Drawing.Point(0, 491);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(533, 22);
            this.statusBar.TabIndex = 11;
            this.statusBar.Text = "statusStrip1";
            // 
            // statusText
            // 
            this.statusText.Name = "statusText";
            this.statusText.Size = new System.Drawing.Size(56, 17);
            this.statusText.Text = "准备就绪";
            // 
            // toolTips
            // 
            this.toolTips.AutomaticDelay = 10;
            this.toolTips.AutoPopDelay = 5000;
            this.toolTips.InitialDelay = 10;
            this.toolTips.ReshowDelay = 2;
            // 
            // tbFilePattern
            // 
            this.tbFilePattern.Location = new System.Drawing.Point(71, 38);
            this.tbFilePattern.Name = "tbFilePattern";
            this.tbFilePattern.Size = new System.Drawing.Size(443, 21);
            this.tbFilePattern.TabIndex = 3;
            this.toolTips.SetToolTip(this.tbFilePattern, "采用Windows的文件名模式匹配进行搜索，例如：\r\n*.txt, *text_en*");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 41);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 12;
            this.label5.Text = "模式匹配";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(533, 513);
            this.Controls.Add(this.tbFilePattern);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.btnSearch);
            this.Controls.Add(this.lbResult);
            this.Controls.Add(this.tbSearchHex);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbSearchText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbTextEncoding);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnBrowser);
            this.Controls.Add(this.tbSearchPath);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "16进制搜索器 by Alpha Team (sweetwxh)";
            this.statusBar.ResumeLayout(false);
            this.statusBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbSearchPath;
        private System.Windows.Forms.Button btnBrowser;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbTextEncoding;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbSearchText;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbSearchHex;
        private System.Windows.Forms.ListBox lbResult;
        private System.Windows.Forms.Button btnSearch;
        private System.Windows.Forms.StatusStrip statusBar;
        private System.Windows.Forms.ToolStripStatusLabel statusText;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.FolderBrowserDialog openFolder;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbFilePattern;
    }
}

