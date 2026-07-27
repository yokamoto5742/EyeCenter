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
        /// バックグラウンドで Excel を起動する。起動済み・起動処理中なら何もしない。
        /// </summary>
        public static void Start()
        {
            lock (sync)
            {
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
        /// まだ準備できていなければ null を返すので、呼び出し側で新規生成すること。
        /// </summary>
        public static Excel.Application Take()
        {
            lock (sync)
            {
                Excel.Application app = warm;
                warm = null;
                return app;
            }
        }

        /// <summary>
        /// 使われないまま残っている Excel を終了する。アプリ終了時に必ず呼ぶこと。
        /// </summary>
        public static void Shutdown()
        {
            Thread t;

            lock (sync)
            {
                t = thread;
            }

            if (t != null)
            {
                // 起動処理中に終了すると解放漏れになるため、完了を待ってから片付ける
                t.Join(5000);
            }

            Excel.Application app = Take();

            if (app == null)
            {
                return;
            }

            try
            {
                app.Quit();
            }
            catch
            {
            }

            Marshal.ReleaseComObject(app);
        }

        static void warmUp()
        {
            Excel.Application app = null;

            try
            {
                app = new Excel.Application();
                app.EnableEvents = false;
                app.ScreenUpdating = false;
                app.DisplayAlerts = false;

                // テンプレートを一度開いて閉じておくと、本番のオープンが約680ms→約200msになる
                preload(app, ExcelControl.GetNsFileName());
                preload(app, ExcelControl.GetRecordFileName());

                lock (sync)
                {
                    warm = app;
                    app = null;
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
                    try
                    {
                        app.Quit();
                    }
                    catch
                    {
                    }

                    Marshal.ReleaseComObject(app);
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
    }
}
