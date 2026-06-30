using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;

namespace EyeCenter
{
    public partial class FormExportKensa : Form
    {
        public FormExportKensa()
        {
            InitializeComponent();
        }

        private void FormExportKensa_Load(object sender, EventArgs e)
        {
            DataTable table = EyeDict.EyeSet.Tables["KensaPage"];

            foreach (DataRow r in table.Rows)
            {
                KensaListBox.Items.Add(r["ID"].ToString() + " " + r["Name"].ToString());
            }
        }

        private void ExeButton_Click(object sender, EventArgs e)
        {
            if (KensaListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("エクスポートする検査を選んでください");
                return;
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                this.Save(saveFileDialog1.FileName);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        void Save(string file)
        {
            MessageBox.Show("この機能はまだ実装されていません");
        }
    }
}