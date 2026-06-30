using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EyeCenter
{
    public partial class FormInput : Form
    {
        public enum Mode : int
        {
            Comment = 0,
            PatientId = 1
        }

        public FormInput()
        {
            InitializeComponent();
        }

        public void ModeChange(Mode mode)
        {
            if (mode == Mode.PatientId)
            {
                this.CommentLabel.Text = "Š³ÒID‚ğ“ü—Í‚µ‚Ä‚­‚¾‚³‚¢";
                this.CommentBox.ImeMode = ImeMode.Disable;
            }
            else
            {
                this.CommentLabel.Text = "ƒRƒƒ“ƒg‚ª‚ ‚ê‚Î“ü—Í‚µ‚Ä‚­‚¾‚³‚¢";
                this.CommentBox.ImeMode = ImeMode.Hiragana;
            }
        }

        public void Clear()
        {
            this.CommentBox.Clear();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}