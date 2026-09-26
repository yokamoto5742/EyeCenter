using System;
using System.Linq;
using System.Media;
using System.Windows.Forms;

namespace EyeCenter
{
    /// <summary>
    /// バーコードリーダー（キーボード入力方式）で36桁バーコードを読み取ったら、その患者の患者台帳を開く。
    /// リーダーは人の打鍵よりずっと速く文字を送るため、打鍵間隔が短い数字36桁＋Enter をバーコードとみなす。
    /// 読み取りでフォーカス先に入った数字は元に戻し、Enter は握りつぶす（既定ボタンなどを押させないため）。
    /// </summary>
    internal class BarcodeKeyFilter : IMessageFilter
    {
        const int WM_KEYDOWN = 0x0100;
        const int WM_CHAR = 0x0102;

        internal const int CodeLength = 36;

        /// <summary>
        /// バーコードとみなす打鍵間隔の上限（ミリ秒）
        /// </summary>
        readonly int maxInterval;

        string buffer = "";
        int lastTick;

        // 読み取り開始時点のフォーカス先の入力内容（読み取った数字を取り除くため）
        Control target;
        string targetText;
        int targetSelStart;
        int targetSelLength;

        public BarcodeKeyFilter(int maxInterval)
        {
            this.maxInterval = maxInterval;
        }

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == WM_CHAR)
            {
                char c = (char)m.WParam.ToInt32();

                if (c >= '0' && c <= '9')
                {
                    if (this.AddDigit(c, Environment.TickCount))
                    {
                        this.SaveTarget(Control.FromChildHandle(m.HWnd));
                    }
                }
                else
                {
                    this.buffer = "";
                }
            }
            else if (m.Msg == WM_KEYDOWN && (Keys)m.WParam.ToInt32() == Keys.Enter)
            {
                string code = this.Complete(Environment.TickCount);

                if (code != null)
                {
                    this.RestoreTarget();
                    this.OpenPat(code);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 数字を1文字受け取る。新しい読み取りの始まりなら true を返す。
        /// </summary>
        internal bool AddDigit(char c, int tick)
        {
            bool start = this.buffer.Length == 0 || this.buffer.Length >= CodeLength || unchecked(tick - this.lastTick) > this.maxInterval;

            if (start)
            {
                this.buffer = "";
            }

            this.buffer += c;
            this.lastTick = tick;

            return start;
        }

        /// <summary>
        /// Enter を受けたときに呼ぶ。バーコードとみなせればその値を、そうでなければ null を返す。
        /// </summary>
        internal string Complete(int tick)
        {
            string code = this.buffer;
            this.buffer = "";

            if (code.Length == CodeLength && unchecked(tick - this.lastTick) <= this.maxInterval)
            {
                return code;
            }

            return null;
        }

        /// <summary>
        /// 36桁バーコードの先頭9桁（患者ID）を取り出す。患者IDとして不正なら null を返す。
        /// </summary>
        internal static string ParsePatientId(string code)
        {
            int id;

            if (code.Length == CodeLength && int.TryParse(code.Substring(0, 9), out id) && id > 0)
            {
                return id.ToString();
            }

            return null;
        }

        void SaveTarget(Control c)
        {
            this.target = null;

            TextBoxBase tb = c as TextBoxBase;
            ComboBox cb = c as ComboBox;

            if (tb != null)
            {
                this.targetSelStart = tb.SelectionStart;
                this.targetSelLength = tb.SelectionLength;
            }
            else if (cb != null && cb.DropDownStyle != ComboBoxStyle.DropDownList)
            {
                this.targetSelStart = cb.SelectionStart;
                this.targetSelLength = cb.SelectionLength;
            }
            else
            {
                return;
            }

            this.target = c;
            this.targetText = c.Text;
        }

        void RestoreTarget()
        {
            Control c = this.target;
            this.target = null;

            if (c == null || c.IsDisposed || c.Text.Equals(this.targetText))
            {
                return;
            }

            c.Text = this.targetText;

            if (c is TextBoxBase)
            {
                ((TextBoxBase)c).Select(this.targetSelStart, this.targetSelLength);
            }
            else if (c is ComboBox)
            {
                ((ComboBox)c).Select(this.targetSelStart, this.targetSelLength);
            }
        }

        void OpenPat(string code)
        {
            string pt_id = ParsePatientId(code);

            // 検索画面などのダイアログ表示中は、その裏で患者台帳を開かない
            if (pt_id == null || Application.OpenForms.Cast<Form>().Any(f => f.Modal))
            {
                SystemSounds.Beep.Play();
                return;
            }

            FormControl.FormPat_ShowByBarcode(pt_id);
        }
    }
}
