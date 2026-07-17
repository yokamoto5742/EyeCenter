using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace EyeCenter
{
    /// <summary>
    /// XML マスターの DataRow から動的にコントロールを生成する際の共通処理。
    /// FormPat・ControlSumPage・KensaPanelDetail の生成コードで使用する。
    /// 値の比較は大文字小文字を区別しない。
    /// </summary>
    internal static class DynamicControl
    {
        /// <summary>
        /// Ime 列の値（Hiragana / Disable / Off）を ImeMode に変換する。該当しない値は def を返す。
        /// </summary>
        internal static ImeMode GetImeMode(string ime, ImeMode def)
        {
            if (ime.Equals("Hiragana", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImeMode.Hiragana;
            }

            if (ime.Equals("Disable", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImeMode.Disable;
            }

            if (ime.Equals("Off", StringComparison.CurrentCultureIgnoreCase))
            {
                return ImeMode.Off;
            }

            return def;
        }

        /// <summary>
        /// Align 列の値（Left / Center / Right）を HorizontalAlignment に変換する。該当しない値は def を返す。
        /// </summary>
        internal static HorizontalAlignment GetTextAlign(string align, HorizontalAlignment def)
        {
            if (align.Equals("Center", StringComparison.CurrentCultureIgnoreCase))
            {
                return HorizontalAlignment.Center;
            }

            if (align.Equals("Right", StringComparison.CurrentCultureIgnoreCase))
            {
                return HorizontalAlignment.Right;
            }

            if (align.Equals("Left", StringComparison.CurrentCultureIgnoreCase))
            {
                return HorizontalAlignment.Left;
            }

            return def;
        }

        /// <summary>
        /// Align 列の値（Left / Center / Right）を Label 用の ContentAlignment に変換する。該当しない値は def を返す。
        /// </summary>
        internal static ContentAlignment GetContentAlign(string align, ContentAlignment def)
        {
            if (align.Equals("Center", StringComparison.CurrentCultureIgnoreCase))
            {
                return ContentAlignment.MiddleCenter;
            }

            if (align.Equals("Right", StringComparison.CurrentCultureIgnoreCase))
            {
                return ContentAlignment.MiddleRight;
            }

            if (align.Equals("Left", StringComparison.CurrentCultureIgnoreCase))
            {
                return ContentAlignment.MiddleLeft;
            }

            return def;
        }

        /// <summary>
        /// X, Y, Width, Height 列をコントロールに適用する（空欄・不正な値は変更しない）。
        /// Width と Height がともに未指定なら true を返す（Label の AutoSize 判定用）。
        /// </summary>
        internal static bool ApplyBounds(Control c, DataRow r)
        {
            int x = 0;
            int y = 0;

            if (int.TryParse(r["X"].ToString(), out x) && int.TryParse(r["Y"].ToString(), out y))
            {
                c.Location = new Point(x, y);
            }

            int w = 0;
            int h = 0;

            if (r["Width"].ToString().Length > 0 && int.TryParse(r["Width"].ToString(), out w))
            {
                c.Width = w;
            }

            if (r["Height"].ToString().Length > 0 && int.TryParse(r["Height"].ToString(), out h))
            {
                c.Height = h;
            }

            return w == 0 && h == 0;
        }

        /// <summary>
        /// Width / Height 列からコントロールのサイズを取得する。空欄は既定値を使う。
        /// </summary>
        internal static Size GetSize(DataRow r, int defWidth, int defHeight)
        {
            int w = defWidth;
            int h = defHeight;

            if (r["Width"].ToString().Length > 0)
            {
                int.TryParse(r["Width"].ToString(), out w);
            }

            if (r["Height"].ToString().Length > 0)
            {
                int.TryParse(r["Height"].ToString(), out h);
            }

            return new Size(w, h);
        }
    }
}
