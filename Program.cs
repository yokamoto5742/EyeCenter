using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    static class Program
    {
        /*
        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        static extern IntPtr SendMessage(IntPtr hWnd, IntPtr Msg, IntPtr wParam, IntPtr lParam);
        */

        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
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
                    string[] args = Environment.GetCommandLineArgs();
                    string arg = "";

                    for (int i = 0; i < args.Length; i++)
                    {
                        if (i > 0)
                        {
                            arg += " ";
                        }

                        arg += args[i];
                    }

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