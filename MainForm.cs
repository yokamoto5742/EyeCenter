using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class MainForm : StdForm1
    {
        enum FIRST_SHOW : int
        {
            MAIN = 0,
            LIST = 1,
            PAT = 2,
            RSV = 3
        }

        FIRST_SHOW first_show = FIRST_SHOW.MAIN;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WinAPI.WM_COPYDATA)
            {
                // 文字列が送信されて来た
                WinAPI.COPYDATASTRUCT mystr = new WinAPI.COPYDATASTRUCT();
                Type mytype = mystr.GetType();
                mystr = (WinAPI.COPYDATASTRUCT)m.GetLParam(mytype);

                if (mystr.lpData.Split(' ').Length > 0)
                {
                    this.InitShow(mystr.lpData.Split(' '));
                }
            }

            base.WndProc(ref m);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                LibSettings.Init();
                EyeDict.Init();
                FormControl.Init();

                this.InitShow(Environment.GetCommandLineArgs());

                // この時点でログインされていなければ終了
                if (LoginUser.Status == LoginUser.STATUS.NONE)
                {
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                LibUtility.Except(ex);
                this.Dispose();
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
        }

        void InitShow(string[] args = null)
        {
            LoginUser.Init(true, args);

            int pat_id = 0;

            // Pat.csv を見る
            int.TryParse(PatBase.ReadPatCSV().Id, out pat_id);

            for (int i = 1; i < args.Length; i++)
            {
                if (args[i].Equals("-p", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (i < args.Length - 1 && int.TryParse(args[i + 1], out pat_id))
                    {
                        // 次のパラメータが数字ならば患者IDとみなす
                        i++;
                    }
                }
                else if (args[i].Equals("-l", StringComparison.CurrentCultureIgnoreCase))
                {
                    first_show = FIRST_SHOW.LIST;
                }
                else if (args[i].Equals("-r", StringComparison.CurrentCultureIgnoreCase))
                {
                    first_show = FIRST_SHOW.RSV;
                }
            }

            if (pat_id > 0)
            {
                FormControl.FormPat_Show(pat_id.ToString(), FormPat.Mode.SHOW);
            }

            if (first_show == FIRST_SHOW.LIST)
            {
                FormControl.FormList_Show();
            }
            else if (first_show == FIRST_SHOW.RSV)
            {
                FormControl.FormOpeRsv_Show();
            }
        }

        private void ListButton_Click(object sender, EventArgs e)
        {
            FormControl.FormList_Show();
        }

        private void PatButton_Click(object sender, EventArgs e)
        {
            FormControl.FormPat_Show();
        }

        private void RsvButton_Click(object sender, EventArgs e)
        {
            FormControl.FormOpeRsv_Show();
        }

        private void OpeFindButton_Click(object sender, EventArgs e)
        {
            FormControl.FormFindOpeRecord_Show();
        }

        private void KensaFindButton_Click(object sender, EventArgs e)
        {
            FormControl.FormFindKensa_Show();
        }

        private void SummaryFindButton_Click(object sender, EventArgs e)
        {
            FormControl.FormFindSummary_Show();
        }

        private void CanonButton_Click(object sender, EventArgs e)
        {
            Launcher.Start("CanonRKF1.exe");
        }

        private void NidekButton_Click(object sender, EventArgs e)
        {
            Launcher.Start("NidekARK1.exe");
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (FormControl.FormPat_Count > 0)
            {
                if (MessageBox.Show("患者画面が開かれています。終了してもよろしいですか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Dispose();
                }
            }
            else
            {
                if (MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == System.Windows.Forms.DialogResult.Yes)
                {
                    this.Dispose();
                }
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            FormControl.FormPrint_Show();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            if (LoginUser.Id.Equals("519") || LoginUser.Id.Equals("363") || LoginUser.Id.Equals("305") || LoginUser.Id.Equals("752") || LoginUser.Id.Equals("1034"))
            {
                FormExport fe = new FormExport();
                fe.ShowDialog();
            }
            else
            {
                MessageBox.Show("この機能は一部ユーザーしか使えません。ご了承ください。(m__m)", "限定機能");
            }
        }

        private void OpeOrderButton_Click(object sender, EventArgs e)
        {
            Launcher.OpeOrder();
        }

        private void MAFormPatListButton1_Click(object sender, EventArgs e)
        {
            MedicalLibrary.Boundary.FormControl.FormPatList_Show();
        }

        private void MAFormPatButton1_Click(object sender, EventArgs e)
        {
            MedicalLibrary.Boundary.FormControl.FormPat_Show(new PatBase());
        }

        private void RsvListButton_Click(object sender, EventArgs e)
        {
            FormControl.FormRsvPatList_Show();
        }

        private void MultiPatBox_CheckedChanged(object sender, EventArgs e)
        {
            // 多人数モードをオフにする時の注意
            if (!this.MultiPatBox.Checked && FormControl.FormPat_Count > 1)
            {
                MessageBox.Show("多人数モードをオフにする際は、患者台帳は１つだけにするか、全部閉じてからお願いします。");
                this.MultiPatBox.Checked = true;
                return;
            }

            FormControl.MultiPat = this.MultiPatBox.Checked;
        }
    }
}