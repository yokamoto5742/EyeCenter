using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    class InPrint2
    {
        public List<string> BaseList = new List<string>();

        public static List<InPrint2> GetList(string kensa_date)
        {
            List<InPrint2> tmpList = new List<InPrint2>();

            if (kensa_date.Length != 8)
            {
                return tmpList;
            }

            string cont = "";
            string kensa1 = "";
            string kensa2 = "";

            string k_code = "";
            string k_date = "";

            Dictionary<string, string> dict1 = new Dictionary<string, string>();
            Dictionary<string, string> dict2 = new Dictionary<string, string>();

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
            {
                if (r["Line"].ToString().Equals("1") && !dict1.ContainsKey(r["Code"].ToString()))
                {
                    dict1.Add(r["Code"].ToString(), r["Label"].ToString());
                }
                else if (r["Line"].ToString().Equals("2") && !dict2.ContainsKey(r["Code"].ToString()))
                {
                    dict2.Add(r["Code"].ToString(), r["Label"].ToString());
                }
            }

            List<EyeSummary> list = EyeSummary.GetListByKensaDate(kensa_date);

            foreach (EyeSummary tmp in list)
            {
                InPrint2 tmpPrint = new InPrint2();

                tmpPrint.BaseList.Add(tmp.PtId);
                tmpPrint.BaseList.Add(tmp.Pat.Name);
                tmpPrint.BaseList.Add(tmp.Plan);

                cont = tmp.Cont2;
                kensa1 = "";
                kensa2 = "";

                if (cont.Length > 0)
                {
                    foreach (string s in cont.Split('\r', '\n'))
                    {
                        if (s.Split(',').Length > 1)
                        {
                            k_code = s.Split(',')[0].Trim();
                            k_date = s.Split(',')[1].Trim().Split(' ')[0];

                            if (!k_date.Equals(kensa_date))
                            {
                                continue;
                            }

                            if (dict1.ContainsKey(k_code))
                            {
                                if (kensa1.Length > 0)
                                {
                                    kensa1 += ", ";
                                }

                                kensa1 += "[Å@] " + dict1[k_code];
                            }
                            else if (dict2.ContainsKey(k_code))
                            {
                                if (kensa2.Length > 0)
                                {
                                    kensa2 += ", ";
                                }

                                kensa2 += "[Å@] " + dict2[k_code];
                            }
                        }
                    }
                }

                tmpPrint.BaseList.Add(kensa1);
                tmpPrint.BaseList.Add(kensa2);

                tmpList.Add(tmpPrint);
            }

            return tmpList;
        }
    }
}
