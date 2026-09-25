using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormBase : StdForm1
    {
        /// <summary>
        /// タイトルバーのオリジナル名称。
        /// </summary>
        protected string OriginalText = "";

        public FormBase()
        {
            InitializeComponent();
        }

        /// <summary>
        /// タイトルバーの名称を表示する。（ログインユーザー名付き）
        /// </summary>
        protected void FormTextShow()
        {
            this.Text = OriginalText + "（" + LoginUser.Name + " ログイン中）";
        }

        protected void FormBase_Paint(object sender, PaintEventArgs e)
        {
            this.FormTextShow();
        }
    }
}