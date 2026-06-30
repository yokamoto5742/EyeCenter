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

            List<EyeKensa> list = EyeKensa.LoadByKensasDates(kensa_id_list, start_date, end_date, true);

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

            bool b = false;

            foreach (EyeKensa kensa in list)
            {
                b = false;

                foreach (DataRow r in table.Rows)
                {
                    if (r["KENSA_DATE"].ToString().Equals(kensa.KensaDate) &&
                        r["ID"].ToString().Equals(kensa.PtId))
                    {
                        if (r["検査"].ToString().Length > 0)
                        {
                            r["検査"] += ", ";
                            r["CONT"] += "\r\n";
                        }

                        r["検査"] += kensa.KensaShort;
                        r["CONT"] += kensa.Cont;

                        b = true;
                        break;
                    }
                }

                if (!b)
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

            AppDataGridView.SexColor(KensaListView);

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

            string value = "";
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
                    if (line.Split(',').Length >= 2 && !recordDict.ContainsKey(line.Split(',')[0]))
                    {
                        value = "";

                        for (int k = 1; k < line.Split(',').Length; k++)
                        {
                            if (value.Length > 0)
                            {
                                value += ",";
                            }

                            value += line.Split(',')[k];
                        }

                        recordDict.Add(line.Split(',')[0], value.Replace("<CR+LF>", "\r\n"));
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

            if (data.ExcelOpen())
            {
                MessageBox.Show("Excel出力が完了しました");
            }
        }

        private void CSVButton_Click(object sender, EventArgs e)
        {
            TableData data = MakeTableData();

            if (data.CSVSave("検査結果検索" + DateTime.Now.ToString("yyMMdd") + ".csv", false, true, true))
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