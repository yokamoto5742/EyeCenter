using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormExport : Form
    {
        class ExpData
        {
            public List<string> DataList = new List<string>();
        }

        public FormExport()
        {
            InitializeComponent();
        }

        private void ExeButton_Click(object sender, EventArgs e)
        {
            if (KensaButton.Checked)
            {
                MessageBox.Show("この機能はまだ実装されていません");
            }
            else
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    this.Save(saveFileDialog1.FileName);
                }
            }
        }

        /// <summary>
        /// Excel にデータを入れて開く。
        /// </summary>
        void Save(string file_name)
        {
            try
            {
                // 先頭２行に書き込むデータを作る
                List<string> title1_list = new List<string>();
                List<string> title2_list = new List<string>();

                // ３行目以降のデータを作る
                List<ExpData> data_list = new List<ExpData>();

                if (OpeRecordButton.Checked)
                {
                    title1_list.Add("ID");
                    title2_list.Add("ID");

                    title1_list.Add("PATIENT_ID");
                    title2_list.Add("PATIENT_ID");

                    title1_list.Add("OPE_DATE");
                    title2_list.Add("OPE_DATE");

                    title1_list.Add("EYE");
                    title2_list.Add("EYE");

                    title1_list.Add("EYE_R");
                    title2_list.Add("EYE_R");

                    title1_list.Add("EYE_L");
                    title2_list.Add("EYE_L");

                    foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTabItem"].Rows)
                    {
                        if (r["Code"].ToString().Length > 0)
                        {
                            title1_list.Add(r["Code"].ToString());
                            title2_list.Add(r["Name"].ToString());
                        }
                    }

                    title1_list.Add("STAFF");
                    title1_list.Add("SAVE_DATE");
                    title1_list.Add("SAVE_TIME");
                    title1_list.Add("STATUS");
                    title1_list.Add("PDF_SAVE");
                    title1_list.Add("PDF_DATE");
                    title1_list.Add("PDF_TIME");

                    title2_list.Add("STAFF");
                    title2_list.Add("SAVE_DATE");
                    title2_list.Add("SAVE_TIME");
                    title2_list.Add("STATUS");
                    title2_list.Add("PDF_SAVE");
                    title2_list.Add("PDF_DATE");
                    title2_list.Add("PDF_TIME");

                    List<EyeOpeRecord> tmp_list = EyeOpeRecord.LoadAll();

                    Dictionary<string, string> dict = new Dictionary<string, string>();

                    foreach (EyeOpeRecord tmp in tmp_list)
                    {
                        ExpData d = new ExpData();

                        d.DataList.Add(tmp.Id);
                        d.DataList.Add(tmp.Ope.PtId);
                        d.DataList.Add(tmp.Ope.OpeDate);

                        if (tmp.Ope.EyeR.Equals("1") && tmp.Ope.EyeL.Equals("1"))
                        {
                            d.DataList.Add("B");
                        }
                        else if (tmp.Ope.EyeR.Equals("1"))
                        {
                            d.DataList.Add("R");
                        }
                        else if (tmp.Ope.EyeL.Equals("1"))
                        {
                            d.DataList.Add("L");
                        }
                        else
                        {
                            d.DataList.Add("");
                        }

                        d.DataList.Add(tmp.Ope.EyeR);
                        d.DataList.Add(tmp.Ope.EyeL);

                        dict.Clear();

                        foreach (string s in tmp.Cont.Split('\r', '\n'))
                        {
                            if (s.Split(',').Length > 1 && !dict.ContainsKey(s.Split(',')[0]))
                            {
                                dict.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                            }
                        }

                        foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTabItem"].Rows)
                        {
                            if (r["Code"].ToString().Length > 0)
                            {
                                if (dict.ContainsKey(r["Code"].ToString()))
                                {
                                    d.DataList.Add(dict[r["Code"].ToString()]);
                                }
                                else
                                {
                                    d.DataList.Add("");
                                }
                            }
                        }

                        d.DataList.Add(tmp.Staff);
                        d.DataList.Add(tmp.SaveDate);
                        d.DataList.Add(tmp.SaveTime);
                        d.DataList.Add(tmp.Status);
                        d.DataList.Add(tmp.PDFSave);
                        d.DataList.Add(tmp.PDFDate);
                        d.DataList.Add(tmp.PDFTime);

                        data_list.Add(d);
                    }
                }
                else if (SummaryButton.Checked)
                {
                    title1_list.Add("PATIENT_ID");
                    title2_list.Add("PATIENT_ID");

                    title1_list.Add("氏名");
                    title2_list.Add("氏名");

                    title1_list.Add("DIAG");
                    title2_list.Add("主病名");

                    title1_list.Add("KIND1");
                    title2_list.Add("分類1");

                    title1_list.Add("KIND2");
                    title2_list.Add("分類2");

                    title1_list.Add("KIND3");
                    title2_list.Add("分類3");

                    title1_list.Add("PLAN");
                    title2_list.Add("方針");

                    title1_list.Add("PASS");
                    title2_list.Add("経過");

                    title1_list.Add("HIST");
                    title2_list.Add("履歴");

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem1"].Rows)
                    {
                        if (r["Code"].ToString().Length > 0)
                        {
                            title1_list.Add(r["Code"].ToString());
                            title2_list.Add(r["Label"].ToString());
                        }
                    }

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
                    {
                        if (r["Code"].ToString().Length > 0)
                        {
                            title1_list.Add(r["Code"].ToString());
                            title1_list.Add(r["Code"].ToString());
                            title2_list.Add(r["Label"].ToString());
                            title2_list.Add(r["Label"].ToString());
                        }
                    }

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem3"].Rows)
                    {
                        if (r["Code"].ToString().Length > 0)
                        {
                            title1_list.Add(r["Code"].ToString());
                            title2_list.Add(r["Label"].ToString());
                        }
                    }

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem4"].Rows)
                    {
                        if (r["Code"].ToString().Length > 0)
                        {
                            title1_list.Add(r["Code"].ToString());
                            title2_list.Add(r["Name"].ToString());
                        }
                    }

                    title1_list.Add("STAFF");
                    title1_list.Add("SAVE_DATE");
                    title1_list.Add("SAVE_TIME");

                    title2_list.Add("STAFF");
                    title2_list.Add("SAVE_DATE");
                    title2_list.Add("SAVE_TIME");

                    Dictionary<string, string> dict1 = new Dictionary<string, string>();
                    Dictionary<string, string> dict2 = new Dictionary<string, string>();
                    Dictionary<string, string> dict3 = new Dictionary<string, string>();
                    Dictionary<string, string> dict4 = new Dictionary<string, string>();

                    List<EyeSummary> sum_list = EyeSummary.LoadAll();

                    foreach (EyeSummary tmp in sum_list)
                    {
                        ExpData d = new ExpData();

                        d.DataList.Add(tmp.PtId);
                        d.DataList.Add(tmp.Pat.Name);
                        d.DataList.Add(tmp.Diag);
                        d.DataList.Add(tmp.Kind1);
                        d.DataList.Add(tmp.Kind2);
                        d.DataList.Add(tmp.Kind3);
                        d.DataList.Add(tmp.Plan);
                        d.DataList.Add(tmp.Pass);
                        d.DataList.Add(tmp.Hist);

                        dict1.Clear();
                        dict2.Clear();
                        dict3.Clear();
                        dict4.Clear();

                        foreach (string s in tmp.Cont1.Split('\r', '\n'))
                        {
                            if (s.Split(',').Length > 1 && !dict1.ContainsKey(s.Split(',')[0]))
                            {
                                dict1.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                            }
                        }

                        foreach (string s in tmp.Cont2.Split('\r', '\n'))
                        {
                            if (s.Split(',').Length > 1 && !dict2.ContainsKey(s.Split(',')[0]))
                            {
                                dict2.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                            }
                        }

                        foreach (string s in tmp.Cont3.Split('\r', '\n'))
                        {
                            if (s.Split(',').Length > 1 && !dict3.ContainsKey(s.Split(',')[0]))
                            {
                                dict3.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                            }
                        }

                        foreach (string s in tmp.Cont4.Split('\r', '\n'))
                        {
                            if (s.Split(',').Length > 1 && !dict4.ContainsKey(s.Split(',')[0]))
                            {
                                dict4.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                            }
                        }

                        foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem1"].Rows)
                        {
                            if (r["Code"].ToString().Length > 0)
                            {
                                if (dict1.ContainsKey(r["Code"].ToString()))
                                {
                                    d.DataList.Add(dict1[r["Code"].ToString()]);
                                }
                                else
                                {
                                    d.DataList.Add("");
                                }
                            }
                        }

                        foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
                        {
                            if (r["Code"].ToString().Length > 0)
                            {
                                if (dict2.ContainsKey(r["Code"].ToString()))
                                {
                                    if (dict2[r["Code"].ToString()].Contains(" "))
                                    {
                                        d.DataList.Add(dict2[r["Code"].ToString()].Split(' ')[0]);
                                        d.DataList.Add(dict2[r["Code"].ToString()].Split(' ')[1]);
                                    }
                                    else
                                    {
                                        d.DataList.Add("");
                                        d.DataList.Add("");
                                    }
                                }
                                else
                                {
                                    d.DataList.Add("");
                                    d.DataList.Add("");
                                }
                            }
                        }

                        foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem3"].Rows)
                        {
                            if (r["Code"].ToString().Length > 0)
                            {
                                if (dict3.ContainsKey(r["Code"].ToString()))
                                {
                                    d.DataList.Add(dict3[r["Code"].ToString()]);
                                }
                                else
                                {
                                    d.DataList.Add("");
                                }
                            }
                        }

                        foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem4"].Rows)
                        {
                            if (r["Code"].ToString().Length > 0)
                            {
                                if (dict4.ContainsKey(r["Code"].ToString()))
                                {
                                    d.DataList.Add(dict4[r["Code"].ToString()]);
                                }
                                else
                                {
                                    d.DataList.Add("");
                                }
                            }
                        }

                        d.DataList.Add(tmp.Staff);
                        d.DataList.Add(tmp.SaveDate);
                        d.DataList.Add(tmp.SaveTime);

                        data_list.Add(d);
                    }
                }

                // ファイルに書き込む
                System.IO.StreamWriter writer = new System.IO.StreamWriter(file_name, false, Encoding.GetEncoding("shift-jis"));

                for (int i = 0; i < title1_list.Count; i++)
                {
                    writer.Write("\"" + title1_list[i].Replace("\"", "\"\"") + "\",");
                }

                writer.WriteLine();

                for (int i = 0; i < title2_list.Count; i++)
                {
                    writer.Write("\"" + title2_list[i].Replace("\"", "\"\"") + "\",");
                }

                writer.WriteLine();

                for (int p = 0; p < data_list.Count; p++)
                {
                    for (int i = 0; i < data_list[p].DataList.Count; i++)
                    {
                        writer.Write("\"" + data_list[p].DataList[i].Replace("\"", "\"\"") + "\",");
                    }

                    writer.WriteLine();
                }

                writer.Close();

                MessageBox.Show("エクスポートが完了しました");
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBox.Show(err);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}