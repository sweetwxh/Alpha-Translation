#region 引用

using System;
using System.Text;
using System.Windows.Forms;

#endregion

namespace TextToHex
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //填充代码页选项
            EncodingInfo[] encodings = Encoding.GetEncodings();
            foreach (EncodingInfo single in encodings)
            {
                cmbCodePage.Items.Add(new EncodingItem
                    {
                        CodePage = single.CodePage,
                        DisplayName = single.DisplayName + string.Format("({0})", single.CodePage)
                    });
            }
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            tbHex.Text = string.Empty;
            int codepage = ((EncodingItem) cmbCodePage.SelectedItem).CodePage;
            byte[] data = Encoding.GetEncoding(codepage).GetBytes(tbText.Text);
            StringBuilder hexCombine = new StringBuilder();
            foreach (byte single in data)
            {
                hexCombine.Append(string.Format("{0:X}", single));
            }
            tbHex.Text = hexCombine.ToString();
        }
    }

    public class EncodingItem
    {
        public int CodePage { get; set; }

        public string DisplayName { get; set; }
    }
}