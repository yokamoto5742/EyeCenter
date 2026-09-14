using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EyeCenter
{
    /// <summary>
    /// 画面の位置を %LOCALAPPDATA%\EyeCenter に保存し、次回表示時に復元する。
    /// </summary>
    internal static class WindowPosition
    {
        /// <summary>
        /// 前回終了時の位置を復元し、画面が閉じられたときに位置を保存するよう登録する。
        /// 保存位置がどのモニターの作業領域にも入っていない場合は復元しない（既定位置のまま）。
        /// </summary>
        internal static void Attach(Form form, string key)
        {
            Point p;

            if (TryParse(Load(key), out p) && IsVisible(p, WorkingAreas()))
            {
                form.Location = p;
            }

            // 終了ボタンは Dispose() で閉じるため FormClosing ではなく HandleDestroyed で保存する
            form.HandleDestroyed += (sender, e) =>
            {
                Point loc = form.WindowState == FormWindowState.Normal ? form.Location : form.RestoreBounds.Location;
                Save(key, loc.X + "," + loc.Y);
            };
        }

        internal static bool TryParse(string s, out Point p)
        {
            p = Point.Empty;

            if (string.IsNullOrEmpty(s))
            {
                return false;
            }

            string[] xy = s.Split(',');
            int x, y;

            if (xy.Length != 2 || !int.TryParse(xy[0], out x) || !int.TryParse(xy[1], out y))
            {
                return false;
            }

            p = new Point(x, y);
            return true;
        }

        /// <summary>
        /// タイトルバー左上付近がいずれかの作業領域内にあれば true。
        /// </summary>
        internal static bool IsVisible(Point p, Rectangle[] areas)
        {
            Point title = new Point(p.X + 50, p.Y + 10);

            foreach (Rectangle area in areas)
            {
                if (area.Contains(title))
                {
                    return true;
                }
            }

            return false;
        }

        static Rectangle[] WorkingAreas()
        {
            Screen[] screens = Screen.AllScreens;
            Rectangle[] areas = new Rectangle[screens.Length];

            for (int i = 0; i < screens.Length; i++)
            {
                areas[i] = screens[i].WorkingArea;
            }

            return areas;
        }

        static string PositionFile(string key)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"EyeCenter\WindowPosition_" + key + ".txt");
        }

        static string Load(string key)
        {
            try
            {
                if (File.Exists(PositionFile(key)))
                {
                    return File.ReadAllText(PositionFile(key)).Trim();
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        static void Save(string key, string value)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PositionFile(key)));

                File.WriteAllText(PositionFile(key), value);
            }
            catch (Exception)
            {
            }
        }
    }
}
