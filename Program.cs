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