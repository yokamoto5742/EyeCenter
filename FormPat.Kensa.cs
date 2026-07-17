using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// 検査歴（KensaHistoryView）の表示。
    /// </summary>
    public partial class FormPat
    {
        /// <summary>
        /// 検査歴を表示する
        /// </summary>
        public void PtKensaHistoryShow()
        {
            DataTable tmpTable = dSet.Tables["検査歴"];
            tmpTable.Rows.Clear();

            List<EyeKensa> tmpList = EyeKensa.LoadByPatient(this.Pat.Id, "");

            string tmpDate = "";
            DataRow tmpRow = tmpTable.NewRow();

            foreach (EyeKensa tmpKensa in tmpList)
            {
                // XMLマスターにない検査ならば飛ばす
                if (!tmpTable.Columns.Contains(tmpKensa.KensaId))
                {
                    continue;
                }

                if (!tmpDate.Equals(tmpKensa.KensaDate))
                {
                    if (tmpRow["検査日"].ToString().Length > 0)
                    {
                        tmpTable.Rows.Add(tmpRow);
                    }

                    tmpDate = tmpKensa.KensaDate;
                    tmpRow = tmpTable.NewRow();
                    tmpRow["KENSA_DATE"] = tmpKensa.KensaDate;
                    tmpRow["検査日"] = tmpKensa.KensaDate.PadRight(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                }

                tmpRow[tmpKensa.KensaId] = "○";

                // KensaId に応じて表示用の検査データを作成する
                if (tmpKensa.KensaId.Equals("1"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("101R", "");
                    tmpDict.Add("102R", "");
                    tmpDict.Add("103R", "");
                    tmpDict.Add("104R", "");
                    tmpDict.Add("105R", "");
                    tmpDict.Add("106R", "");
                    tmpDict.Add("101L", "");
                    tmpDict.Add("102L", "");
                    tmpDict.Add("103L", "");
                    tmpDict.Add("104L", "");
                    tmpDict.Add("105L", "");
                    tmpDict.Add("106L", "");
                    tmpDict.Add("107R", "");
                    tmpDict.Add("107L", "");
                    tmpDict.Add("108R", "");
                    tmpDict.Add("108L", "");
                    tmpDict.Add("109R", "");
                    tmpDict.Add("110R", "");
                    tmpDict.Add("109L", "");
                    tmpDict.Add("110L", "");
                    tmpDict.Add("109B", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // 遠見視力（右）データ作成
                    if (tmpDict["108R"].Equals("1"))
                    {
                        tmpRow["Sight_R"] = "RV=" + tmpDict["109R"] + "(" + tmpDict["110R"] + ")";
                    }
                    else if (tmpDict["107R"].Equals("1"))
                    {
                        tmpRow["Sight_R"] = "RV=" + tmpDict["101R"] + "(" + tmpDict["102R"] + "×" + tmpDict["103R"] + "D)";
                    }
                    else
                    {
                        tmpRow["Sight_R"] = "RV=" + tmpDict["101R"] + "(" + tmpDict["102R"] + "×" + tmpDict["103R"] + "D=cyl" + tmpDict["104R"] + "D Ax" + tmpDict["105R"] + "°)";
                    }

                    // 遠見視力（左）データ作成
                    if (tmpDict["108L"].Equals("1"))
                    {
                        tmpRow["Sight_L"] = "LV=" + tmpDict["109L"] + "(" + tmpDict["110L"] + ")";
                    }
                    else if (tmpDict["107L"].Equals("1"))
                    {
                        tmpRow["Sight_L"] = "LV=" + tmpDict["101L"] + "(" + tmpDict["102L"] + "×" + tmpDict["103L"] + "D)";
                    }
                    else
                    {
                        tmpRow["Sight_L"] = "LV=" + tmpDict["101L"] + "(" + tmpDict["102L"] + "×" + tmpDict["103L"] + "D=cyl" + tmpDict["104L"] + "D Ax" + tmpDict["105L"] + "°)";
                    }

                    // 視力（右）データ作成
                    if (tmpDict["102R"].Length == 0 || tmpDict["102R"].Contains("n.c"))
                    {
                        tmpRow["CorrectSight_R"] = tmpDict["101R"];
                    }
                    else
                    {
                        tmpRow["CorrectSight_R"] = tmpDict["102R"];
                    }

                    // 視力（左）データ作成
                    if (tmpDict["102L"].Length == 0 || tmpDict["102L"].Contains("n.c"))
                    {
                        tmpRow["CorrectSight_L"] = tmpDict["101L"];
                    }
                    else
                    {
                        tmpRow["CorrectSight_L"] = tmpDict["102L"];
                    }

                    // 球面度数データ作成
                    tmpRow["KyumenDosu_R"] = tmpDict["103R"];
                    tmpRow["KyumenDosu_L"] = tmpDict["103L"];

                    // 円柱度数データ作成
                    tmpRow["EnchuDosu_R"] = tmpDict["104R"];
                    tmpRow["EnchuDosu_L"] = tmpDict["104L"];
                }
                else if (tmpKensa.KensaId.Equals("3"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("301R", "");
                    tmpDict.Add("302R", "");
                    tmpDict.Add("303R", "");
                    tmpDict.Add("304R", "");
                    tmpDict.Add("301L", "");
                    tmpDict.Add("302L", "");
                    tmpDict.Add("303L", "");
                    tmpDict.Add("304L", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // 眼圧（右）データ作成
                    tmpRow["TensionAvg_R"] = tmpDict["304R"];
                    tmpRow["Tension_R"] = "RT=" + tmpDict["301R"] + "," + tmpDict["302R"] + "," + tmpDict["303R"] + " (AVG)" + tmpDict["304R"] + "mmHg";

                    // 眼圧（左）データ作成
                    tmpRow["TensionAvg_L"] = tmpDict["304L"];
                    tmpRow["Tension_L"] = "LT=" + tmpDict["301L"] + "," + tmpDict["302L"] + "," + tmpDict["303L"] + " (AVG)" + tmpDict["304L"] + "mmHg";
                }
                else if (tmpKensa.KensaId.Equals("5"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("501R", "");
                    tmpDict.Add("501L", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // GATデータ作成
                    tmpRow["GAT_R"] = tmpDict["501R"];
                    tmpRow["GAT_L"] = tmpDict["501L"];
                }
                else if (tmpKensa.KensaId.Equals("7"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("701R", "");
                    tmpDict.Add("701L", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // 綿糸法データ作成
                    tmpRow["Menshihou_R"] = tmpDict["701R"];
                    tmpRow["Menshihou_L"] = tmpDict["701L"];
                }
                else if (tmpKensa.KensaId.Equals("8"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("801R", "");
                    tmpDict.Add("802R", "");
                    tmpDict.Add("801L", "");
                    tmpDict.Add("802L", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // Mチャートデータ作成
                    tmpRow["MChartTate_R"] = tmpDict["801R"];
                    tmpRow["MChartYoko_R"] = tmpDict["802R"];
                    tmpRow["MChartTate_L"] = tmpDict["801L"];
                    tmpRow["MChartYoko_L"] = tmpDict["802L"];
                }
                else if (tmpKensa.KensaId.Equals("9"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("901R", "");
                    tmpDict.Add("902R", "");
                    tmpDict.Add("903R", "");
                    tmpDict.Add("904R", "");
                    tmpDict.Add("905R", "");
                    tmpDict.Add("901L", "");
                    tmpDict.Add("902L", "");
                    tmpDict.Add("903L", "");
                    tmpDict.Add("904L", "");
                    tmpDict.Add("905L", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // コントラスト（右）データ作成
                    tmpRow["ContrastA_R"] = tmpDict["901R"];
                    tmpRow["ContrastB_R"] = tmpDict["902R"];
                    tmpRow["ContrastC_R"] = tmpDict["903R"];
                    tmpRow["ContrastD_R"] = tmpDict["904R"];
                    tmpRow["ContrastE_R"] = tmpDict["905R"];

                    // コントラスト（左）データ作成
                    tmpRow["ContrastA_L"] = tmpDict["901L"];
                    tmpRow["ContrastB_L"] = tmpDict["902L"];
                    tmpRow["ContrastC_L"] = tmpDict["903L"];
                    tmpRow["ContrastD_L"] = tmpDict["904L"];
                    tmpRow["ContrastE_L"] = tmpDict["905L"];
                }
                else if (tmpKensa.KensaId.Equals("31"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("1110R", "");
                    tmpDict.Add("1110L", "");

                    ContData.ParseInto(tmpKensa.Cont, tmpDict);

                    // 網膜厚データ作成
                    tmpRow["Moumakukou_R"] = tmpDict["1110R"];
                    tmpRow["Moumakukou_L"] = tmpDict["1110L"];
                }
            }

            if (tmpRow["検査日"].ToString().Length > 0)
            {
                tmpTable.Rows.Add(tmpRow);
            }

            // EyeKensa2 のデータがあればテーブルに追加する。
            List<EyeKensa2> tmpList2 = EyeKensa2.LoadByPatient(this.Pat.Id, "");

            foreach (EyeKensa2 tmpKensa in tmpList2)
            {
                // tmpTable の行をチェックして、同一検査日のものがあればそこで処理する。
                bool no_flg = true;

                foreach (DataRow r in tmpTable.Rows)
                {
                    if (r["KENSA_DATE"].ToString().Equals(tmpKensa.KensaDate))
                    {
                        r[tmpKensa.KensaId] = "○";
                        no_flg = false;
                        break;
                    }
                }

                // tmpTable の行に、同一検査日のものがなければ行を追加する。
                if (no_flg)
                {
                    DataRow r = tmpTable.NewRow();
                    r["KENSA_DATE"] = tmpKensa.KensaDate;
                    r["検査日"] = tmpKensa.KensaDate.PadRight(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                    r[tmpKensa.KensaId] = "○";
                    tmpTable.Rows.Add(r);
                }
            }

            DataView tmpView = new DataView(tmpTable);
            tmpView.Sort = "KENSA_DATE desc";

            KensaHistoryView.DataSource = tmpView;

            this.PtKensaHistoryWide();
        }

        /// <summary>
        /// KensaHistoryView の大きさに応じてカラムの表示・非表示と幅を調整する。
        /// </summary>
        void PtKensaHistoryWide()
        {
            KensaHistoryView.Columns["KENSA_DATE"].Visible = false;

            KensaHistoryView.Columns["検査日"].Width = 55;
            KensaHistoryView.Columns["検査日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["KensaPage"].Rows)
            {
                KensaHistoryView.Columns[tmpRow["ID"].ToString()].Width = 25;
                KensaHistoryView.Columns[tmpRow["ID"].ToString()].HeaderText = tmpRow["Header"].ToString();
                KensaHistoryView.Columns[tmpRow["ID"].ToString()].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            int width = 0;

            foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["KensaHistory"].Rows)
            {
                if (tmpRow["Visible"].ToString().Equals("1"))
                {
                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].Visible = true;
                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].HeaderText = tmpRow["Text"].ToString();

                    if (tmpRow["Width"].ToString().Length > 0 && int.TryParse(tmpRow["Width"].ToString(), out width))
                    {
                        KensaHistoryView.Columns[tmpRow["Name"].ToString()].Width = width;
                    }

                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else
                {
                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].Visible = false;
                }
            }
        }
    }
}
