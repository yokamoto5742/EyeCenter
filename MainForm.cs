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
                // メイン画面のサイズを設定ファイル(EyeCenter.exe.config)から反映する
                this.ClientSize = new Size(
                    AppConfig.GetInt("MainFormWidth", this.ClientSize.Width),
                    AppConfig.GetInt("MainFormHeight", this.ClientSize.Height));

                LibSettings.Init();

                // DBコマンドのタイムアウト秒数を設定ファイル(EyeCenter.exe.config)から反映する（既定値 60秒）
                DB.SetCommandTimeout(AppConfig.GetInt("DbCommandTimeout", 60));

                EyeDict.Init();
                FormControl.Init();

                this.InitShow(Environment.GetCommandLineArgs());

                // この時点でログインされていなければ終了
                if (LoginUser.Status == LoginUser.STATUS.NONE)
                {
                    MessageBox.Show("ログインされていないため終了します", "EyeCenter");
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
                else if (args[i].Equals("-r", StringComparison.CurrentCultureIgnoreCase))
                {
                    first_show = FIRST_SHOW.RSV;
                }
            }

            if (pat_id > 0)
            {
                FormControl.FormPat_Show(pat_id.ToString(), FormPat.Mode.SHOW);
            }

            if (first_show == FIRST_SHOW.RSV)
            {
                FormControl.FormOpeRsv_Show();
            }
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
            this.Dispose();
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            FormControl.FormPrint_Show();
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            // エクスポート許可ユーザーは設定ファイル(EyeCenter.exe.config)の ExportAllowedUsers で変更可能
            bool allowed = false;

            foreach (string id in AppConfig.GetString("ExportAllowedUsers", "519,363,305,752,1034").Split(','))
            {
                if (id.Trim().Equals(LoginUser.Id))
                {
                    allowed = true;
                    break;
                }
            }

            if (allowed)
            {
                FormExport fe = new FormExport();
                fe.ShowDialog();
            }
            else
            {
                MessageBox.Show("この機能は一部ユーザーしか使えません。ご了承ください。(m__m)", "限定機能");
            }
        }

    }
}
