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
        public FormExport()
        {
            InitializeComponent();
        }

        private void ExeButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (KensaButton.Checked)
                {
                    this.SaveKensa(saveFileDialog1.FileName);
                }
                else if (OpeRecordButton.Checked)
                {
                    this.SaveOpe(saveFileDialog1.FileName);
                }
                else
                {
                    this.SaveSummary(saveFileDialog1.FileName);
                }
            }
        }

        /// <summary>
        /// 検査データ（EYE_KENSA）をCSVに書き出す。
        /// 件数が多いため主キー順のページング処理でロードし、ストリームに逐次書き込む。
        /// </summary>
        void SaveKensa(string file_name)
        {
            const int PAGE_SIZE = 5000;

            string title = this.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                ExeButton.Enabled = false;
                CloseButton.Enabled = false;

                int total = 0;

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file_name, false, Encoding.GetEncoding("shift-jis")))
                {
                    List<string> title_list = new List<string>() { "PATIENT_ID", "KENSA_ID", "KENSA_NAME", "KENSA_DATE", "CONT", "STAFF", "SAVE_DATE", "SAVE_TIME", "PDF_SAVE" };

                    // ヘッダーは１行のみ出力する（汎用インポートツールは先頭１行をヘッダーとみなすため）
                    List<string> header_list = new List<string>();
                    for (int i = 0; i < title_list.Count; i++)
                    {
                        header_list.Add("\"" + title_list[i].Replace("\"", "\"\"") + "\"");
                    }
                    writer.WriteLine(string.Join(",", header_list));

                    string last_pt = "";
                    string last_kensa = "";
                    string last_date = "";

                    while (true)
                    {
                        // 前ページの最終キーより後ろを主キー順に PAGE_SIZE 件だけ取得する
                        string cmd = "select * from (select * from EYE_KENSA ";

                        if (last_pt.Length > 0)
                        {
                            cmd += " where PATIENT_ID > " + last_pt +
                                " or (PATIENT_ID = " + last_pt + " and KENSA_ID > " + last_kensa + ")" +
                                " or (PATIENT_ID = " + last_pt + " and KENSA_ID = " + last_kensa + " and KENSA_DATE > " + last_date + ")";
                        }

                        cmd += " order by PATIENT_ID, KENSA_ID, KENSA_DATE) where ROWNUM <= " + PAGE_SIZE;

                        List<StdClass> page_list = StdClass.GetList(DB.Db2, cmd);

                        foreach (StdClass tmp in page_list)
                        {
                            last_pt = tmp.GetDataString("PATIENT_ID");
                            last_kensa = tmp.GetDataString("KENSA_ID");
                            last_date = tmp.GetDataString("KENSA_DATE");

                            string kensa_name = "";

                            if (EyeKensaMaster.Dict.ContainsKey(last_kensa))
                            {
                                kensa_name = EyeKensaMaster.Dict[last_kensa].Name;
                            }

                            List<string> cells = new List<string>();
                            cells.Add("\"" + last_pt + "\"");
                            cells.Add("\"" + last_kensa + "\"");
                            cells.Add("\"" + kensa_name.Replace("\"", "\"\"") + "\"");
                            cells.Add("\"" + last_date + "\"");
                            // CONT内の改行はトークン(<CR+LF>)に置換し、1レコード=1物理行にする（行ベースの取込ツールでも確実に読めるようにするため。Ope/Summary側と同じトークン方式）
                            cells.Add("\"" + tmp.GetDataString("CONT").Replace("\"", "\"\"").Replace("\r\n", "<CR+LF>").Replace("\r", "<CR+LF>").Replace("\n", "<CR+LF>") + "\"");
                            cells.Add("\"" + tmp.GetDataString("STAFF") + "\"");
                            cells.Add("\"" + tmp.GetDataString("SAVE_DATE") + "\"");
                            cells.Add("\"" + tmp.GetDataString("SAVE_TIME") + "\"");
                            cells.Add("\"" + tmp.GetDataString("PDF_SAVE") + "\"");
                            writer.WriteLine(string.Join(",", cells));
                        }

                        total += page_list.Count;

                        this.Text = title + " " + total.ToString("#,0") + "件";
                        Application.DoEvents();

                        if (this.IsDisposed)
                        {
                            return;
                        }

                        if (page_list.Count < PAGE_SIZE)
                        {
                            break;
                        }
                    }
                }

                MessageBox.Show("エクスポートが完了しました（" + total.ToString("#,0") + "件）");
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBox.Show(err);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.Text = title;
                    ExeButton.Enabled = true;
                    CloseButton.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// １行分のセルをＣＳＶ形式で書き込む（従来のエクスポートと同じ、各セル引用符囲み・末尾カンマ付き）。
        /// </summary>
        static void WriteCsvLine(System.IO.StreamWriter writer, List<string> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                writer.Write("\"" + cells[i].Replace("\"", "\"\"") + "\",");
            }

            writer.WriteLine();
        }

        /// <summary>
        /// 手術記録（EYE_OPE_RECORD＋EYE_OPE）をCSVに書き出す。
        /// 件数が多いため主キー順のページング処理でロードし、ストリームに逐次書き込む。
        /// </summary>
        void SaveOpe(string file_name)
        {
            const int PAGE_SIZE = 5000;

            string title = this.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                ExeButton.Enabled = false;
                CloseButton.Enabled = false;

                // 先頭２行に書き込むデータを作る
                List<string> title1_list = new List<string>();
                List<string> title2_list = new List<string>();

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

                int total = 0;

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file_name, false, Encoding.GetEncoding("shift-jis")))
                {
                    WriteCsvLine(writer, title1_list);
                    WriteCsvLine(writer, title2_list);

                    Dictionary<string, string> dict = new Dictionary<string, string>();

                    string last_id = "";

                    while (true)
                    {
                        // 前ページの最終IDより後ろをID順に PAGE_SIZE 件だけ取得する
                        string cmd = "select * from (select EYE_OPE_RECORD.ID, EYE_OPE_RECORD.CONT, EYE_OPE_RECORD.STAFF," +
                            " EYE_OPE_RECORD.SAVE_DATE, EYE_OPE_RECORD.SAVE_TIME, EYE_OPE_RECORD.STATUS," +
                            " EYE_OPE.PATIENT_ID, EYE_OPE.OPE_DATE, EYE_OPE.EYE_R, EYE_OPE.EYE_L" +
                            " from EYE_OPE_RECORD" +
                            " inner join EYE_OPE on EYE_OPE_RECORD.ID = EYE_OPE.ID";

                        if (last_id.Length > 0)
                        {
                            cmd += " where EYE_OPE_RECORD.ID > " + last_id;
                        }

                        cmd += " order by EYE_OPE_RECORD.ID) where ROWNUM <= " + PAGE_SIZE;

                        List<StdClass> page_list = StdClass.GetList(DB.Db2, cmd);

                        foreach (StdClass tmp in page_list)
                        {
                            last_id = tmp.GetDataString("ID");

                            List<string> cells = new List<string>();

                            cells.Add(last_id);
                            cells.Add(tmp.GetDataString("PATIENT_ID"));
                            cells.Add(tmp.GetDataString("OPE_DATE"));

                            string eye_r = tmp.GetDataString("EYE_R");
                            string eye_l = tmp.GetDataString("EYE_L");

                            if (eye_r.Equals("1") && eye_l.Equals("1"))
                            {
                                cells.Add("B");
                            }
                            else if (eye_r.Equals("1"))
                            {
                                cells.Add("R");
                            }
                            else if (eye_l.Equals("1"))
                            {
                                cells.Add("L");
                            }
                            else
                            {
                                cells.Add("");
                            }

                            cells.Add(eye_r);
                            cells.Add(eye_l);

                            dict.Clear();

                            foreach (string s in tmp.GetDataString("CONT").Split('\r', '\n'))
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
                                        cells.Add(dict[r["Code"].ToString()]);
                                    }
                                    else
                                    {
                                        cells.Add("");
                                    }
                                }
                            }

                            cells.Add(tmp.GetDataString("STAFF"));
                            cells.Add(tmp.GetDataString("SAVE_DATE"));
                            cells.Add(tmp.GetDataString("SAVE_TIME"));
                            cells.Add(tmp.GetDataString("STATUS"));
                            cells.Add(tmp.GetDataString("PDF_SAVE"));
                            cells.Add(tmp.GetDataString("PDF_DATE"));
                            cells.Add(tmp.GetDataString("PDF_TIME"));

                            WriteCsvLine(writer, cells);
                        }

                        total += page_list.Count;

                        this.Text = title + " " + total.ToString("#,0") + "件";
                        Application.DoEvents();

                        if (this.IsDisposed)
                        {
                            return;
                        }

                        if (page_list.Count < PAGE_SIZE)
                        {
                            break;
                        }
                    }
                }

                MessageBox.Show("エクスポートが完了しました（" + total.ToString("#,0") + "件）");
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBox.Show(err);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.Text = title;
                    ExeButton.Enabled = true;
                    CloseButton.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        /// <summary>
        /// サマリー（EYE_SUMMARY）をCSVに書き出す。
        /// 件数が多いため主キー順のページング処理でロードし、ストリームに逐次書き込む。
        /// 患者氏名はページごとに PatBase.GetList でまとめて取得する。
        /// </summary>
        void SaveSummary(string file_name)
        {
            const int PAGE_SIZE = 5000;

            string title = this.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                ExeButton.Enabled = false;
                CloseButton.Enabled = false;

                // 先頭２行に書き込むデータを作る
                List<string> title1_list = new List<string>();
                List<string> title2_list = new List<string>();

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

                int total = 0;

                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(file_name, false, Encoding.GetEncoding("shift-jis")))
                {
                    WriteCsvLine(writer, title1_list);
                    WriteCsvLine(writer, title2_list);

                    Dictionary<string, string> dict1 = new Dictionary<string, string>();
                    Dictionary<string, string> dict2 = new Dictionary<string, string>();
                    Dictionary<string, string> dict3 = new Dictionary<string, string>();
                    Dictionary<string, string> dict4 = new Dictionary<string, string>();

                    string last_pt = "";

                    while (true)
                    {
                        // 前ページの最終患者IDより後ろを患者ID順に PAGE_SIZE 件だけ取得する
                        string cmd = "select * from (select * from EYE_SUMMARY ";

                        if (last_pt.Length > 0)
                        {
                            cmd += " where PATIENT_ID > " + last_pt;
                        }

                        cmd += " order by PATIENT_ID) where ROWNUM <= " + PAGE_SIZE;

                        List<StdClass> page_list = StdClass.GetList(DB.Db2, cmd);

                        // このページの患者氏名をまとめて取得する
                        List<string> pt_list = new List<string>();

                        foreach (StdClass tmp in page_list)
                        {
                            pt_list.Add(tmp.GetDataString("PATIENT_ID"));
                        }

                        Dictionary<string, string> name_dict = new Dictionary<string, string>();

                        foreach (PatBase pat in PatBase.GetList(pt_list))
                        {
                            if (!name_dict.ContainsKey(pat.Id))
                            {
                                name_dict.Add(pat.Id, pat.Name);
                            }
                        }

                        foreach (StdClass tmp in page_list)
                        {
                            last_pt = tmp.GetDataString("PATIENT_ID");

                            List<string> cells = new List<string>();

                            cells.Add(last_pt);

                            if (name_dict.ContainsKey(last_pt))
                            {
                                cells.Add(name_dict[last_pt]);
                            }
                            else
                            {
                                cells.Add("");
                            }

                            cells.Add(tmp.GetDataString("DIAG"));
                            cells.Add(tmp.GetDataString("KIND1"));
                            cells.Add(tmp.GetDataString("KIND2"));
                            cells.Add(tmp.GetDataString("KIND3"));
                            cells.Add(tmp.GetDataString("PLAN"));
                            cells.Add(tmp.GetDataString("PASS"));
                            cells.Add(tmp.GetDataString("HIST"));

                            dict1.Clear();
                            dict2.Clear();
                            dict3.Clear();
                            dict4.Clear();

                            foreach (string s in tmp.GetDataString("CONT1").Split('\r', '\n'))
                            {
                                if (s.Split(',').Length > 1 && !dict1.ContainsKey(s.Split(',')[0]))
                                {
                                    dict1.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                                }
                            }

                            foreach (string s in tmp.GetDataString("CONT2").Split('\r', '\n'))
                            {
                                if (s.Split(',').Length > 1 && !dict2.ContainsKey(s.Split(',')[0]))
                                {
                                    dict2.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                                }
                            }

                            foreach (string s in tmp.GetDataString("CONT3").Split('\r', '\n'))
                            {
                                if (s.Split(',').Length > 1 && !dict3.ContainsKey(s.Split(',')[0]))
                                {
                                    dict3.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                                }
                            }

                            foreach (string s in tmp.GetDataString("CONT4").Split('\r', '\n'))
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
                                        cells.Add(dict1[r["Code"].ToString()]);
                                    }
                                    else
                                    {
                                        cells.Add("");
                                    }
                                }
                            }

                            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
                            {
                                if (r["Code"].ToString().Length > 0)
                                {
                                    if (dict2.ContainsKey(r["Code"].ToString()) && dict2[r["Code"].ToString()].Contains(" "))
                                    {
                                        cells.Add(dict2[r["Code"].ToString()].Split(' ')[0]);
                                        cells.Add(dict2[r["Code"].ToString()].Split(' ')[1]);
                                    }
                                    else
                                    {
                                        cells.Add("");
                                        cells.Add("");
                                    }
                                }
                            }

                            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem3"].Rows)
                            {
                                if (r["Code"].ToString().Length > 0)
                                {
                                    if (dict3.ContainsKey(r["Code"].ToString()))
                                    {
                                        cells.Add(dict3[r["Code"].ToString()]);
                                    }
                                    else
                                    {
                                        cells.Add("");
                                    }
                                }
                            }

                            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem4"].Rows)
                            {
                                if (r["Code"].ToString().Length > 0)
                                {
                                    if (dict4.ContainsKey(r["Code"].ToString()))
                                    {
                                        cells.Add(dict4[r["Code"].ToString()]);
                                    }
                                    else
                                    {
                                        cells.Add("");
                                    }
                                }
                            }

                            cells.Add(tmp.GetDataString("STAFF"));
                            cells.Add(tmp.GetDataString("SAVE_DATE"));
                            cells.Add(tmp.GetDataString("SAVE_TIME"));

                            WriteCsvLine(writer, cells);
                        }

                        total += page_list.Count;

                        this.Text = title + " " + total.ToString("#,0") + "件";
                        Application.DoEvents();

                        if (this.IsDisposed)
                        {
                            return;
                        }

                        if (page_list.Count < PAGE_SIZE)
                        {
                            break;
                        }
                    }
                }

                MessageBox.Show("エクスポートが完了しました（" + total.ToString("#,0") + "件）");
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBox.Show(err);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.Text = title;
                    ExeButton.Enabled = true;
                    CloseButton.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}