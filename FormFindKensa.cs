using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormFindKensa : Form
    {
        DataSet dSet = new DataSet();

        public FormFindKensa()
        {
            InitializeComponent();
        }

        private void FormFindKensa_Load(object sender, EventArgs e)
        {
            DataTable table = dSet.Tables.Add("検査結果");
            table.Columns.Add("KENSA_DATE");
            table.Columns.Add("日付");
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("カナ");
            table.Columns.Add("氏名");
            table.Columns.Add("性別");
            table.Columns.Add("生年月日");
            table.Columns.Add("年齢", typeof(int));
            table.Columns.Add("検査");
            table.Columns.Add("CONT");

            foreach (DataRow r in EyeDict.EyeSet.Tables["KensaPage"].Select("PageType = '1'", "ID"))
            {
                KensaListBox.Items.Add(r["ID"].ToString() + " " + r["Name"].ToString());
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            string start_date = StartDate.Value.ToString("yyyyMMdd");
            string end_date = EndDate.Value.ToString("yyyyMMdd");

            List<string> kensa_id_list = new List<string>();

            foreach (string s in KensaListBox.CheckedItems)
            {
                if (s.Contains(" "))
                {
                    kensa_id_list.Add(s.Split(' ')[0]);
                }
            }

            int limit = AppConfig.GetInt("FindRowLimit", 10000);

            List<EyeKensa> list = SearchTask.Run("検査結果を検索しています...",
                t => EyeKensa.LoadByKensasDates(kensa_id_list, start_date, end_date, true, limit, t.EyeDb, t.PatDb));

            // 中止・エラー時は表示中の一覧を維持する
            if (list == null)
            {
                return;
            }

            if (list.Count >= limit)
            {
                MessageBox.Show("検索結果が上限の " + limit.ToString("#,0") + " 件に達しました。\r\n期間や検査の種類を絞って再検索してください。");
            }

            // 患者ID, 検査日, 検査ID でソート
            list.Sort((x, y) =>
            {
                int i = 0;

                int ix = int.Parse(x.PtId);
                int iy = int.Parse(y.PtId);
                i = ix - iy;

                if (i.Equals(0))
                {
                    ix = int.Parse(x.KensaDate);
                    iy = int.Parse(y.KensaDate);
                    i = ix - iy;
                }

                if (i.Equals(0))
                {
                    ix = int.Parse(x.KensaId);
                    iy = int.Parse(y.KensaId);
                    i = ix - iy;
                }

                return i;
            });

            DataTable table = dSet.Tables["検査結果"];
            table.Clear();

            // 患者ID・検査日でソート済みのため、同じ患者・同じ日の検査は直前の行にまとめる
            DataRow last = null;

            foreach (EyeKensa kensa in list)
            {
                if (last != null &&
                    last["KENSA_DATE"].ToString().Equals(kensa.KensaDate) &&
                    last["ID"].ToString().Equals(kensa.PtId))
                {
                    if (last["検査"].ToString().Length > 0)
                    {
                        last["検査"] += ", ";
                        last["CONT"] += "\r\n";
                    }

                    last["検査"] += kensa.KensaShort;
                    last["CONT"] += kensa.Cont;
                }
                else
                {
                    DataRow r = table.NewRow();

                    r["KENSA_DATE"] = kensa.KensaDate;
                    r["日付"] = DateTimeAgent.DateFormat(int.Parse(kensa.KensaDate), DateTimeAgent.DateFormatKind.SHORT);
                    r["ID"] = kensa.Pat.Id;
                    r["カナ"] = kensa.Pat.Kana;
                    r["氏名"] = kensa.Pat.Name;
                    r["性別"] = kensa.Pat.SexNameEng;
                    r["生年月日"] = kensa.Pat.BirthString;
                    r["年齢"] = kensa.Pat.AgeCalc(kensa.KensaDate);
                    r["検査"] = kensa.KensaShort;
                    r["CONT"] = kensa.Cont;

                    table.Rows.Add(r);
                    last = r;
                }
            }

            KensaListView.DataSource = new DataView(table);

            KensaListView.Columns["KENSA_DATE"].Visible = false;

            KensaListView.Columns["日付"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            KensaListView.Columns["日付"].Width = 55;

            KensaListView.Columns["ID"].Width = 55;
            KensaListView.Columns["ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            KensaListView.Columns["カナ"].Width = 70;

            KensaListView.Columns["氏名"].Width = 80;

            KensaListView.Columns["性別"].HeaderText = "性";
            KensaListView.Columns["性別"].Width = 25;
            KensaListView.Columns["性別"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            KensaListView.Columns["生年月日"].Width = 70;
            KensaListView.Columns["生年月日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            KensaListView.Columns["年齢"].HeaderText = "齢";
            KensaListView.Columns["年齢"].Width = 25;
            KensaListView.Columns["年齢"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            KensaListView.Columns["検査"].Width = 80;

            KensaListView.Columns["CONT"].Visible = false;

            CountLabel.Text = "人数　　" + table.Rows.Count + " 人";
        }

        /// <summary>
        /// 出力データを作成する。
        /// </summary>
        /// <returns></returns>
        TableData MakeTableData()
        {
            TableData data = new TableData();

            data.Title.Add("日付");
            data.Title.Add("ID");
            data.Title.Add("カナ");
            data.Title.Add("氏名");
            data.Title.Add("性別");
            data.Title.Add("生年月日");
            data.Title.Add("年齢");

            List<EyeKensaItemMaster> list;

            foreach (string s in KensaListBox.CheckedItems)
            {
                if (s.Contains(" "))
                {
                    list = EyeKensaItemMaster.ListByKensaId(s.Split(' ')[0]);

                    foreach (EyeKensaItemMaster kensa in list)
                    {
                        data.Title.Add(kensa.Code);
                    }
                }
            }

            int comma = 0;
            string key = "";
            Dictionary<string, string> recordDict = new Dictionary<string, string>();
            int i = 0;

            foreach (DataGridViewRow d in KensaListView.Rows)
            {
                TableDataRecord record = new TableDataRecord();

                record.DataList.Add(d.Cells["日付"].Value.ToString());
                record.DataList.Add(d.Cells["ID"].Value.ToString());
                record.DataList.Add(d.Cells["カナ"].Value.ToString());
                record.DataList.Add(d.Cells["氏名"].Value.ToString());
                record.DataList.Add(d.Cells["性別"].Value.ToString());
                record.DataList.Add(d.Cells["生年月日"].Value.ToString());
                record.DataList.Add(d.Cells["年齢"].Value.ToString());

                recordDict.Clear();

                foreach (string line in d.Cells["CONT"].Value.ToString().Split('\r', '\n'))
                {
                    // 最初のカンマより前が項目コード、後ろ（カンマを含む）が値
                    comma = line.IndexOf(',');

                    if (comma < 0)
                    {
                        continue;
                    }

                    key = line.Substring(0, comma);

                    if (!recordDict.ContainsKey(key))
                    {
                        recordDict.Add(key, line.Substring(comma + 1).Replace("<CR+LF>", "\r\n"));
                    }
                }

                for (i = 7; i < data.Title.Count; i++)
                {
                    if (recordDict.ContainsKey(data.Title[i]))
                    {
                        record.DataList.Add(recordDict[data.Title[i]]);
                    }
                    else
                    {
                        record.DataList.Add("");
                    }
                }

                data.RecordList.Add(record);
            }

            return data;
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            TableData data = MakeTableData();

            if (!FormCsvColumnSelect.FilterColumns(data, "Kensa"))
            {
                return;
            }

            if (data.ExcelOpen())
            {
                MessageBox.Show("Excel出力が完了しました");
            }
        }

        private void CSVButton_Click(object sender, EventArgs e)
        {
            TableData data = MakeTableData();

            if (!FormCsvColumnSelect.FilterColumns(data, "Kensa"))
            {
                return;
            }

            if (data.CSVSave("検査結果検索" + DateTime.Now.ToString("yyMMdd") + ".csv", false, true, true))
            {
                MessageBox.Show("出力が完了しました");
            }
        }

        /// <summary>
        /// レフ・ケラト（NIDEK ARK）の代表値を項目ごとに分けた出力データを作成する。
        /// 期間は画面の日付範囲を使い、1測定（SEQ）を1行として右眼・左眼を横に並べる。
        /// </summary>
        /// <returns>中止・エラー時は null</returns>
        TableData MakeRefKrtTableData()
        {
            string start_date = StartDate.Value.ToString("yyyyMMdd");
            string end_date = EndDate.Value.ToString("yyyyMMdd");

            int limit = AppConfig.GetInt("FindRowLimit", 10000);

            List<EyeKensa2> list = SearchTask.Run("レフ・ケラトを検索しています...",
                t => EyeKensa2.LoadByKensaDates("18", start_date, end_date, true, limit, t.EyeDb, t.PatDb));

            if (list == null)
            {
                return null;
            }

            if (list.Count >= limit)
            {
                MessageBox.Show("検索結果が上限の " + limit.ToString("#,0") + " 件に達しました。\r\n期間を絞って再度出力してください。");
            }

            // 患者ID, 検査日, SEQ でソート
            list.Sort((x, y) =>
            {
                int i = int.Parse(x.PtId) - int.Parse(y.PtId);

                if (i.Equals(0))
                {
                    i = int.Parse(x.KensaDate) - int.Parse(y.KensaDate);
                }

                if (i.Equals(0))
                {
                    i = int.Parse(x.KensaSEQ) - int.Parse(y.KensaSEQ);
                }

                return i;
            });

            TableData data = new TableData();

            data.Title.Add("日付");
            data.Title.Add("ID");
            data.Title.Add("カナ");
            data.Title.Add("氏名");
            data.Title.Add("性別");
            data.Title.Add("生年月日");
            data.Title.Add("年齢");
            data.Title.Add("SEQ");
            data.Title.Add("機種");
            data.Title.Add("測定日時");

            foreach (string eye in new[] { "R_", "L_" })
            {
                foreach (string s in NidekArkParser.EyeTitles)
                {
                    data.Title.Add(eye + s);
                }
            }

            foreach (EyeKensa2 kensa in list)
            {
                // NIDEK ARK 以外（CANON など）の形式は出力しない
                NidekArkParser.Result result = NidekArkParser.Parse(kensa.Cont);

                if (result == null)
                {
                    continue;
                }

                TableDataRecord record = new TableDataRecord();

                record.DataList.Add(DateTimeAgent.DateFormat(int.Parse(kensa.KensaDate), DateTimeAgent.DateFormatKind.SHORT));
                record.DataList.Add(kensa.Pat.Id);
                record.DataList.Add(kensa.Pat.Kana);
                record.DataList.Add(kensa.Pat.Name);
                record.DataList.Add(kensa.Pat.SexNameEng);
                record.DataList.Add(kensa.Pat.BirthString);
                record.DataList.Add(kensa.Pat.AgeCalc(kensa.KensaDate).ToString());
                record.DataList.Add(kensa.KensaSEQ);
                record.DataList.Add(result.Model);
                record.DataList.Add(result.MeasuredAt);
                record.DataList.AddRange(result.R.ToArray());
                record.DataList.AddRange(result.L.ToArray());

                data.RecordList.Add(record);
            }

            return data;
        }

        private void RefKrtExcelButton_Click(object sender, EventArgs e)
        {
            TableData data = MakeRefKrtTableData();

            if (data == null || !FormCsvColumnSelect.FilterColumns(data, "RefKrt"))
            {
                return;
            }

            if (data.ExcelOpen())
            {
                MessageBox.Show("Excel出力が完了しました");
            }
        }

        private void RefKrtCSVButton_Click(object sender, EventArgs e)
        {
            TableData data = MakeRefKrtTableData();

            if (data == null || !FormCsvColumnSelect.FilterColumns(data, "RefKrt"))
            {
                return;
            }

            if (data.CSVSave("レフケラ" + DateTime.Now.ToString("yyMMdd") + ".csv", false, true, true))
            {
                MessageBox.Show("出力が完了しました");
            }
        }

        private void KensaListView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                FormControl.FormPat_Show(KensaListView.Rows[e.RowIndex].Cells["ID"].Value.ToString(), FormPat.Mode.SHOW);
            }
        }
    }
}