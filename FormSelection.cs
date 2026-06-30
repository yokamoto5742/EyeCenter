using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EyeCenter
{
    public partial class FormSelection : Form
    {
        Control result;
        string delimiter = ",";

        public FormSelection(Control c, List<string> list, string delim)
        {
            InitializeComponent();

            result = c;
            delimiter = delim;

            foreach (string s in list)
            {
                ListBox1.Items.Add(s);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            string s = "";

            foreach (Object o in ListBox1.CheckedItems)
            {
                if (s.Length > 0)
                {
                    s += delimiter;
                }

                s += o.ToString();
            }

            if (result.Text.Length > 0)
            {
                DialogResult d_result = MessageBox.Show("既にデータが存在します。追加しますか？上書きしますか？\r\n　Yes…追加する\r\n　No…上書きする", "確認", MessageBoxButtons.YesNoCancel);

                if (d_result == DialogResult.Yes)
                {
                    result.Text += delimiter + s;
                }
                else if (d_result == DialogResult.No)
                {
                    result.Text = s;
                }
            }
            else
            {
                result.Text = s;
            }

            this.Dispose();
        }
    }
}