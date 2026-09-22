using System;
using System.Collections.Generic;
using System.Globalization;

namespace EyeCenter
{
    /// <summary>
    /// NIDEK ARK（レフ・ケラト）の CONT 文字列から代表値を取り出す。
    /// レフは "Avg" の次の行（S C A）、ケラトは R1/R2（D と軸）・Avg（D）・CYL（D と軸）を使う。
    /// K1 は弱主経線（D が小さい方）、K2 は強主経線。同値の場合は R1 を K1 とする。
    /// </summary>
    static class NidekArkParser
    {
        /// <summary>眼ごとの出力項目名（先頭に R_ / L_ を付けて列名にする）。</summary>
        public static readonly string[] EyeTitles = { "S", "C", "A", "K1", "K1軸", "K2", "K2軸", "Kavg", "CYL", "CYL軸" };

        public class Eye
        {
            public string S = "", C = "", A = "";
            public string K1 = "", K1Axis = "", K2 = "", K2Axis = "", Kavg = "";
            public string Cyl = "", CylAxis = "";

            /// <summary>EyeTitles の順に値を返す。</summary>
            public string[] ToArray()
            {
                return new[] { S, C, A, K1, K1Axis, K2, K2Axis, Kavg, Cyl, CylAxis };
            }
        }

        public class Result
        {
            public string Model = "";
            public string MeasuredAt = "";
            public Eye R = new Eye();
            public Eye L = new Eye();
        }

        /// <summary>
        /// CONT を解析する。NIDEK ARK 形式でなければ null を返す。
        /// </summary>
        public static Result Parse(string cont)
        {
            if (string.IsNullOrEmpty(cont))
            {
                return null;
            }

            string[] lines = cont.Replace(ContData.NewLineToken, "\n").Replace("\r\n", "\n").Split('\n', '\r');

            if (!lines[0].Trim().StartsWith("NIDEK ARK"))
            {
                return null;
            }

            Result result = new Result();
            result.Model = lines[0].Trim();

            if (lines.Length > 1)
            {
                result.MeasuredAt = lines[1].Trim();
            }

            Eye eye = null;
            string[] r1 = null, r2 = null;

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string[] t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (line == "<R>" || line == "<L>")
                {
                    SetKerato(eye, r1, r2);
                    eye = line == "<R>" ? result.R : result.L;
                    r1 = r2 = null;
                }
                else if (eye == null || t.Length == 0)
                {
                    continue;
                }
                else if (line == "Avg")
                {
                    // レフの代表値は次の空でない行
                    for (i++; i < lines.Length; i++)
                    {
                        string[] v = lines[i].Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                        if (v.Length > 0)
                        {
                            if (v.Length >= 3)
                            {
                                eye.S = v[0];
                                eye.C = v[1];
                                eye.A = v[2];
                            }

                            break;
                        }
                    }
                }
                else if (t[0] == "R1" && t.Length >= 4)
                {
                    r1 = t;
                }
                else if (t[0] == "R2" && t.Length >= 4)
                {
                    r2 = t;
                }
                else if (t[0] == "Avg" && t.Length >= 3)
                {
                    eye.Kavg = t[2];
                }
                else if (t[0] == "CYL" && t.Length >= 3)
                {
                    eye.Cyl = t[1];
                    eye.CylAxis = t[2];
                }
            }

            SetKerato(eye, r1, r2);

            return result;
        }

        /// <summary>R1/R2 を弱主経線（K1）・強主経線（K2）の順に並べて設定する。</summary>
        static void SetKerato(Eye eye, string[] r1, string[] r2)
        {
            if (eye == null || r1 == null || r2 == null)
            {
                return;
            }

            double d1, d2;

            if (double.TryParse(r1[2], NumberStyles.Float, CultureInfo.InvariantCulture, out d1) &&
                double.TryParse(r2[2], NumberStyles.Float, CultureInfo.InvariantCulture, out d2) &&
                d2 < d1)
            {
                string[] tmp = r1;
                r1 = r2;
                r2 = tmp;
            }

            eye.K1 = r1[2];
            eye.K1Axis = r1[3];
            eye.K2 = r2[2];
            eye.K2Axis = r2[3];
        }
    }
}
