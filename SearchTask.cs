using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// 検索をバックグラウンドスレッドで実行し、進行中ダイアログ（中止ボタン付き）を表示する。
    /// 共有DB接続（DB.Db2/Db3）はスレッドセーフではないため、
    /// 検索専用の接続 EyeDb / PatDb を作成してワーカースレッドから使用する。
    /// </summary>
    internal class SearchTask : Form
    {
        /// <summary>検索専用の眼科DB接続（DB.Db2 と同じ接続先）</summary>
        public DB EyeDb = new DB();

        /// <summary>検索専用の患者マスタDB接続（DB.Db3 と同じ接続先）</summary>
        public DB PatDb = new DB();

        Thread Worker;
        volatile bool Completed = false;
        bool Cancelled = false;
        object Result = null;
        Exception Error = null;

        Button StopButton = new Button();
        System.Windows.Forms.Timer PollTimer = new System.Windows.Forms.Timer();

        SearchTask(string message)
        {
            this.Text = "検索中";
            this.ClientSize = new Size(320, 100);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.ControlBox = false;
            this.MinimizeBox = false;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            Label label = new Label();
            label.Text = message;
            label.AutoSize = true;
            label.Location = new Point(20, 20);
            this.Controls.Add(label);

            StopButton.Text = "中止";
            StopButton.Size = new Size(80, 28);
            StopButton.Location = new Point(120, 60);
            StopButton.Click += new EventHandler(StopButton_Click);
            this.Controls.Add(StopButton);

            PollTimer.Interval = 200;
            PollTimer.Tick += new EventHandler(PollTimer_Tick);
            PollTimer.Start();
        }

        void PollTimer_Tick(object sender, EventArgs e)
        {
            if (Completed)
            {
                PollTimer.Stop();
                this.DialogResult = DialogResult.OK;
            }
        }

        void StopButton_Click(object sender, EventArgs e)
        {
            Cancelled = true;
            StopButton.Enabled = false;

            // 実行中のSQLをキャンセルする（OracleCommand.Cancel は別スレッドから呼び出せる）
            try { EyeDb.Command.Cancel(); } catch (Exception) { }
            try { PatDb.Command.Cancel(); } catch (Exception) { }

            PollTimer.Stop();
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// 検索処理をバックグラウンドで実行する。
        /// 中止時・エラー時は null を返す（エラーはメッセージ表示する）。
        /// search の中では引数の SearchTask が持つ EyeDb / PatDb だけを使い、
        /// 共有接続（DB.Db2 等）には触れないこと。
        /// </summary>
        /// <param name="message">ダイアログに表示するメッセージ</param>
        /// <param name="search">検索処理</param>
        public static T Run<T>(string message, Func<SearchTask, T> search) where T : class
        {
            SearchTask form = new SearchTask(message);

            form.EyeDb.Init(DB.Db2.InitString);
            form.PatDb.Init(DB.Db3.InitString);

            form.Worker = new Thread(delegate ()
            {
                try
                {
                    form.Result = search(form);
                }
                catch (Exception ex)
                {
                    form.Error = ex;
                }
                finally
                {
                    // 接続はワーカースレッドが所有し、終了時に必ず破棄する
                    try { form.EyeDb.Connection.Dispose(); } catch (Exception) { }
                    try { form.PatDb.Connection.Dispose(); } catch (Exception) { }

                    form.Completed = true;
                }
            });

            form.Worker.IsBackground = true;
            form.Worker.Start();

            // 一瞬で終わる検索ではダイアログを表示しない
            form.Worker.Join(200);

            if (!form.Completed)
            {
                form.ShowDialog();
            }

            T result = null;

            if (form.Cancelled)
            {
                // 中止済み。結果は破棄する
            }
            else if (form.Error != null)
            {
                MessageBox.Show(form.Error.Message);
            }
            else
            {
                result = (T)form.Result;
            }

            form.Dispose();

            return result;
        }
    }
}
