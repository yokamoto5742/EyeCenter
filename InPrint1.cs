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

            List<PatIn> patInList = PatIn.GetListByDate(adm_date, "", "7");
            List<string> pt_list = new List<string>();

            foreach (PatIn tmpPat in patInList)
            {
                if (!tmpPat.Dept.Equals("7"))
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

            string cont = "";
            string cont1 = "";
            string cont3 = "";
            string cont4 = "";
            string value = "";

            List<EyeSummary> tmp_list = EyeSummary.GetListByPats(pt_list);

            foreach (EyeSummary tmp in tmp_list)
            {
                if (tmpDict.ContainsKey(tmp.PtId.ToString()))
                {
                    InPrint1 tmpPrint = tmpDict[tmp.PtId];

                    tmpPrint.BaseList[3] = tmp.Plan;

                    cont1 = tmp.Cont1;
                    cont3 = tmp.Cont3;
                    cont4 = tmp.Cont4;

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrint1"].Rows)
                    {
                        value = "";

                        if (r["Kind"].ToString().Equals("1"))
                        {
                            cont = cont1;
                        }
                        else if (r["Kind"].ToString().Equals("3"))
                        {
                            cont = cont3;
                        }
                        else if (r["Kind"].ToString().Equals("4"))
                        {
                            cont = cont4;
                        }

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

                        tmpPrint.ValueList.Add(value);
                    }

                    tmpDict[tmp.PtId] = tmpPrint;
                }
            }

            return tmpDict;
        }
    }
}
