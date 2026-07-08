using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    class InPrint3
    {
        public List<string> BaseList = new List<string>();
        public List<string> ValueList1 = new List<string>();
        public List<string> ValueList2 = new List<string>();

        public static Dictionary<string, InPrint3> GetDict(string adm_date)
        {
            Dictionary<string, InPrint3> tmpDict = new Dictionary<string, InPrint3>();

            List<PatIn> patInList = PatIn.GetListByDate(adm_date, "", InPrintCommon.DeptCode);

            foreach (PatIn tmpPat in patInList)
            {
                if (!tmpPat.Dept.Equals(InPrintCommon.DeptCode))
                {
                    continue;
                }

                // 入院予定の人は出さないように 2019/11/01 視能訓練士より
                if (tmpPat.Status == PatInStatus.Yet)
                {
                    continue;
                }

                InPrint3 tmpPrint = new InPrint3();
                tmpPrint.BaseList.Add(tmpPat.Room);
                tmpPrint.BaseList.Add(tmpPat.Id);
                tmpPrint.BaseList.Add(tmpPat.Name);
                tmpPrint.BaseList.Add(tmpPat.Kana);
                tmpPrint.BaseList.Add("");
                tmpDict.Add(tmpPat.Id, tmpPrint);
            }

            SetDict(tmpDict);

            return tmpDict;
        }

        /// <summary>
        /// 空の患者リストを受け取り、EYE_SUMMARY テーブルから取得したデータをセットする。
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        static void SetDict(Dictionary<string, InPrint3> dict)
        {
            List<string> pt_list = new List<string>();

            foreach (string k in dict.Keys)
            {
                if (k.Length > 0 && !pt_list.Contains(k))
                {
                    pt_list.Add(k);
                }
            }

            if (pt_list.Count == 0)
            {
                return;
            }

            List<EyeSummary> tmp_list = EyeSummary.GetListByPats(pt_list);

            foreach (EyeSummary tmp in tmp_list)
            {
                if (dict.ContainsKey(tmp.PtId))
                {
                    InPrint3 tmpPrint = dict[tmp.PtId];

                    tmpPrint.BaseList[4] = tmp.Plan;

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrint3"].Rows)
                    {
                        string value = InPrintCommon.GetSummaryValue(tmp, r);

                        if (r["Area"].ToString().Equals("1"))
                        {
                            tmpPrint.ValueList1.Add(value);
                        }
                        else if (r["Area"].ToString().Equals("2"))
                        {
                            tmpPrint.ValueList2.Add(value);
                        }
                    }

                    dict[tmp.PtId] = tmpPrint;
                }
            }

            return;
        }
    }
}
