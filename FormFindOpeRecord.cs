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
    public partial class FormFindOpeRecord : Form
    {
        DataSet dSet = new DataSet();

        CheckBox SumJoinBox = new CheckBox();

        public FormFindOpeRecord()
        {
            InitializeComponent();
        }

        private void FormFindOpeRecord_Load(object sender, EventArgs e)
        {
            DataRow tmpRow = EyeDict.EyeSet.Tables["OpeFind"].Rows[0];

            foreach (string s in tmpRow["Diag"].ToString().Split(','))
            {
                DiagBox.Items.Add(s);
            }

            foreach (string s in tmpRow["Ope"].ToString().Split(','))
            {
                OpeBox.Items.Add(s);
            }

            foreach (string s in tmpRow["Doctor"].ToString().Split(','))
            {
                DoctorBox.Items.Add(s);
            }

            RecordBox11.Items.Add("");
            RecordBox21.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTab"].Rows)
            {
                RecordBox11.Items.Add(r["Name"].ToString());
                RecordBox21.Items.Add(r["Name"].ToString());
            }

            SumJoinBox.AutoSize = true;
            SumJoinBox.Text = "CSVにサマリー結合";
            SumJoinBox.Location = new Point(365, 92);
            this.Controls.Add(SumJoinBox);

            this.DSetInit();
        }

        private void DSetInit()
        {
            DataTable tmpTable = dSet.Tables.Add("手術履歴");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("OPE_DATE");
            tmpTable.Columns.Add("手術日");
            tmpTable.Columns.Add("OPE_TIME");
            tmpTable.Columns.Add("時刻");
            tmpTable.Columns.Add("PT_ID", typeof(int));
            tmpTable.Columns.Add("カナ");
            tmpTable.Columns.Add("氏名");
            tmpTable.Columns.Add("性別");
            tmpTable.Columns.Add("生年月日");
            tmpTable.Columns.Add("年齢", typeof(int));
            tmpTable.Columns.Add("OPE_KIND");
            tmpTable.Columns.Add("種別");
            tmpTable.Columns.Add("手術室");
            tmpTable.Columns.Add("手術");
            tmpTable.Columns.Add("医師");
            tmpTable.Columns.Add("麻酔");
            tmpTable.Columns.Add("病名");
            tmpTable.Columns.Add("入外");
            tmpTable.Columns.Add("右");
            tmpTable.Columns.Add("左");
            tmpTable.Columns.Add("感染");
            tmpTable.Columns.Add("同意");
            tmpTable.Columns.Add("早期");
            tmpTable.Columns.Add("術前");
            tmpTable.Columns.Add("備考");
            tmpTable.Columns.Add("記録");
            tmpTable.Columns.Add("経過");
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            ListShow();
        }

        void ListShow()
        {
            string start_date = StartDate.Value.ToString("yyyyMMdd");
            string end_date = EndDate.Value.ToString("yyyyMMdd");

            // コントロールの値はワーカースレッドから参照しないよう先に取り出しておく
            string diag = DiagBox.Text;
            string ope = OpeBox.Text;
            string doctor = DoctorBox.Text;
            string record11 = RecordBox11.Text;
            string record12 = RecordBox12.Text.Split(' ')[0];
            string record13 = RecordBox13.Text;
            string record21 = RecordBox21.Text;
            string record22 = RecordBox22.Text.Split(' ')[0];
            string record23 = RecordBox23.Text;

            int limit = AppConfig.GetInt("FindRowLimit", 10000);

            List<EyeOpe> opeList = SearchTask.Run("手術記録を検索しています...",
                t => EyeOpe.GetList(start_date, end_date, diag, ope, doctor, record11, record12, record13, record21, record22, record23, limit, t.EyeDb, t.PatDb));

            // 中止・エラー時は表示中の一覧を維持する
            if (opeList == null)
            {
                return;
            }

            if (opeList.Count >= limit)
            {
                MessageBox.Show("検索結果が上限の " + limit.ToString("#,0") + " 件に達しました。\r\n期間や条件を絞って再検索してください。");
            }

            DataTable tmpTable = dSet.Tables["手術履歴"];
            tmpTable.Clear();

            if (opeList.Count > 0)
            {
                foreach (EyeOpe tmpOpe in opeList)
                {
                    DataRow r = tmpTable.NewRow();

                    r["ID"] = tmpOpe.Id;
                    r["OPE_DATE"] = tmpOpe.OpeDate;
                    r["手術日"] = DateTimeAgent.DateFormat(tmpOpe.OpeDate, DateTimeAgent.DateFormatKind.SHORT);

                    r["OPE_TIME"] = tmpOpe.OpeTime;
                    r["時刻"] = tmpOpe.OpeTime.PadLeft(4, '0').Insert(2, ":");

                    r["PT_ID"] = tmpOpe.Pat.Id;
                    r["カナ"] = tmpOpe.Pat.Kana;
                    r["氏名"] = tmpOpe.Pat.Name;
                    r["性別"] = tmpOpe.Pat.SexNameShort;
                    r["生年月日"] = tmpOpe.Pat.BirthString;

                    r["年齢"] = tmpOpe.Pat.AgeCalc(tmpOpe.OpeDate);

                    r["OPE_KIND"] = tmpOpe.OpeKind;

                    if (EyeDict.OpeKindDict.ContainsKey(tmpOpe.OpeKind))
                    {
                        r["種別"] = EyeDict.OpeKindDict[tmpOpe.OpeKind];
                    }
                    else
                    {
                        r["種別"] = "";
                    }

                    r["手術室"] = tmpOpe.OpeRoom;
                    r["手術"] = tmpOpe.OpeName;
                    r["医師"] = tmpOpe.Doctor;
                    r["麻酔"] = tmpOpe.Anes;
                    r["病名"] = tmpOpe.Diag;
                    r["入外"] = tmpOpe.InOut;

                    r["右"] = tmpOpe.EyeR.Equals("1") ? "○" : "";
                    r["左"] = tmpOpe.EyeL.Equals("1") ? "○" : "";

                    r["感染"] = tmpOpe.Infection.Contains("+") ? "+" : "-";
                    r["同意"] = tmpOpe.Agree.Equals("1") ? "○" : "";
                    r["早期"] = tmpOpe.EarlierOK.Equals("1") ? "○" : "";
                    r["術前"] = tmpOpe.PreCheck.Equals("1") ? "○" : "";

                    r["備考"] = tmpOpe.Comment;
                    r["記録"] = tmpOpe.OpeRecord;
                    r["経過"] = tmpOpe.OpePass;

                    tmpTable.Rows.Add(r);
                }
            }

            this.ListFormat();
        }

        void ListFormat()
        {
            DataTable table = dSet.Tables["手術履歴"];
            DataView view = new DataView(table);

            string filter = "";

            if (PreCheckBox.Checked)
            {
                filter = "術前 = '○'";
            }

            view.RowFilter = filter;

            OpeListView.DataSource = view;

            OpeListView.Columns["ID"].Visible = false;

            OpeListView.Columns["OPE_DATE"].Visible = false;

            OpeListView.Columns["手術日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            OpeListView.Columns["手術日"].Width = 55;

            OpeListView.Columns["OPE_TIME"].Visible = false;

            OpeListView.Columns["時刻"].Width = 35;
            OpeListView.Columns["時刻"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["PT_ID"].HeaderText = "ID";
            OpeListView.Columns["PT_ID"].Width = 55;
            OpeListView.Columns["PT_ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            OpeListView.Columns["カナ"].Width = 65;

            OpeListView.Columns["氏名"].Width = 75;

            OpeListView.Columns["性別"].HeaderText = "性";
            OpeListView.Columns["性別"].Width = 25;
            OpeListView.Columns["性別"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["生年月日"].Width = 70;
            OpeListView.Columns["生年月日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["年齢"].HeaderText = "齢";
            OpeListView.Columns["年齢"].Width = 35;
            OpeListView.Columns["年齢"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["OPE_KIND"].Visible = false;

            OpeListView.Columns["種別"].Width = 60;
            OpeListView.Columns["種別"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["手術室"].HeaderText = "室";
            OpeListView.Columns["手術室"].Width = 25;
            OpeListView.Columns["手術室"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["手術"].Width = 100;

            OpeListView.Columns["医師"].Width = 65;

            OpeListView.Columns["麻酔"].Width = 60;

            OpeListView.Columns["病名"].Width = 90;

            OpeListView.Columns["入外"].Width = 45;
            OpeListView.Columns["入外"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["右"].Width = 25;
            OpeListView.Columns["右"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["左"].Width = 25;
            OpeListView.Columns["左"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["感染"].Width = 30;
            OpeListView.Columns["感染"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["同意"].Visible = false;

            OpeListView.Columns["早期"].Width = 25;
            OpeListView.Columns["早期"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["術前"].Width = 25;
            OpeListView.Columns["術前"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            OpeListView.Columns["備考"].Visible = false;

            OpeListView.Columns["記録"].Visible = false;

            OpeListView.Columns["経過"].Visible = false;

            AppDataGridView.SexColor(OpeListView);

            CountLabel.Text = "件数　　" + table.Rows.Count + " 件";
        }

        /// <summary>
        /// 出力データを作成する。
        /// </summary>
        /// <returns></returns>
        TableData MakeTableData()
        {
            TableData data = new TableData();

            data.Title.Add("手術日");
            data.Title.Add("手術時刻");
            data.Title.Add("患者ID");
            data.Title.Add("カナ");
            data.Title.Add("氏名");
            data.Title.Add("性別");
            data.Title.Add("生年月日");
            data.Title.Add("年齢");
            data.Title.Add("種別");
            data.Title.Add("手術室");
            data.Title.Add("手術");
            data.Title.Add("医師");
            data.Title.Add("麻酔");
            data.Title.Add("病名");
            data.Title.Add("入外");
            data.Title.Add("右");
            data.Title.Add("左");
            data.Title.Add("感染");
            data.Title.Add("同意");
            data.Title.Add("早期");
            data.Title.Add("術前");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTabItem"].Rows)
            {
                if (r["Code"].ToString().Length > 0)
                {
                    data.Title.Add(r["Name"].ToString());
                }
            }

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpePassItem"].Rows)
            {
                foreach (string s in r["Data"].ToString().Split(','))
                {
                    data.Title.Add(r["Text"].ToString() + "_" + s);
                }
            }

            Dictionary<string, string> recordDict;
            Dictionary<string, string> passDict;

            foreach (DataGridViewRow d in OpeListView.Rows)
            {
                TableDataRecord record = new TableDataRecord();

                record.DataList.Add(d.Cells["手術日"].Value.ToString());
                record.DataList.Add(d.Cells["時刻"].Value.ToString());
                record.DataList.Add(d.Cells["PT_ID"].Value.ToString());
                record.DataList.Add(d.Cells["カナ"].Value.ToString());
                record.DataList.Add(d.Cells["氏名"].Value.ToString());
                record.DataList.Add(d.Cells["性別"].Value.ToString());
                record.DataList.Add(d.Cells["生年月日"].Value.ToString());
                record.DataList.Add(d.Cells["年齢"].Value.ToString());
                record.DataList.Add(d.Cells["種別"].Value.ToString());
                record.DataList.Add(d.Cells["手術室"].Value.ToString());
                record.DataList.Add(d.Cells["手術"].Value.ToString());
                record.DataList.Add(d.Cells["医師"].Value.ToString());
                record.DataList.Add(d.Cells["麻酔"].Value.ToString());
                record.DataList.Add(d.Cells["病名"].Value.ToString());
                record.DataList.Add(d.Cells["入外"].Value.ToString());
                record.DataList.Add(d.Cells["右"].Value.ToString());
                record.DataList.Add(d.Cells["左"].Value.ToString());
                record.DataList.Add(d.Cells["感染"].Value.ToString());
                record.DataList.Add(d.Cells["同意"].Value.ToString());
                record.DataList.Add(d.Cells["早期"].Value.ToString());
                record.DataList.Add(d.Cells["術前"].Value.ToString());

                recordDict = ContData.Parse(d.Cells["記録"].Value.ToString());
                passDict = ContData.Parse(d.Cells["経過"].Value.ToString());

                foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTabItem"].Rows)
                {
                    if (r["Code"].ToString().Length > 0)
                    {
                        if (recordDict.ContainsKey(r["Code"].ToString()))
                        {
                            record.DataList.Add(recordDict[r["Code"].ToString()]);
                        }
                        else
                        {
                            record.DataList.Add("");
                        }
                    }
                }

                foreach (DataRow r in EyeDict.EyeSet.Tables["OpePassItem"].Rows)
                {
                    foreach (string s in r["Data"].ToString().Split(','))
                    {
                        if (passDict.ContainsKey(r["Code"].ToString() + "_" + s))
                        {
                            record.DataList.Add(passDict[r["Code"].ToString() + "_" + s]);
                        }
                        else
                        {
                            record.DataList.Add("");
                        }
                    }
                }

                data.RecordList.Add(record);
            }

            return data;
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            TableData data = MakeTableData();

            if (!FormCsvColumnSelect.FilterColumns(data, "OpeRecord"))
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

            if (!FormCsvColumnSelect.FilterColumns(data, "OpeRecord"))
            {
                return;
            }

            if (SumJoinBox.Checked)
            {
                List<string> ptList = new List<string>();

                foreach (DataGridViewRow d in OpeListView.Rows)
                {
                    ptList.Add(d.Cells["PT_ID"].Value.ToString());
                }

                if (!FormSumColumnSelect.AppendSummaryColumns(data, ptList))
                {
                    return;
                }
            }

            if (data.CSVSave("手術記録検索" + DateTime.Now.ToString("yyMMdd") + ".csv", false, true, true))
            {
                MessageBox.Show("出力が完了しました");
            }
        }

        private void RecordBox11_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecordBox12.Items.Clear();
            RecordBox12.Items.Add("");
            RecordBox13.Clear();

            if (RecordBox11.Text.Length > 0)
            {
                DataRow[] tmpRows = EyeDict.EyeSet.Tables["OpeTab"].Select("Name = '" + RecordBox11.Text + "'");

                foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTabItem"].Select("OpeTab_Id = '" + tmpRows[0]["OpeTab_Id"].ToString() + "'"))
                {
                    if (r["Code"].ToString().Length > 0)
                    {
                        RecordBox12.Items.Add(r["Code"].ToString() + " " + r["Name"].ToString());
                    }
                }
            }
        }

        private void RecordBox21_SelectedIndexChanged(object sender, EventArgs e)
        {
            RecordBox22.Items.Clear();
            RecordBox22.Items.Add("");
            RecordBox23.Clear();

            if (RecordBox21.Text.Length > 0)
            {
                DataRow[] tmpRows = EyeDict.EyeSet.Tables["OpeTab"].Select("Name = '" + RecordBox21.Text + "'");

                foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTabItem"].Select("OpeTab_Id = '" + tmpRows[0]["OpeTab_Id"].ToString() + "'"))
                {
                    if (r["Code"].ToString().Length > 0)
                    {
                        RecordBox22.Items.Add(r["Code"].ToString() + " " + r["Name"].ToString());
                    }
                }
            }
        }

        private void OpeListView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                FormControl.FormPat_Show_ByRecord(OpeListView.Rows[e.RowIndex].Cells["ID"].Value.ToString());
            }
        }

        private void PreCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ListFormat();
        }
    }
}