using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormFindSummary : Form
    {
        DataSet DSet = new DataSet();

        public FormFindSummary()
        {
            InitializeComponent();
        }

        private void FormFindSummary_Load(object sender, EventArgs e)
        {
            DataRow tmpRow = EyeDict.EyeSet.Tables["Summary"].Rows[0];

            SumDiagBox.Items.Add("");

            foreach (string s in tmpRow["Diag"].ToString().Split(','))
            {
                SumDiagBox.Items.Add(s);
            }

            SumKindBox1.Items.Add("");

            foreach (string s in tmpRow["Kind1"].ToString().Split(','))
            {
                SumKindBox1.Items.Add(s);
            }

            SumKindBox2.Items.Add("");

            foreach (string s in tmpRow["Kind2"].ToString().Split(','))
            {
                SumKindBox2.Items.Add(s);
            }

            SumKindBox3.Items.Add("");

            foreach (string s in tmpRow["Kind3"].ToString().Split(','))
            {
                SumKindBox3.Items.Add(s);
            }

            DataTable table = DSet.Tables.Add("サマリ");

            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("カナ");
            table.Columns.Add("氏名");
            table.Columns.Add("性別");
            table.Columns.Add("生年月日");
            table.Columns.Add("年齢", typeof(int));
            table.Columns.Add("登録日");
            table.Columns.Add("登録時", typeof(int));
            table.Columns.Add("主病名");
            table.Columns.Add("分類1");
            table.Columns.Add("分類2");
            table.Columns.Add("分類3");
            table.Columns.Add("PLAN");
            table.Columns.Add("PASS");
            table.Columns.Add("HIST");
            table.Columns.Add("CONT1");
            table.Columns.Add("CONT2");
            table.Columns.Add("CONT3");
            table.Columns.Add("CONT4");
        }

        private void SumDiagLabel_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>();

            foreach (Object o in SumDiagBox.Items)
            {
                if (o.ToString().Length > 0)
                {
                    list.Add(o.ToString());
                }
            }

            FormSelection fs = new FormSelection(SumDiagBox, list, "+");
            fs.ShowDialog();
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            if (SumDiagBox.Text.Length == 0 && SumKindBox1.Text.Length == 0 && SumKindBox2.Text.Length == 0 && SumKindBox3.Text.Length == 0)
            {
                MessageBox.Show("検索条件を入力してください");
                return;
            }

            // コントロールの値はワーカースレッドから参照しないよう先に取り出しておく
            string diag = SumDiagBox.Text;
            string kind1 = SumKindBox1.Text;
            string kind2 = SumKindBox2.Text;
            string kind3 = SumKindBox3.Text;

            int limit = AppConfig.GetInt("FindRowLimit", 10000);

            List<EyeSummary> list = SearchTask.Run("サマリーを検索しています...",
                t => EyeSummary.Find(diag, kind1, kind2, kind3, limit, t.EyeDb, t.PatDb));

            // 中止・エラー時は表示中の一覧を維持する
            if (list == null)
            {
                return;
            }

            if (list.Count >= limit)
            {
                MessageBox.Show("検索結果が上限の " + limit.ToString("#,0") + " 件に達しました。\r\n条件を絞って再検索してください。");
            }

            DataTable table = DSet.Tables["サマリ"];
            table.Clear();

            foreach (EyeSummary sum in list)
            {
                DataRow r = table.NewRow();

                r["ID"] = sum.Pat.Id;
                r["カナ"] = sum.Pat.Kana;
                r["氏名"] = sum.Pat.Name;
                r["性別"] = sum.Pat.SexNameShort;
                r["生年月日"] = sum.Pat.BirthString;
                r["年齢"] = sum.Pat.Age;
                r["登録日"] = DateTimeAgent.DateFormat(sum.SaveDate, DateTimeAgent.DateFormatKind.LONG);
                r["登録時"] = sum.Pat.AgeCalc(sum.SaveDate);
                r["主病名"] = sum.Diag;
                r["分類1"] = sum.Kind1;
                r["分類2"] = sum.Kind2;
                r["分類3"] = sum.Kind3;
                r["PLAN"] = sum.Plan;
                r["PASS"] = sum.Pass;
                r["HIST"] = sum.Hist;
                r["CONT1"] = sum.Cont1;
                r["CONT2"] = sum.Cont2;
                r["CONT3"] = sum.Cont3;
                r["CONT4"] = sum.Cont4;

                table.Rows.Add(r);
            }

            DataView view = new DataView(table);

            SumListView.DataSource = view;

            SumListView.Columns["ID"].Width = 60;
            SumListView.Columns["ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            SumListView.Columns["カナ"].Width = 60;
            SumListView.Columns["氏名"].Width = 70;

            SumListView.Columns["性別"].Width = 40;
            SumListView.Columns["性別"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            SumListView.Columns["生年月日"].Width = 75;
            SumListView.Columns["生年月日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            SumListView.Columns["年齢"].Width = 35;
            SumListView.Columns["年齢"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            SumListView.Columns["登録日"].Width = 75;
            SumListView.Columns["登録日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            SumListView.Columns["登録時"].Width = 35;
            SumListView.Columns["登録時"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            SumListView.Columns["主病名"].Width = 180;
            SumListView.Columns["分類1"].Width = 90;
            SumListView.Columns["分類2"].Width = 90;
            SumListView.Columns["分類3"].Width = 90;

            SumListView.Columns["PLAN"].Visible = false;
            SumListView.Columns["PASS"].Visible = false;
            SumListView.Columns["HIST"].Visible = false;
            SumListView.Columns["CONT1"].Visible = false;
            SumListView.Columns["CONT2"].Visible = false;
            SumListView.Columns["CONT3"].Visible = false;
            SumListView.Columns["CONT4"].Visible = false;

            AppDataGridView.SexColor(SumListView);
        }

        private void CSVButton_Click(object sender, EventArgs e)
        {
            TableData data = new TableData();

            data.Title.Add("ID");
            data.Title.Add("カナ");
            data.Title.Add("氏名");
            data.Title.Add("性別");
            data.Title.Add("生年月日");
            data.Title.Add("年齢");
            data.Title.Add("登録日");
            data.Title.Add("登録時");

            data.Title.Add("主病名");
            data.Title.Add("分類1");
            data.Title.Add("分類2");
            data.Title.Add("分類3");
            data.Title.Add("方針");
            data.Title.Add("経過");
            data.Title.Add("履歴");

            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");

            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");
            data.Title2.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem1"].Rows)
            {
                if (r["Code"].ToString().Length > 0)
                {
                    data.Title.Add(r["Code"].ToString());
                    data.Title2.Add(r["Label"].ToString());
                }
            }

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
            {
                if (r["Code"].ToString().Length > 0)
                {
                    data.Title.Add(r["Code"].ToString());
                    data.Title.Add(r["Code"].ToString());
                    data.Title2.Add(r["Label"].ToString());
                    data.Title2.Add(r["Label"].ToString());
                }
            }

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem3"].Rows)
            {
                if (r["Code"].ToString().Length > 0)
                {
                    data.Title.Add(r["Code"].ToString());
                    data.Title2.Add(r["Label"].ToString());
                }
            }

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem4"].Rows)
            {
                if (r["Code"].ToString().Length > 0)
                {
                    data.Title.Add(r["Code"].ToString());
                    data.Title2.Add(r["Name"].ToString());
                }
            }

            Dictionary<string, string> dict1 = new Dictionary<string, string>();
            Dictionary<string, string> dict2 = new Dictionary<string, string>();
            Dictionary<string, string> dict3 = new Dictionary<string, string>();
            Dictionary<string, string> dict4 = new Dictionary<string, string>();

            foreach (DataRow row in DSet.Tables["サマリ"].Rows)
            {
                TableDataRecord d = new TableDataRecord();

                d.DataList.Add(row["ID"].ToString());
                d.DataList.Add(row["カナ"].ToString());
                d.DataList.Add(row["氏名"].ToString());
                d.DataList.Add(row["性別"].ToString());
                d.DataList.Add(row["生年月日"].ToString());
                d.DataList.Add(row["年齢"].ToString());
                d.DataList.Add(row["登録日"].ToString());
                d.DataList.Add(row["登録時"].ToString());
                d.DataList.Add(row["主病名"].ToString());
                d.DataList.Add(row["分類1"].ToString());
                d.DataList.Add(row["分類2"].ToString());
                d.DataList.Add(row["分類3"].ToString());
                d.DataList.Add(row["PLAN"].ToString());
                d.DataList.Add(row["PASS"].ToString());
                d.DataList.Add(row["HIST"].ToString());

                dict1.Clear();
                dict2.Clear();
                dict3.Clear();
                dict4.Clear();

                foreach (string s in row["CONT1"].ToString().Split('\r', '\n'))
                {
                    if (s.Split(',').Length > 1 && !dict1.ContainsKey(s.Split(',')[0]))
                    {
                        dict1.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                    }
                }

                foreach (string s in row["CONT2"].ToString().Split('\r', '\n'))
                {
                    if (s.Split(',').Length > 1 && !dict2.ContainsKey(s.Split(',')[0]))
                    {
                        dict2.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                    }
                }

                foreach (string s in row["CONT3"].ToString().Split('\r', '\n'))
                {
                    if (s.Split(',').Length > 1 && !dict3.ContainsKey(s.Split(',')[0]))
                    {
                        dict3.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                    }
                }

                foreach (string s in row["CONT4"].ToString().Split('\r', '\n'))
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

                data.RecordList.Add(d);
            }

            if (!FormCsvColumnSelect.FilterColumns(data, "Summary"))
            {
                return;
            }

            if (data.CSVSave("サマリ検索" + DateTime.Now.ToString("yyMMdd") + ".csv", false, true, true))
            {
                MessageBox.Show("出力が完了しました");
            }
        }

        private void SumListView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                FormControl.FormPat_Show(SumListView.Rows[e.RowIndex].Cells["ID"].Value.ToString(), FormPat.Mode.SHOW);
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}