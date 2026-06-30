using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;

namespace EyeCenter
{
    public partial class FormOpeRsv : FormBase
    {
        public FormOpeRsv()
        {
            InitializeComponent();
        }

        private void FormOpeRsv_Load(object sender, EventArgs e)
        {
            this.OriginalText = "éËèpó\ñÒ";
            this.FormTextShow();

            DataTable opeKindTable = EyeDict.EyeSet.Tables["OpeKind"];

            foreach (DataRow r in opeKindTable.Rows)
            {
                FormOpeCal formOpeCal = new FormOpeCal(r["ID"].ToString());
                formOpeCal.MdiParent = this;
                formOpeCal.WindowState = FormWindowState.Normal;
                formOpeCal.Show();
                formOpeCal.Update();
            }

            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void PtIdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.PtTwinkle(this.PtIdBox.Text);
            }
            else if (e.KeyCode == Keys.F3)
            {
                PatBase p = FormFindPat.FindPat();
                this.PtIdBox.Text = p.Id;

                int i = 0;

                if (this.PtIdBox.Text.Length > 0 && int.TryParse(this.PtIdBox.Text, out i))
                {
                    this.PtTwinkle(this.PtIdBox.Text);
                }
            }
        }

        public void PtTwinkle(string pt_id)
        {
            int i = 0;

            if (pt_id.Length > 0 && int.TryParse(pt_id, out i))
            {
                this.PtIdBox.Text = pt_id;
                this.PtInfoBox.Text = PatBase.Load(pt_id).Info1;

                foreach (FormOpeCal c in this.MdiChildren)
                {
                    c.PtTwinkle(pt_id);
                }
            }
        }

        private void FileCloseMenuItem_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void WindowHorizontalMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void WindowVerticalMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void ViewUpdateMenuItem_Click(object sender, EventArgs e)
        {
            foreach (FormOpeCal fop in this.MdiChildren)
            {
                fop.RsvTableShow();
            }
        }

        private void ToolPatMenuItem_Click(object sender, EventArgs e)
        {
            FormControl.FormPat_Show();
        }

        private void ToolOpeFindMenuItem_Click(object sender, EventArgs e)
        {
            FormFindOpeRecord f = new FormFindOpeRecord();
            f.ShowDialog();
        }

        private void FileExitMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("èIóπÇµÇ‹Ç∑Ç©ÅH", "ämîF", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                Application.Exit();
            }
        }
    }
}