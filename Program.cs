using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Oracle.DataAccess は厳密名のため、config のリダイレクト先とパッチバージョンが
            // 一致しないと読み込めない（本番機の 2.112 系はパッチ版が環境ごとに異なる）。
            // 解決失敗時は exe と同じフォルダの DLL をバージョン不問で読み込む。
            AppDomain.CurrentDomain.AssemblyResolve += ResolveOracleAssembly;

            // ワーカースレッド（検索など）で例外が起きた場合、既定では何も表示されずに
            // プロセスが終了してしまい原因が分からないため、必ずログに残す。
            AppDomain.CurrentDomain.UnhandledException += LogUnhandledException;
            Application.ThreadException += LogThreadException;

            try
            {
                MainBody();
            }
            catch (Exception ex)
            {
                // Application.Run 前の例外は既定では何も表示されずに終了するため、必ず表示する
                MessageBox.Show(ex.ToString(), "起動エラー");
            }
        }

        /// <summary>
        /// 例外の内容を exe と同じフォルダの EyeData_error.log に追記する。
        /// ログ出力自体で落ちないよう、失敗しても無視する。
        /// </summary>
        internal static void WriteErrorLog(string kind, object error)
        {
            try
            {
                string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EyeData_error.log");

                string text = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " [" + kind + "]" + Environment.NewLine +
                    (error == null ? "(null)" : error.ToString()) + Environment.NewLine +
                    "----------------------------------------" + Environment.NewLine;

                System.IO.File.AppendAllText(path, text, System.Text.Encoding.UTF8);
            }
            catch (Exception)
            {
            }
        }

        static void LogUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            WriteErrorLog("UnhandledException", e.ExceptionObject);
        }

        static void LogThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            WriteErrorLog("ThreadException", e.Exception);

            MessageBox.Show(e.Exception.ToString(), "エラー");
        }

        static System.Reflection.Assembly ResolveOracleAssembly(object sender, ResolveEventArgs args)
        {
            if (new System.Reflection.AssemblyName(args.Name).Name != "Oracle.DataAccess")
            {
                return null;
            }

            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Oracle.DataAccess.dll");
            return System.IO.File.Exists(path) ? System.Reflection.Assembly.LoadFrom(path) : null;
        }

        static void MainBody()
        {
            // すでに起動しているか
            bool proc = false;
            Process[] procs = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);

            if (procs.Length > 1)
            {
                IntPtr hWnd = WinAPI.FindWindow(null, "EyeCenter");

                if (hWnd != IntPtr.Zero)
                {
                    proc = true;
                    WinAPI.SetForegroundWindow(hWnd);

                    // 受けたパラメータを送る
                    string arg = string.Join(" ", Environment.GetCommandLineArgs());

                    WinAPI.COPYDATASTRUCT cds;
                    cds.dwData = new IntPtr(0);
                    cds.lpData = arg;
                    cds.cbData = new IntPtr(cds.lpData.Length + 1);

                    WinAPI.SendMessage(hWnd, new IntPtr(WinAPI.WM_COPYDATA), IntPtr.Zero, ref cds);
                }
            }

            // 起動しておらず、メッセージを送る相手が無い場合
            if (!proc)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());
            }
        }
    }
}