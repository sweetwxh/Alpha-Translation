namespace TextToHex
{
    partial class Form1
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbHex = new System.Windows.Forms.TextBox();
            this.btnGo = new System.Windows.Forms.Button();
            this.cmbCodePage = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "代码页";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "文本";
            // 
            // tbText
            // 
            this.tbText.Location = new System.Drawing.Point(59, 33);
            this.tbText.Name = "tbText";
            this.tbText.Size = new System.Drawing.Size(329, 21);
            this.tbText.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(24, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "结果";
            // 
            // tbHex
            // 
            this.tbHex.Location = new System.Drawing.Point(59, 60);
            this.tbHex.Multiline = true;
            this.tbHex.Name = "tbHex";
            this.tbHex.Size = new System.Drawing.Size(329, 82);
            this.tbHex.TabIndex = 5;
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(163, 148);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(75, 23);
            this.btnGo.TabIndex = 6;
            this.btnGo.Text = "GO";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // cmbCodePage
            // 
            this.cmbCodePage.DisplayMember = "DisplayName";
            this.cmbCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCodePage.FormattingEnabled = true;
            this.cmbCodePage.Location = new System.Drawing.Point(59, 6);
            this.cmbCodePage.Name = "cmbCodePage";
            this.cmbCodePage.Size = new System.Drawing.Size(329, 20);
            this.cmbCodePage.TabIndex = 7;
            this.cmbCodePage.ValueMember = "CodePage";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 182);
            this.Controls.Add(this.cmbCodePage);
            this.Controls.Add(this.btnGo);
            this.Controls.Add(this.tbHex);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbText);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "文本16进制转换器";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbText;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbHex;
        private System.Windows.Forms.Button btnGo;
        private System.Windows.Forms.ComboBox cmbCodePage;
    }
}

