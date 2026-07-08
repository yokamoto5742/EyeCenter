using System.Data;
using MedicalLibrary.Agent;

namespace EyeCenter
{
    /// <summary>
    /// 一括印刷（入院患者一覧・ワークシート）の共通処理。
    /// </summary>
    static class InPrintCommon
    {
        /// <summary>
        /// 眼科の診療科コード。電子カルテ側のコード体系に依存する。
        /// </summary>
        public const string DeptCode = "7";

        /// <summary>
        /// サマリーの内容（Kind に応じて Cont1/Cont3/Cont4）から、
        /// 印刷項目定義（Code）に該当する行の値を取り出す。
        /// </summary>
        /// <param name="sum">サマリー</param>
        /// <param name="r">印刷項目定義（SumPrint1/SumPrint3 の行）</param>
        /// <returns>該当する値。なければ空文字。</returns>
        public static string GetSummaryValue(EyeSummary sum, DataRow r)
        {
            string cont = "";

            if (r["Kind"].ToString().Equals("1"))
            {
                cont = sum.Cont1;
            }
            else if (r["Kind"].ToString().Equals("3"))
            {
                cont = sum.Cont3;
            }
            else if (r["Kind"].ToString().Equals("4"))
            {
                cont = sum.Cont4;
            }

            string value = "";

            if (cont.Length > 0)
            {
                foreach (string s in cont.Split('\r', '\n'))
                {
                    string[] ss = s.Split(',');

                    if (ss.Length > 1 && r["Code"].ToString().Equals(ss[0].Trim()))
                    {
                        for (int i = 1; i < ss.Length; i++)
                        {
                            if (value.Length > 0)
                            {
                                value += ",";
                            }

                            value += ss[i].Replace("<CR+LF>", "\r\n");
                        }

                        break;
                    }
                }
            }

            return value;
        }
    }
}
