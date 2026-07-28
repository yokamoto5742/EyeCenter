using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace EyeCenter
{
    /// <summary>
    /// Excel の起動コスト（Application 生成 約0.7秒 + テンプレート初回オープン 約0.5秒）を
    /// 帳票ボタン押下時に払わずに済むよう、あらかじめバックグラウンドで Excel を起動しておく。
    /// COM は MTA スレッドで生成して UI(STA) スレッドから使う。Excel はプロセス外の COM
    /// サーバのためアパートメントを跨いでも CLR が自動でマーシャリングし、速度低下は無い。
    ///
    /// Excel 2013 以降は1プロセスに全ブックを同居させるため、待機中の Excel に利用者が
    /// 開いたファイルが相乗りすることがある。相乗りされた Excel を掴んだまま設定を変えたり
    /// 終了したりすると利用者の作業を壊すので、待機中は既定の状態で置き、
    /// 引き渡し・終了の前に必ず未使用かどうかを確認する。
    /// </summary>
    static class ExcelWarmup
    {
        static readonly object sync = new object();

        /// <summary>
        /// 起動済みで未使用の Excel。Take で引き渡すと null に戻る。
        /// </summary>
        static Excel.Application warm;

        static Thread thread;

        /// <summary>
        /// 終了要求。起動処理の完了と入れ違いになった場合に待機させないためのフラグ。
        /// </summary>
        static bool shuttingDown;

        static Timer idleTimer;

        /// <summary>
        /// 使われないまま待機し続ける Excel を終了するまでの時間。
        /// 常駐時間が延びるほど利用者の Excel に相乗りされる可能性が上がるため短めにする。
        /// </summary>
        const int IdleTimeoutMs = 10 * 60 * 1000;

        /// <summary>
        /// バックグラウンドで Excel を起動する。起動済み・起動処理中なら何もしない。
        /// </summary>
        public static void Start()
        {
            if (!isEnabled())
            {
                return;
            }

            lock (sync)
            {
                shuttingDown = false;

                if (warm != null || (thread != null && thread.IsAlive))
                {
                    return;
                }

                thread = new Thread(warmUp);
                thread.IsBackground = true;
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
            }
        }

        /// <summary>
        /// 起動済みの Excel を受け取る。受け取った後の解放は呼び出し側の責任。
        /// まだ準備できていない場合と、利用者のブックが相乗りしている場合は null を返すので、
        /// 呼び出し側で新規生成すること。
        /// </summary>
        public static Excel.Application Take()
        {
            Excel.Application app;

            lock (sync)
            {
                app = warm;
                warm = null;
                stopIdleTimer();
            }

            if (app == null)
            {
                return null;
            }

            if (isUnused(app))
            {
                return app;
            }

            // 利用者がこの Excel でファイルを開いている。描画停止などの設定を被せると
            // 作業中の画面を壊すため、使い回さずに参照だけ手放す（Quit はしない）。
            release(app);
            return null;
        }

        /// <summary>
        /// 使われないまま残っている Excel を終了する。アプリ終了時に必ず呼ぶこと。
        /// </summary>
        public static void Shutdown()
        {
            Shutdown(true);
        }

        /// <summary>
        /// 使われないまま残っている Excel を終了する。
        /// wait が true の場合は起動処理の完了を待ってから片付ける（解放漏れを防ぐ）。
        /// UI スレッドから呼ぶ場合は待たせないよう false を渡すこと。
        /// </summary>
        public static void Shutdown(bool wait)
        {
            Thread t;

            lock (sync)
            {
                shuttingDown = true;
                stopIdleTimer();
                t = thread;
            }

            if (wait && t != null)
            {
                t.Join(5000);
            }

            Excel.Application app;

            lock (sync)
            {
                app = warm;
                warm = null;
            }

            if (app != null)
            {
                quit(app);
            }
        }

        static void warmUp()
        {
            Excel.Application app = null;

            try
            {
                app = new Excel.Application();
                app.Visible = false;
                app.EnableEvents = false;
                app.ScreenUpdating = false;
                app.DisplayAlerts = false;

                // テンプレートを一度開いて閉じておくと、本番のオープンが約680ms→約200msになる
                preload(app, ExcelControl.GetNsFileName());
                preload(app, ExcelControl.GetRecordFileName());

                // 待機中に利用者のブックが相乗りすると、描画停止・警告抑止のままでは
                // 画面が再描画されず「空の Excel が開いたまま」に見える。必ず既定へ戻す。
                app.ScreenUpdating = true;
                app.EnableEvents = true;
                app.DisplayAlerts = true;

                lock (sync)
                {
                    if (!shuttingDown)
                    {
                        warm = app;
                        app = null;
                        startIdleTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                // 起動できなかった場合はウォームアップ無しで動く（従来どおり都度生成）。
                // 遅いままの原因が分かるようログには残す。
                Program.WriteErrorLog("ExcelWarmup", ex);
            }
            finally
            {
                if (app != null)
                {
                    quit(app);
                }
            }
        }

        /// <summary>
        /// テンプレートを読み取り専用で開いて閉じる。UpdateLinks=0・AddToMru=false を指定し、
        /// 外部リンクの解決と最近使ったファイル一覧への追加を行わない。
        /// </summary>
        static void preload(Excel.Application app, string fileName)
        {
            if (!File.Exists(fileName))
            {
                return;
            }

            Excel._Workbook book = (Excel._Workbook)(app.Workbooks.Open(fileName,
                0, true, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, false,
                Missing.Value, Missing.Value));

            book.Close(false, Missing.Value, Missing.Value);
            Marshal.ReleaseComObject(book);
        }

        /// <summary>
        /// 未使用（非表示でブックが1冊も無い）ことが確認できたときだけ true を返す。
        /// 判定できない場合は false とし、利用者のブックを巻き込まないようにする。
        /// </summary>
        static bool isUnused(Excel.Application app)
        {
            Excel.Workbooks books = null;

            try
            {
                if (app.Visible)
                {
                    return false;
                }

                books = app.Workbooks;
                return books.Count == 0;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (books != null)
                {
                    Marshal.ReleaseComObject(books);
                }
            }
        }

        /// <summary>
        /// 未使用の場合だけ Excel を終了して参照を解放する。
        /// 相乗りされている場合は終了させず参照だけ手放す（利用者のブックを閉じないため）。
        /// </summary>
        static void quit(Excel.Application app)
        {
            try
            {
                if (isUnused(app))
                {
                    // 保存確認を握りつぶしたまま終了しないよう、警告表示を戻してから終了する
                    app.DisplayAlerts = true;
                    app.Quit();
                }
            }
            catch
            {
            }
            finally
            {
                release(app);
            }
        }

        static void release(Excel.Application app)
        {
            try
            {
                Marshal.ReleaseComObject(app);
            }
            catch
            {
            }
        }

        /// <summary>
        /// アイドルタイマーを開始する。lock (sync) の中から呼ぶこと。
        /// </summary>
        static void startIdleTimer()
        {
            stopIdleTimer();
            idleTimer = new Timer(onIdleTimeout, null, IdleTimeoutMs, Timeout.Infinite);
        }

        /// <summary>
        /// アイドルタイマーを停止する。lock (sync) の中から呼ぶこと。
        /// </summary>
        static void stopIdleTimer()
        {
            if (idleTimer != null)
            {
                idleTimer.Dispose();
                idleTimer = null;
            }
        }

        static void onIdleTimeout(object state)
        {
            Excel.Application app;

            lock (sync)
            {
                app = warm;
                warm = null;
                stopIdleTimer();
            }

            if (app != null)
            {
                quit(app);
            }
        }

        /// <summary>
        /// EyeDataSettings.ini の [EXCEL_SETTINGS] EXCEL_WARMUP=0 で事前起動を止められる。
        /// 現場で不具合が出た場合に exe を差し替えずに無効化するための逃げ道。
        /// </summary>
        static bool isEnabled()
        {
            return !ExcelControl.readIniValue("EXCEL_WARMUP", "1").Equals("0");
        }
    }
}
