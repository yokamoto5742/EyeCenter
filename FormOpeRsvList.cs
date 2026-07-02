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
    public partial class FormOpeRsvList : FormBase
    {
        DataSet dSet = new DataSet();

        private int row = 0;           // RsvGridView 内での行番号
        private string room = "";       // 手術室番号
        private int page = 0;        // 当該手術室でのページ番号

        public FormOpeRsvList()
        {
            InitializeComponent();
        }

        private void FormOpeRsvList_Load(object sender, EventArgs e)
        {
            DataTable tmpTable = dSet.Tables.Add("手術患者");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("種別");
            tmpTable.Columns.Add("手術室");
            tmpTable.Columns.Add("時間");
            tmpTable.Columns.Add("患者ID");
            tmpTable.Columns.Add("カナ");
            tmpTable.Columns.Add("氏名");
            tmpTable.Columns.Add("性別");
            tmpTable.Columns.Add("年齢");
            tmpTable.Columns.Add("術式");
            tmpTable.Columns.Add("医師");
            tmpTable.Columns.Add("予定時間");
            tmpTable.Columns.Add("麻酔");
            tmpTable.Columns.Add("病名");
            tmpTable.Columns.Add("入外");
            tmpTable.Columns.Add("入院日");
            tmpTable.Columns.Add("病室");
            tmpTable.Columns.Add("右");
            tmpTable.Columns.Add("左");
            tmpTable.Columns.Add("身長");
            tmpTable.Columns.Add("体重");
            tmpTable.Columns.Add("感染");
            tmpTable.Columns.Add("感染詳細");

            // もとは「同意書渡し済み」だったが、不要になったとのことで「禁忌」に変更。2010/08/29
            tmpTable.Columns.Add("禁忌");

            tmpTable.Columns.Add("締後");
            tmpTable.Columns.Add("備考");
            tmpTable.Columns.Add("術前チェック");
        }

        /// <summary>
        /// 該当日の手術予約一覧を表示する。
        /// </summary>
        /// <param name="ope_date"></param>
        public void Show(string ope_date)
        {
            this.Show();

            this.OriginalText = "眼科手術予約";
            this.FormTextShow();

            OpeDateTimePicker.Value = DateTime.Parse(ope_date.Insert(4, "/").Insert(7, "/"));

            KindBox.Items.Clear();
            KindBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeKind"].Rows)
            {
                KindBox.Items.Add(r["ID"] + " " + r["Name"]);
            }

            this.ListShow();
        }

        /// <summary>
        /// 該当日・該当種別・該当時刻の手術予約一覧を表示する。
        /// </summary>
        /// <param name="ope_kind"></param>
        /// <param name="ope_date"></param>
        /// <param name="ope_time"></param>
        public void Show(string ope_kind, string ope_date, string ope_time)
        {
            this.Show();

            if (EyeDict.OpeKindDict.ContainsKey(ope_kind))
            {
                this.OriginalText = EyeDict.OpeKindDict[ope_kind];
                this.FormTextShow();
            }

            OpeDateTimePicker.Value = DateTime.Parse(ope_date.Insert(4, "/").Insert(7, "/"));

            KindBox.Items.Clear();
            KindBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeKind"].Rows)
            {
                KindBox.Items.Add(r["ID"] + " " + r["Name"]);
            }

            if (EyeDict.OpeKindDict.ContainsKey(ope_kind))
            {
                KindBox.Text = ope_kind + " " + EyeDict.OpeKindDict[ope_kind];
            }

            TimeChange(ope_time);

            this.ListShow();
        }

        /// <summary>
        /// 手術日・種別に応じた時間枠コンボボックスの作成
        /// </summary>
        private void TimeChange(string default_time)
        {
            string ope_kind = "";

            if (KindBox.Text.Contains(" "))
            {
                ope_kind = KindBox.Text.Split(' ')[0];
            }

            TimeBox.Items.Clear();
            TimeBox.Items.Add("");

            if (ope_kind.Length > 0 && EyeDict.OpeKindDict.ContainsKey(ope_kind))
            {
                string[] wakus = EyeDict.GetWakus(ope_kind, OpeDateTimePicker.Value.ToString("yyyyMMdd"));
                string[] wakuNums = EyeDict.GetWakuNums(ope_kind, OpeDateTimePicker.Value.ToString("yyyyMMdd"));

                for (int i = 0; i < wakus.Length; i++)
                {
                    if (i < wakuNums.Length && !wakuNums[i].Equals("0"))
                    {
                        TimeBox.Items.Add(wakus[i]);
                    }
                }
            }

            if (TimeBox.Items.Contains(default_time))
            {
                TimeBox.Text = default_time;
            }
        }

        private void ListShow()
        {
            string kind = "";
            string time1 = "";
            string time2 = "";

            string ope_date = OpeDateTimePicker.Value.ToString("yyyyMMdd");

            DataTable tmpTable = dSet.Tables["手術患者"];
            tmpTable.Clear();

            if (KindBox.Text.Contains(" "))
            {
                kind = KindBox.Text.Split(' ')[0];
            }

            if (TimeBox.Text.Length > 0)
            {
                time1 = TimeBox.Text.Split('-')[0];
                time2 = TimeBox.Text.Split('-')[1];
            }

            List<EyeOpe> list = EyeOpe.GetListByKindDateTimes(kind, ope_date, time1, time2);

            foreach (EyeOpe obj in list)
            {
                DataRow r = tmpTable.NewRow();

                r["ID"] = obj.Id;

                if (EyeDict.OpeKindDict.ContainsKey(obj.OpeKind))
                {
                    r["種別"] = EyeDict.OpeKindDict[obj.OpeKind];
                }

                r["手術室"] = obj.OpeRoom;
                r["時間"] = DateTimeAgent.TimeFormat(obj.OpeTime);

                r["患者ID"] = obj.Pat.Id;
                r["カナ"] = obj.Pat.Kana;
                r["氏名"] = obj.Pat.Name;
                r["性別"] = obj.Pat.SexNameShort;
                r["年齢"] = obj.Pat.AgeCalc(ope_date).ToString();

                r["術式"] = obj.OpeName;
                r["医師"] = obj.Doctor;
                r["予定時間"] = obj.PlanTime;
                r["麻酔"] = obj.Anes;
                r["病名"] = obj.Diag;
                r["入外"] = obj.InOut;

                r["入院日"] = DateTimeAgent.DateFormat(obj.InDate, DateTimeAgent.DateFormatKind.SHORT);
                r["病室"] = obj.InRoom;

                /*
                foreach (PatIn pat in in_list)
                {
                    if (obj.Pat.Id.Equals(pat.Id))
                    {
                        r["病室"] = pat.Room;
                        break;
                    }
                }
                */

                r["右"] = obj.EyeR.Equals("1") ? "○" : "";
                r["左"] = obj.EyeL.Equals("1") ? "○" : "";

                r["身長"] = obj.Height;
                r["体重"] = obj.Weight;

                if (obj.Infection.Contains("+"))
                {
                    r["感染"] = "+";
                }
                else if (obj.Infection.Length > 0)
                {
                    r["感染"] = "-";
                }

                r["感染詳細"] = obj.Infection;

                r["禁忌"] = obj.Agree.Equals("1") ? "○" : "";
                r["締後"] = obj.EarlierOK.Equals("1") ? "○" : "";
                r["備考"] = obj.Comment;
                r["術前チェック"] = obj.PreCheck.Equals("1") ? "○" : "";

                tmpTable.Rows.Add(r);
            }

            RsvGridView.DataSource = new DataView(tmpTable);

            RsvGridView.Columns["ID"].Visible = false;

            RsvGridView.Columns["種別"].Width = 65;

            RsvGridView.Columns["手術室"].HeaderText = "室";
            RsvGridView.Columns["手術室"].Width = 30;
            RsvGridView.Columns["手術室"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["時間"].Width = 40;
            RsvGridView.Columns["時間"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["患者ID"].HeaderText = "ID";
            RsvGridView.Columns["患者ID"].Width = 45;
            RsvGridView.Columns["患者ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            RsvGridView.Columns["カナ"].Visible = false;

            RsvGridView.Columns["氏名"].Width = 75;

            RsvGridView.Columns["性別"].HeaderText = "性";
            RsvGridView.Columns["性別"].Width = 30;
            RsvGridView.Columns["性別"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["年齢"].HeaderText = "齢";
            RsvGridView.Columns["年齢"].Width = 30;
            RsvGridView.Columns["年齢"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["術式"].Width = 70;
            RsvGridView.Columns["医師"].Width = 65;

            RsvGridView.Columns["予定時間"].HeaderText = "予定";
            RsvGridView.Columns["予定時間"].Width = 35;
            RsvGridView.Columns["予定時間"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["麻酔"].Width = 45;
            RsvGridView.Columns["病名"].Width = 65;

            RsvGridView.Columns["入外"].Width = 40;

            RsvGridView.Columns["入院日"].Width = 55;

            RsvGridView.Columns["病室"].Width = 35;
            RsvGridView.Columns["病室"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["右"].Width = 30;
            RsvGridView.Columns["右"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["左"].Width = 30;
            RsvGridView.Columns["左"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["感染"].Width = 30;
            RsvGridView.Columns["感染"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["感染詳細"].Visible = false;

            RsvGridView.Columns["身長"].Visible = false;
            RsvGridView.Columns["体重"].Visible = false;

            RsvGridView.Columns["禁忌"].Width = 30;
            RsvGridView.Columns["禁忌"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["締後"].Width = 30;
            RsvGridView.Columns["締後"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            RsvGridView.Columns["備考"].Width = 60;

            RsvGridView.Columns["術前チェック"].HeaderText = "前Chk";
            RsvGridView.Columns["術前チェック"].Width = 30;
            RsvGridView.Columns["術前チェック"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            ((DataView)RsvGridView.DataSource).Sort = "手術室 ASC, 時間 ASC";

            AppDataGridView.SexColor(RsvGridView);

            CountLabel.Text = "件数　　" + tmpTable.Rows.Count + " 件";
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            if (KindBox.Text.Length == 0)
            {
                MessageBox.Show("種別を選択してください");
                return;
            }

            if (TimeBox.Text.Length == 0)
            {
                MessageBox.Show("時間枠を選択してください");
                return;
            }

            FormControl.FormInput_ModeChange(FormInput.Mode.PatientId);

            if (FormControl.FormInput_ShowDialog() == DialogResult.OK)
            {
                FormControl.FormPat_Show_ByNewRecord(FormControl.FormInput_CommentGet(), KindBox.Text.Split(' ')[0], OpeDateTimePicker.Value.ToString("yyyyMMdd"), TimeBox.Text);
                FormControl.FormInput_CommentClear();
            }

            this.ListShow();
        }

        private void ModButton_Click(object sender, EventArgs e)
        {
            this.FormPatOpen_From_SelectedRecord();
        }

        private void RsvGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            this.FormPatOpen_From_SelectedRecord();
        }

        /// <summary>
        /// 選択されている行のレコードに応じた FormPat（患者データ画面）を開く。
        /// </summary>
        /// <param name="record_id"></param>
        private void FormPatOpen_From_SelectedRecord()
        {
            if (RsvGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("対象の予約を選択してください");
                return;
            }

            string record_id = RsvGridView.SelectedRows[0].Cells["ID"].Value.ToString();

            FormControl.FormPat_Show_ByRecord(record_id);

            this.ListShow();
        }

        private void DelButton_Click(object sender, EventArgs e)
        {
            if (RsvGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("対象の予約を選択してください");
                return;
            }

            if (MessageBox.Show("削除しますか？\r\n該当の手術記録台帳も削除されます。", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button2) == DialogResult.OK)
            {
                string record_id = RsvGridView.SelectedRows[0].Cells["ID"].Value.ToString();

                EyeOpe.Delete(record_id, LoginUser.Id);

                MessageBox.Show("削除しました");

                this.ListShow();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void OpeDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            this.TimeChange(TimeBox.Text);
        }

        private void KindBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.TimeChange(TimeBox.Text);
        }

        private void ShowButton_Click(object sender, EventArgs e)
        {
            this.ListShow();
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            row = 0;
            room = "";
            page = 0;

            printDialog1.PrinterSettings = new System.Drawing.Printing.PrinterSettings();

            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.PrinterSettings = printDialog1.PrinterSettings;
                printDocument1.Print();

//                printPreviewDialog1.Document = printDocument1;
//                printPreviewDialog1.ShowDialog();
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Font f20 = new Font("", 20);
            Font f16 = new Font("", 16);
            Font f14 = new Font("", 14);
            Font f12 = new Font("", 12);
            Font f10 = new Font("", 10);
            Font f9 = new Font("", 9);
            Font f8 = new Font("", 8);
            Font f7 = new Font("", 7);

            Pen p1 = new Pen(Brushes.Black, 1);

            if (row < RsvGridView.RowCount)
            {
                if (!RsvGridView.Rows[row].Cells["手術室"].Value.ToString().Equals(room))
                {
                    room = RsvGridView.Rows[row].Cells["手術室"].Value.ToString();
                    page = 0;
                }

                page++;

                e.Graphics.DrawString(page + " ページ", f9, Brushes.Black, 730, 30);

                e.Graphics.DrawString(DateTime.Now.ToString("yy/MM/dd HH:mm") + " 発行", f9, Brushes.Black, 550, 30);

                e.Graphics.DrawString("手術患者一覧　［" + room + "］", f16, Brushes.Black, 50, 50);
                e.Graphics.DrawString("手術日 : " + OpeDateTimePicker.Value.ToString("yyyy年MM月dd日") + "（" + DateTimeAgent.JWeekday(OpeDateTimePicker.Value.ToString("yyyyMMdd")) + "）", f12, Brushes.Black, 320, 55);

                e.Graphics.DrawString("室", f10, Brushes.Black, 30, 120);
                e.Graphics.DrawString("時間", f9, Brushes.Black, 50, 105);
                e.Graphics.DrawString("病室", f9, Brushes.Black, 110, 105);
                e.Graphics.DrawString("患者", f9, Brushes.Black, 50, 125);
                e.Graphics.DrawString("ID", f9, Brushes.Black, 150, 105);
                e.Graphics.DrawString("性", f9, Brushes.Black, 150, 118);
                e.Graphics.DrawString("年齢", f9, Brushes.Black, 180, 118);
                e.Graphics.DrawString("眼", f9, Brushes.Black, 220, 118);
                e.Graphics.DrawString("入外", f9, Brushes.Black, 150, 131);
                e.Graphics.DrawString("入院日", f9, Brushes.Black, 195, 131);
                e.Graphics.DrawString("病名", f9, Brushes.Black, 250, 105);
                e.Graphics.DrawString("術式", f9, Brushes.Black, 370, 105);
                e.Graphics.DrawString("医師", f9, Brushes.Black, 490, 105);
                e.Graphics.DrawString("予定時間", f9, Brushes.Black, 490, 118);
                e.Graphics.DrawString("麻酔", f9, Brushes.Black, 490, 131);
                e.Graphics.DrawString("感染", f9, Brushes.Black, 580, 105);
                e.Graphics.DrawString("締切後", f9, Brushes.Black, 580, 131);
//                e.Graphics.DrawString("ビスダイン", f9, Brushes.Black, 580, 118);
//                e.Graphics.DrawString("ブドウ糖", f9, Brushes.Black, 580, 131);
                e.Graphics.DrawString("伝達", f9, Brushes.Black, 640, 105);

                e.Graphics.DrawLine(p1, 50, 148, 770, 148);

                int h = 155;

                for (int i = 0; i < 10 && row < RsvGridView.RowCount && RsvGridView.Rows[row].Cells["手術室"].Value.ToString().Equals(room); i++)
                {
                    e.Graphics.DrawRectangle(p1, new Rectangle(50, h + 15, 720, 50));
                    e.Graphics.DrawLine(p1, 50, h + 32, 150, h + 32);
                    e.Graphics.DrawLine(p1, 150, h + 15, 150, h + 65);
                    e.Graphics.DrawLine(p1, 150, h + 32, 250, h + 32);
                    e.Graphics.DrawLine(p1, 150, h + 47, 250, h + 47);
                    e.Graphics.DrawLine(p1, 250, h + 15, 250, h + 65);
                    e.Graphics.DrawLine(p1, 370, h + 15, 370, h + 65);
                    e.Graphics.DrawLine(p1, 490, h + 15, 490, h + 65);
                    e.Graphics.DrawLine(p1, 580, h + 15, 580, h + 65);
                    e.Graphics.DrawLine(p1, 490, h + 32, 640, h + 32);
                    e.Graphics.DrawLine(p1, 490, h + 47, 640, h + 47);
                    e.Graphics.DrawLine(p1, 640, h + 15, 640, h + 65);

                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["手術室"].Value.ToString(), f10, Brushes.Black, 30, h + 30);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["時間"].Value.ToString(), f9, Brushes.Black, 50, h + 18);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["病室"].Value.ToString(), f9, Brushes.Black, 110, h + 18);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["カナ"].Value.ToString(), f7, Brushes.Black, 50, h + 35);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["氏名"].Value.ToString() + " 様", f9, Brushes.Black, 50, h + 48);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["患者ID"].Value.ToString(), f9, Brushes.Black, 150, h + 18);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["性別"].Value.ToString(), f9, Brushes.Black, 150, h + 33);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["年齢"].Value.ToString() + "歳", f9, Brushes.Black, 180, h + 33);

                    if (RsvGridView.Rows[row].Cells["右"].Value.ToString().Contains("○") && RsvGridView.Rows[row].Cells["左"].Value.ToString().Contains("○"))
                    {
                        e.Graphics.DrawString("両", f9, Brushes.Black, 220, h + 33);
                    }
                    else if (RsvGridView.Rows[row].Cells["右"].Value.ToString().Contains("○"))
                    {
                        e.Graphics.DrawString("右", f9, Brushes.Black, 220, h + 33);
                    }
                    else if (RsvGridView.Rows[row].Cells["左"].Value.ToString().Contains("○"))
                    {
                        e.Graphics.DrawString("左", f9, Brushes.Black, 220, h + 33);
                    }

                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["入外"].Value.ToString(), f9, Brushes.Black, 150, h + 48);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["入院日"].Value.ToString(), f9, Brushes.Black, 195, h + 48);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["病名"].Value.ToString(), f9, Brushes.Black, new RectangleF(250, h + 18, 115, 45));
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["術式"].Value.ToString(), f9, Brushes.Black, new RectangleF(370, h + 18, 115, 45));
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["医師"].Value.ToString(), f9, Brushes.Black, 490, h + 18);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["予定時間"].Value.ToString(), f9, Brushes.Black, 490, h + 33);
                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["麻酔"].Value.ToString(), f9, Brushes.Black, 490, h + 48);

                    e.Graphics.DrawString(RsvGridView.Rows[row].Cells["感染"].Value.ToString(), f16, Brushes.Black, 580, h + 12);

                    if (RsvGridView.Rows[row].Cells["感染"].Value.ToString().Contains("+"))
                    {
                        e.Graphics.DrawString("感染症： " + RsvGridView.Rows[row].Cells["感染詳細"].Value.ToString(), f9, Brushes.Black, 420, h + 67);
                    }

                    string height = RsvGridView.Rows[row].Cells["身長"].Value.ToString();
                    string weight = RsvGridView.Rows[row].Cells["体重"].Value.ToString();

                    // ビスダイン・ブドウ糖の印字はなくす
                    // 岡本様のご要望により 2014/04/25, by sakane
                    /*
                    double d = 0;

                    if (double.TryParse(height, out d) && double.TryParse(weight, out d))
                    {
                        e.Graphics.DrawString(EyeDict.CalcVisdine(double.Parse(height), double.Parse(weight)).ToString(), f9, Brushes.Black, 580, h + 33);
                        e.Graphics.DrawString(EyeDict.CalcGrape(double.Parse(height), double.Parse(weight)).ToString(), f9, Brushes.Black, 580, h + 48);
                    }
                     */

                    if (RsvGridView.Rows[row].Cells["締後"].Value.ToString().Equals("○"))
                    {
                        e.Graphics.DrawString("締切後", f9, Brushes.Black, 580, h + 48);
                    }

                    if (RsvGridView.Rows[row].Cells["備考"].Value.ToString().Length > 15)
                    {
                        e.Graphics.DrawString(RsvGridView.Rows[row].Cells["備考"].Value.ToString().Substring(0, 15) + " ...", f9, Brushes.Black, new RectangleF(640, h + 18, 140, 45));
                    }
                    else
                    {
                        e.Graphics.DrawString(RsvGridView.Rows[row].Cells["備考"].Value.ToString(), f9, Brushes.Black, new RectangleF(640, h + 18, 140, 45));
                    }

                    h += 75;
                    row++;
                }
                
                if (row < RsvGridView.RowCount)
                {
                    e.HasMorePages = true;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
        }
    }
}