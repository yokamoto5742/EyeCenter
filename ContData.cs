using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace EyeCenter
{
    /// <summary>
    /// Cont 形式文字列の相互変換。
    /// Cont 形式は "code,value" の行を改行区切りで並べたもので、
    /// value 内の改行は &lt;CR+LF&gt; トークンに置き換えて 1 行に収める。
    /// 検査（Cont）・手術記録・経過記録・サマリー（Cont1〜Cont4）で共通に使う。
    /// </summary>
    static class ContData
    {
        /// <summary>value 内の改行を表すトークン。</summary>
        public const string NewLineToken = "<CR+LF>";

        /// <summary>
        /// Cont 文字列を code → value の Dictionary に展開する。
        /// value 内のカンマは保持し、&lt;CR+LF&gt; トークンは改行へ戻す。
        /// code のみで値の無い行は無視する。同一 code の行が複数ある場合は先勝ち。
        /// </summary>
        /// <param name="cont">Cont 形式の文字列（null 可）</param>
        /// <returns>code → value の Dictionary</returns>
        public static Dictionary<string, string> Parse(string cont)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(cont))
            {
                return dict;
            }

            foreach (string line in cont.Split('\r', '\n'))
            {
                string[] s = line.Split(',');

                if (s.Length >= 2 && !dict.ContainsKey(s[0]))
                {
                    dict.Add(s[0], string.Join(",", s, 1, s.Length - 1).Replace(NewLineToken, "\r\n"));
                }
            }

            return dict;
        }

        /// <summary>
        /// Cont 文字列をパースし、dict に既にあるキーの値だけを上書きする（キーの追加はしない）。
        /// 「対象コードと初期値をあらかじめ用意しておき、Cont の値で埋める」表示処理向け。
        /// </summary>
        /// <param name="cont">Cont 形式の文字列（null 可）</param>
        /// <param name="dict">上書き対象の Dictionary</param>
        public static void ParseInto(string cont, Dictionary<string, string> dict)
        {
            foreach (KeyValuePair<string, string> kv in Parse(cont))
            {
                if (dict.ContainsKey(kv.Key))
                {
                    dict[kv.Key] = kv.Value;
                }
            }
        }

        /// <summary>
        /// コレクション内のコントロールから Cont 文字列を組み立てる。
        /// TextBox / ComboBox は Tag と Text がともに空でなければ "Tag,Text"（Text 内の改行はトークン化）、
        /// CheckBox はチェックされていれば "Tag,1" を出力する。その他のコントロールは無視する。
        /// </summary>
        /// <param name="controls">組み立て対象のコントロール群</param>
        /// <returns>Cont 形式の文字列（末尾に改行は付かない）</returns>
        public static string Build(Control.ControlCollection controls)
        {
            StringBuilder cont = new StringBuilder();

            AppendControls(cont, controls);

            return cont.ToString();
        }

        /// <summary>
        /// タブ内全ページのコントロールをまとめて 1 つの Cont 文字列に組み立てる。
        /// </summary>
        /// <param name="pages">組み立て対象のタブページ群</param>
        /// <returns>Cont 形式の文字列（末尾に改行は付かない）</returns>
        public static string Build(TabControl.TabPageCollection pages)
        {
            StringBuilder cont = new StringBuilder();

            foreach (TabPage page in pages)
            {
                AppendControls(cont, page.Controls);
            }

            return cont.ToString();
        }

        static void AppendControls(StringBuilder cont, Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                if (c.Tag == null || c.Tag.ToString().Length == 0)
                {
                    continue;
                }

                if ((c is TextBox || c is ComboBox) && c.Text.Length > 0)
                {
                    AppendLine(cont, c.Tag.ToString(), c.Text.Replace("\r\n", NewLineToken));
                }
                else if (c is CheckBox && ((CheckBox)c).Checked)
                {
                    AppendLine(cont, c.Tag.ToString(), "1");
                }
            }
        }

        static void AppendLine(StringBuilder cont, string code, string value)
        {
            if (cont.Length > 0)
            {
                cont.Append("\r\n");
            }

            cont.Append(code).Append(',').Append(value);
        }
    }
}
