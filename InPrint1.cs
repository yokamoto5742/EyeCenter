using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    class InPrint1
    {
        public List<string> BaseList = new List<string>();
        public List<string> ValueList = new List<string>();

        public static Dictionary<string, InPrint1> GetDict(string adm_date)
        {
            Dictionary<string, InPrint1> tmpDict = new Dictionary<string, InPrint1>();

            List<PatIn> patInList = PatIn.GetListByDate(adm_date, "", InPrintCommon.DeptCode);
            List<string> pt_list = new List<string>();

            foreach (PatIn tmpPat in patInList)
            {
                if (!tmpPat.Dept.Equals(InPrintCommon.DeptCode))
                {
                    continue;
                }

                if (tmpDict.ContainsKey(tmpPat.Id))
                {
                    continue;
                }

                InPrint1 tmpPrint = new InPrint1();
                tmpPrint.BaseList.Add(tmpPat.Room);
                tmpPrint.BaseList.Add(tmpPat.Id);
                tmpPrint.BaseList.Add(tmpPat.Name);
                tmpPrint.BaseList.Add("");

                tmpDict.Add(tmpPat.Id, tmpPrint);

                if (tmpPat.Id.Length > 0 && !pt_list.Contains(tmpPat.Id))
                {
                    pt_list.Add(tmpPat.Id);
                }
            }

            if (pt_list.Count == 0)
            {
                return tmpDict;
            }

            List<EyeSummary> tmp_list = EyeSummary.GetListByPats(pt_list);

            foreach (EyeSummary tmp in tmp_list)
            {
                if (tmpDict.ContainsKey(tmp.PtId.ToString()))
                {
                    InPrint1 tmpPrint = tmpDict[tmp.PtId];

                    tmpPrint.BaseList[3] = tmp.Plan;

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrint1"].Rows)
                    {
                        tmpPrint.ValueList.Add(InPrintCommon.GetSummaryValue(tmp, r));
                    }

                    tmpDict[tmp.PtId] = tmpPrint;
                }
            }

            return tmpDict;
        }
    }
}
