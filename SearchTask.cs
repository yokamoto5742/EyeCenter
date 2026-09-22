using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// 検索・一覧作成・出力をバックグラウンドスレッドで実行し、進行中ダイアログ（進捗表示・中止ボタン付き）を表示する。
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
        volatile bool Cancelled = false;
        volatile string Progress = "";
        object Result = null;
        Exception Error = null;

        Label ProgressLabel = new Label();
        Button StopButton = new Button();
        System.Windows.Forms.Timer PollTimer = new System.Windows.Forms.Timer();

        SearchTask(string message)
        {
            this.Text = "処理中";
            this.ClientSize = new Size(320, 120);
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

            ProgressLabel.AutoSize = true;
            ProgressLabel.Location = new Point(20, 45);
            this.Controls.Add(ProgressLabel);

            StopButton.Text = "中止";
            StopButton.Size = new Size(80, 28);
            StopButton.Location = new Point(120, 80);
            StopButton.Click += new EventHandler(StopButton_Click);
            this.Controls.Add(StopButton);

            PollTimer.Interval = 200;
            PollTimer.Tick += new EventHandler(PollTimer_Tick);
            PollTimer.Start();
        }

        /// <summary>
        /// 進捗をダイアログに表示する（ワーカースレッドから呼び出す）。
        /// 中止済みの場合は OperationCanceledException を投げて処理を打ち切る。
        /// </summary>
        /// <param name="text">表示する進捗</param>
        public void Report(string text)
        {
            if (Cancelled)
            {
                throw new OperationCanceledException();
            }

            Progress = text;
        }

        void PollTimer_Tick(object sender, EventArgs e)
        {
            ProgressLabel.Text = Progress;

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

            // 実行中のSQLをキャンセルする（別スレッドから呼び出してよい）
            try { EyeDb.CancelCommand(); } catch (Exception) { }
            try { PatDb.CancelCommand(); } catch (Exception) { }

            PollTimer.Stop();
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Excel に出力する（書き込みはワーカーで行い、進捗を表示する）。
        /// </summary>
        /// <param name="data">出力データ</param>
        public static void ExcelOpen(TableData data)
        {
            if (Run("Excelに出力しています...", t => data.ExcelOpen(true, t.Report)))
            {
                MessageBox.Show("Excel出力が完了しました");
            }
        }

        /// <summary>
        /// 保存先を選んで CSV に出力する（書き込みはワーカーで行い、進捗を表示する）。
        /// </summary>
        /// <param name="data">出力データ</param>
        /// <param name="file">保存先ダイアログの初期ファイル名</param>
        public static void CSVSave(TableData data, string file)
        {
            string save_file = TableData.SelectSaveFile(file);

            if (save_file.Length == 0)
            {
                return;
            }

            if (Run("CSVを出力しています...", t => data.CSVWrite(save_file, false, true, t.Report)))
            {
                MessageBox.Show("出力が完了しました");
            }
        }

        /// <summary>
        /// 結果が成否（bool）の処理をバックグラウンドで実行する。
        /// 中止時・エラー時は false を返す。
        /// </summary>
        /// <param name="message">ダイアログに表示するメッセージ</param>
        /// <param name="work">処理</param>
        public static bool Run(string message, Func<SearchTask, bool> work)
        {
            return Run<object>(message, t => work(t) ? new object() : null) != null;
        }

        /// <summary>
        /// 検索処理をバックグラウンドで実行する。
        /// 中止時・エラー時は null を返す（エラーはメッセージ表示する）。
        /// search の中では引数の SearchTask が持つ EyeDb / PatDb だけを使い、
        /// 共有接続（DB.Db2 等）やフォームのコントロールには触れないこと。
        /// 進捗は Report で表示する。
        /// </summary>
        /// <param name="message">ダイアログに表示するメッセージ</param>
        /// <param name="search">検索処理</param>
        public static T Run<T>(string message, Func<SearchTask, T> search) where T : class
        {
            SearchTask form = new SearchTask(message);

            form.EyeDb.Init(DB.Db2.InitString);
            form.PatDb.Init(DB.Db3.InitString);

            // 大量件数の検索で通信の往復回数を減らす
            form.EyeDb.SetFetchSize(4 * 1024 * 1024);
            form.PatDb.SetFetchSize(4 * 1024 * 1024);

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
                    try { form.EyeDb.DisposeConnection(); } catch (Exception) { }
                    try { form.PatDb.DisposeConnection(); } catch (Exception) { }

                    form.Completed = true;
                }
            });

            // Excel（COM）への出力もワーカーで行うため STA にする
            form.Worker.SetApartmentState(ApartmentState.STA);
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
                // 表示するのはメッセージだけなので、原因追跡用にスタックトレースをログへ残す
                Program.WriteErrorLog("SearchTask", form.Error);

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
