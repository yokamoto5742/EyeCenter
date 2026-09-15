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
        /// withSize が true の場合は大きさも保存・復元する（最大化・最小化中は通常時の大きさ）。
        /// </summary>
        internal static void Attach(Form form, string key, bool withSize = false)
        {
            Point p;

            if (withSize && TryParse(Load(key + "_Size"), out p) && p.X > 0 && p.Y > 0)
            {
                form.Size = new Size(p.X, p.Y);
            }

            if (TryParse(Load(key), out p) && IsVisible(p, WorkingAreas()))
            {
                form.Location = p;
            }

            if (withSize)
            {
                form.Bounds = FitTo(form.Bounds, Screen.FromPoint(new Point(form.Left + 50, form.Top + 10)).WorkingArea);
            }

            // 終了ボタンは Dispose() で閉じるため FormClosing ではなく HandleDestroyed で保存する
            form.HandleDestroyed += (sender, e) =>
            {
                Rectangle b = form.WindowState == FormWindowState.Normal ? form.Bounds : form.RestoreBounds;
                Save(key, b.X + "," + b.Y);

                if (withSize)
                {
                    Save(key + "_Size", b.Width + "," + b.Height);
                }
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

        /// <summary>
        /// 作業領域より幅・高さが大きい場合は作業領域の大きさに縮め、その方向の位置を作業領域の端に合わせる。
        /// </summary>
        internal static Rectangle FitTo(Rectangle b, Rectangle area)
        {
            if (b.Width > area.Width)
            {
                b.Width = area.Width;
                b.X = area.X;
            }

            if (b.Height > area.Height)
            {
                b.Height = area.Height;
                b.Y = area.Y;
            }

            return b;
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
