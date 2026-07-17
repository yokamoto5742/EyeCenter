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
    class OpeCalCount
    {
        public string Date = "";

        public string InOut = "";

        public int Count = 0;
    }

    public partial class FormOpeCal : Form
    {
        public string OpeKind = "";

        int ShowDays = 14;

        public FormOpeCal(string ope_kind)
        {
            InitializeComponent();

            if (EyeDict.OpeKindDict.ContainsKey(ope_kind))
            {
                OpeKind = ope_kind;
                this.Text = EyeDict.OpeKindDict[ope_kind];
            }
            else
            {
                OpeKind = "2";
                this.Text = "眼科手術予約";
            }

            int.TryParse(EyeDict.EyeSet.Tables["OpeShow"].Rows[0]["Days"].ToString(), out ShowDays);
        }

        private void FormOpeCal_Load(object sender, EventArgs e)
        {
            this.RsvTableShow(CritDateTimePicker.Value);
        }

        /// <summary>
        /// 予約カレンダーを表示する
        /// </summary>
        public void RsvTableShow()
        {
            this.RsvTableShow(CritDateTimePicker.Value);
        }

        /// <summary>
        /// 予約カレンダーを表示する
        /// </summary>
        /// <param name="crit_date"></param>
        private void RsvTableShow(DateTime crit_date)
        {
            opeCalPanel.Controls.Clear();

            int x = 0;
            int y = 0;

            const int TIME_WIDTH = 38;
            const int TIME_HEIGHT = 28;

            const int DATE_WIDTH = 38;
            const int DATE_HEIGHT = 38;

            const int DATA_WIDTH = 38;
            const int DATA_HEIGHT = 58;

            const int WAKU_WIDTH = DATE_WIDTH;
            const int WAKU_HEIGHT = TIME_HEIGHT;

            const int WAKU_SPAN_X = WAKU_WIDTH + 2;
            const int WAKU_SPAN_Y = TIME_HEIGHT + 2;

            const int DATE_SHOW_INTERVAL = 10;

            Size WakuLabelSize = new Size(WAKU_WIDTH, WAKU_HEIGHT);
            Size TimeLabelSize = new Size(TIME_WIDTH, TIME_HEIGHT);
            Size DateLabelSize = new Size(DATE_WIDTH, DATE_HEIGHT);

            x = 0;
            y = DATA_HEIGHT + 2 + DATE_HEIGHT + 2;

            string[] wakus = EyeDict.GetWakus(OpeKind, crit_date.ToString("yyyyMMdd"));
            string[] wakuNums;

            int date_show_count = 0;
            List<Point> date_point_list = new List<Point>();

            // 日付ラベルの始点
            date_point_list.Add(new Point(TIME_WIDTH + 2, DATA_HEIGHT + 2));
            
            // 時間ラベルの作成
            for (int i = 0; i < wakus.Length; i++)
            {
                Label tmpLabel = new Label();
                tmpLabel.Text = wakus[i].Split('-')[0].PadLeft(4, '0').Insert(2, ":");
                tmpLabel.Tag = wakus[i].Split('-')[0].PadLeft(4, '0').Insert(2, ":") + "\r\n-\r\n" + wakus[i].Split('-')[1].PadLeft(4, '0').Insert(2, ":");
                tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                tmpLabel.BackColor = Color.LightYellow;
                tmpLabel.Size = TimeLabelSize;
                tmpLabel.Location = new Point(x, y);
                opeCalPanel.Controls.Add(tmpLabel);

                y += WAKU_SPAN_Y;

                date_show_count++;

                if (date_show_count >= DATE_SHOW_INTERVAL)
                {
                    // 日付ラベル作成地点の追加
                    date_point_list.Add(new Point(DATE_WIDTH + 2, y));

                    y += DATE_HEIGHT + 2;
                    date_show_count = 0;
                }
            }

            // 枠外ラベルの作成
            Label etcLabel = new Label();
            etcLabel.Text = "他";
            etcLabel.TextAlign = ContentAlignment.MiddleCenter;
            etcLabel.BackColor = Color.LightYellow;
            etcLabel.Size = TimeLabelSize;
            etcLabel.Location = new Point(x, y);
            opeCalPanel.Controls.Add(etcLabel);

            x = DATE_WIDTH + 2;
            y = 0;

            string[] wday = { "日", "月", "火", "水", "木", "金", "土" };

            // 当該種別時間枠の最終日を求める
            DateTime endDate = DateTime.Parse("2999/12/31");

            if (EyeDict.GetEnd(OpeKind, crit_date.ToString("yyyyMMdd")).Length == 8)
            {
                endDate = DateTime.Parse(EyeDict.GetEnd(OpeKind, crit_date.ToString("yyyyMMdd")).Insert(4, "/").Insert(7, "/"));
            }

            // 日付枠の数を調べる
            int wakuDays = 0;

            for (int i = 0; i < ShowDays; i++)
            {
                // 当該種別の最終日を超えたら終わり
                if (crit_date.AddDays(i) > endDate)
                {
                    break;
                }

                wakuDays++;
            }

            // 日付ラベルの作成
            foreach (Point dp in date_point_list)
            {
                x = dp.X;
                y = dp.Y;

                for (int i = 0; i < ShowDays; i++)
                {
                    Label tmpLabel = new Label();
                    tmpLabel.Text = crit_date.AddDays(i).ToString("M/d") + "\r\n" + wday[(int)(crit_date.AddDays(i).DayOfWeek)];
                    tmpLabel.TextAlign = ContentAlignment.MiddleCenter;

                    if (crit_date.AddDays(i).DayOfWeek == DayOfWeek.Sunday)
                    {
                        tmpLabel.BackColor = Color.LightPink;
                    }
                    else if (Dict.HolidayDict.ContainsKey(crit_date.AddDays(i).ToString("yyyyMMdd")))
                    {
                        tmpLabel.BackColor = Color.LightPink;
                    }
                    else if (crit_date.AddDays(i).DayOfWeek == DayOfWeek.Saturday)
                    {
                        tmpLabel.BackColor = Color.LightBlue;
                    }
                    else
                    {
                        tmpLabel.BackColor = Color.LightYellow;
                    }

                    tmpLabel.BorderStyle = BorderStyle.FixedSingle;
                    tmpLabel.Size = DateLabelSize;
                    tmpLabel.Location = new Point(x, y);
                    tmpLabel.Tag = this.OpeKind + "," + crit_date.AddDays(i).ToString("yyyyMMdd") + ",0-9999";
                    tmpLabel.DoubleClick += new EventHandler(tmpLabel_DoubleClick);
                    opeCalPanel.Controls.Add(tmpLabel);

                    x += WAKU_SPAN_X;
                }
            }

            x = TIME_WIDTH + 2;

            int rsv_waku = 0;

            // 予約が入っている人数
            int[,] rsv_num = new int[wakuDays, wakus.Length + 1];

            // 患者IDのリスト
            string[,] rsv_pt = new string[wakuDays, wakus.Length + 1];

            // 日別・種類別の件数
            List<OpeCalCount> data_list = new List<OpeCalCount>();

            int rsv_waku_x = 0;
            int rsv_waku_y = 0;

            for (rsv_waku_x = 0; rsv_waku_x < wakuDays; rsv_waku_x++)
            {
                for (rsv_waku_y = 0; rsv_waku_y < wakus.Length + 1; rsv_waku_y++)
                {
                    rsv_num[rsv_waku_x, rsv_waku_y] = 0;
                    rsv_pt[rsv_waku_x, rsv_waku_y] = "";
                }
            }

            rsv_waku_x = 0;
            rsv_waku_y = 0;

            List<EyeOpe> tmp_list = EyeOpe.GetListByKindDates(OpeKind, crit_date.ToString("yyyyMMdd"), crit_date.AddDays(wakuDays - 1).ToString("yyyyMMdd"));

            string tmp_date = "";
            string ope_date = "";
            string in_out = "";

            int ope_time = 0;
            int plan_time = 0;
            int waku_time1 = 0;
            int waku_time2 = 0;
            int waku_no = 0;
            bool waku_flg = false;

            foreach (EyeOpe tmp in tmp_list)
            {
                tmp_date = tmp.OpeDate;

                if (tmp_date != ope_date)
                {
                    ope_date = tmp_date;

                    OpeCalCount data = new OpeCalCount();
                    data.Date = ope_date;
                    data.InOut = "外来";
                    data_list.Add(data);

                    data = new OpeCalCount();
                    data.Date = ope_date;
                    data.InOut = "あやめ";
                    data_list.Add(data);

                    data = new OpeCalCount();
                    data.Date = ope_date;
                    data.InOut = "わかば";
                    data_list.Add(data);
                }

                in_out = tmp.InOut;

                ope_time = 0;
                plan_time = 0;
                waku_time1 = 0;
                waku_time2 = 0;
                waku_flg = false;

                if (ope_date.Equals(crit_date.ToString("yyyyMMdd")))
                {
                    rsv_waku_x = 0;
                }
                else
                {
                    rsv_waku_x = (DateTime.Parse(ope_date.Insert(4, "/").Insert(7, "/")).Subtract(crit_date)).Days + 1;
                }

                int.TryParse(tmp.OpeTime, out ope_time);
                int.TryParse(tmp.PlanTime, out plan_time);

                if (ope_time > 0)
                {
                    for (waku_no = 0; waku_no < wakus.Length; waku_no++)
                    {
                        waku_time1 = int.Parse(wakus[waku_no].Split('-')[0]);
                        waku_time2 = int.Parse(wakus[waku_no].Split('-')[1]);

                        if ((ope_time >= waku_time1 && ope_time < waku_time2) ||
                            (ope_time < waku_time1 && DateTimeAgent.AddTime(ope_time, plan_time) > waku_time2) ||
                            (DateTimeAgent.AddTime(ope_time, plan_time) > waku_time1 && DateTimeAgent.AddTime(ope_time, plan_time) <= waku_time2))
                        {
                            rsv_num[rsv_waku_x, waku_no] += 1;

                            if (rsv_pt[rsv_waku_x, waku_no].Length > 0)
                            {
                                rsv_pt[rsv_waku_x, waku_no] += "|";
                            }

                            rsv_pt[rsv_waku_x, waku_no] += tmp.PtId;

                            waku_flg = true;
                        }
                    }
                }

                if (!waku_flg)
                {
                    rsv_num[rsv_waku_x, wakus.Length] += 1;

                    if (rsv_pt[rsv_waku_x, wakus.Length].Length > 0)
                    {
                        rsv_pt[rsv_waku_x, wakus.Length] += "|";
                    }

                    rsv_pt[rsv_waku_x, wakus.Length] += tmp.PtId;
                }

                bool b = false;

                foreach (OpeCalCount data in data_list)
                {
                    if (data.Date == ope_date && data.InOut == in_out)
                    {
                        data.Count++;
                        b = true;
                        break;
                    }
                }

                if (!b)
                {
                    OpeCalCount data = new OpeCalCount();

                    data.Date = ope_date;
                    data.InOut = in_out;
                    data.Count = 1;

                    data_list.Add(data);
                }
            }

            // 当該種別・期間の休診情報を取得する
            List<EyeOpeRsv> rsvList = EyeOpeRsv.Find(crit_date.ToString("yyyyMMdd"), endDate.ToString("yyyyMMdd"), OpeKind);
            string rsv_kind = "";
            string rsv_comment = "";

            // 各日の予約情報ラベルを作成する
            for (int i = 0; i < wakuDays; i++)
            {
                ope_date = crit_date.AddDays(i).ToString("yyyyMMdd");

                Label tmpLabel = new Label();

                // 件数ラベルの内容
                string data_str = "";

                // 件数ラベルを作成する
                foreach (OpeCalCount data in data_list)
                {
                    if (data.Date == ope_date)
                    {
                        if (data_str.Length > 0)
                        {
                            data_str += "\r\n";
                        }

                        if (data.InOut.Length > 0)
                        {
                            data_str += data.InOut.Substring(0, 1);
                        }
                        else
                        {
                            data_str += "空";
                        }

                        data_str += " " + data.Count;
                    }
                }

                tmpLabel.Location = new Point(x, 0);
                tmpLabel.Size = new Size(DATA_WIDTH, DATA_HEIGHT);
                tmpLabel.Text = data_str;
                tmpLabel.BackColor = Color.White;
                tmpLabel.TextAlign = ContentAlignment.TopCenter;
                tmpLabel.Padding = new Padding(2);
                opeCalPanel.Controls.Add(tmpLabel);

                y = DATA_HEIGHT + 2 + DATE_HEIGHT + 2;
                date_show_count = 0;

                wakuNums = EyeDict.GetWakuNums(OpeKind, ope_date);

                // 各枠の予約情報ラベルを作成する
                for (int j = 0; j < wakus.Length + 1; j++)
                {
                    // 診療・休診情報を取得する
                    rsv_kind = "";
                    rsv_comment = "";

                    foreach (EyeOpeRsv rsv in rsvList)
                    {
                        // リストの検索対象が当該日を超えたら終了する
                        if (Int32.Parse(rsv.OpeDate) > Int32.Parse(ope_date))
                        {
                            break;
                        }

                        // 日付・枠・種別が一致したら終了する
                        if (rsv.OpeDate.Equals(ope_date) && j < wakus.Length && rsv.OpeWaku.Equals(wakus[j]) && rsv.OpeKind.Equals(OpeKind))
                        {
                            rsv_kind = rsv.RsvKind;
                            rsv_comment = rsv.Comment;
                            break;
                        }
                    }

                    tmpLabel = new Label();

                    if (j < wakus.Length)
                    {
                        rsv_waku = int.Parse(wakuNums[j]);
                    }
                    else
                    {
                        rsv_waku = 0;
                    }

                    tmpLabel.Text = rsv_num[i, j] + "/" + rsv_waku;

                    if (rsv_kind.Equals("1") || rsv_kind.Equals("2"))
                    {
                        // 診察・休診設定がある場合

                        if (rsv_kind.Equals("1"))
                        {
                            tmpLabel.BackColor = Color.White;

                            if (rsv_comment.Length > 0)
                            {
                                tmpLabel.Text += "\r\n臨診\r\n" + rsv_comment;
                            }
                            else
                            {
                                tmpLabel.Text += "\r\n臨診";
                            }
                        }
                        else
                        {
                            if (rsv_num[i, j] > 0)
                            {
                                tmpLabel.BackColor = Color.Red;
                            }
                            else
                            {
                                tmpLabel.BackColor = Color.LightGray;
                            }

                            if (rsv_comment.Length > 0)
                            {
                                tmpLabel.Text += "\r\n臨休\r\n" + rsv_comment;
                            }
                            else
                            {
                                tmpLabel.Text += "\r\n臨休";
                            }
                        }
                    }
                    else
                    {
                        // 診察・休診設定が無い場合

                        if (rsv_num[i, j] > rsv_waku)
                        {
                            tmpLabel.BackColor = Color.Red;
                        }
                        else if (rsv_num[i, j] == rsv_waku)
                        {
                            if (rsv_num[i, j] == 0)
                            {
                                tmpLabel.BackColor = Color.LightGray;
                                tmpLabel.ForeColor = Color.LightGray;
                            }
                            else
                            {
                                tmpLabel.BackColor = Color.Pink;
                            }
                        }
                        else if (rsv_num[i, j] >= rsv_waku - 1)
                        {
                            tmpLabel.BackColor = Color.Yellow;
                        }
                        else
                        {
                            tmpLabel.BackColor = Color.White;
                        }
                    }
                    
                    tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                    tmpLabel.Size = WakuLabelSize;
                    tmpLabel.Location = new Point(x, y);

                    if (j < wakus.Length)
                    {
                        tmpLabel.Tag = this.OpeKind + "," + ope_date + "," + wakus[j] + "," + rsv_kind + "," + rsv_comment + "," + tmpLabel.BackColor.Name + "," + rsv_pt[i, j];
                        tmpLabel.ContextMenuStrip = RsvMenu;
                    }
                    else
                    {
                        tmpLabel.Tag = this.OpeKind + "," + ope_date + ",0-9999," + rsv_kind + "," + rsv_comment + "," + tmpLabel.BackColor.Name + "," + rsv_pt[i, j];
                    }

                    tmpLabel.MouseClick += new MouseEventHandler(tmpLabel_MouseClick);
                    tmpLabel.DoubleClick += new EventHandler(tmpLabel_DoubleClick);
                    opeCalPanel.Controls.Add(tmpLabel);

                    y += WAKU_SPAN_Y;

                    date_show_count++;

                    // 日付ラベルを挿入するところは、少し下げる
                    if (date_show_count >= DATE_SHOW_INTERVAL)
                    {
                        y += DATE_HEIGHT + 2;
                        date_show_count = 0;
                    }
                }

                x += WAKU_SPAN_X;
            }

            int p = 0;
            string pt_id = this.MdiParent.Controls["PtIdBox"].Text;

            if (pt_id.Length > 0 && int.TryParse(pt_id, out p))
            {
                this.PtTwinkle(pt_id);
            }
        }

        /// <summary>
        /// 該当患者の枠を光らせる
        /// </summary>
        /// <param name="pt_id"></param>
        public void PtTwinkle(string pt_id)
        {
            foreach (Control c in opeCalPanel.Controls)
            {
                if (c is Label && c.Tag != null && c.Tag.ToString().Split(',').Length > 6)
                {
                    string bg_color = c.Tag.ToString().Split(',')[5];
                    string pt_ids = c.Tag.ToString().Split(',')[6];

                    ((Label)c).BackColor =  Color.FromName(bg_color);

                    for (int i = 0; i < pt_ids.Split('|').Length; i++)
                    {
                        if (pt_ids.Split('|')[i].Equals(pt_id))
                        {
                            ((Label)c).BackColor = Color.Blue;
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 予約枠にマウスを当てると日付・時刻がポップアップ表示される。
        /// ※これを実行すると、ダブルクリックで予約一覧を表示できなくなるため、現在は使われていない。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void tmpLabel_MouseHover(object sender, EventArgs e)
        {
            Label tmpLabel = (Label)(sender);

            // 休診枠でなければポップアップヘルプが表示される。
            if (tmpLabel.BackColor != Color.LightGray)
            {
                string[] tmpTags = tmpLabel.Tag.ToString().Split(',');

                string date = tmpTags[1].PadLeft(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                string wday = DateTimeAgent.JWeekday(tmpTags[1].PadLeft(8, '0'));

                Help.ShowPopup(tmpLabel, date + "(" + wday + ")" + "\r\n" + tmpTags[2], Control.MousePosition);
            }
        }

        void tmpLabel_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                Label tmpLabel = (Label)(sender);
                string[] tmpTags = tmpLabel.Tag.ToString().Split(',');

                if (tmpTags[3].Equals("1") || tmpTags[3].Equals("2"))
                {
                    RsvMenu.Items[3].Enabled = true;
                }
                else
                {
                    RsvMenu.Items[3].Enabled = false;
                }
            }
        }

        void tmpLabel_DoubleClick(object sender, EventArgs e)
        {
            Label tmpLabel = (Label)sender;
            string ope_kind = tmpLabel.Tag.ToString().Split(',')[0];
            string ope_date = tmpLabel.Tag.ToString().Split(',')[1];
            string ope_time = tmpLabel.Tag.ToString().Split(',')[2];

            FormControl.FormOpeRsvList_Show(ope_date);

//            this.RsvTableShow(CritDateTimePicker.Value);
        }

        private void CritDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            this.RsvTableShow(CritDateTimePicker.Value);
        }

        private void RsvMenu_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip tmpStrip = (ContextMenuStrip)(sender);
            Label tmpLabel = (Label)(tmpStrip.SourceControl);

            if (tmpLabel.BackColor == Color.LightGray)
            {
                RsvMenu.Items["RsvMenuItem4"].Enabled = false;
            }
            else
            {
                RsvMenu.Items["RsvMenuItem4"].Enabled = true;
            }
        }

        private void RsvMenuItem1_Click(object sender, EventArgs e)
        {
            ToolStripItem tmpItem = (ToolStripItem)(sender);
            ContextMenuStrip tmpStrip = (ContextMenuStrip)(tmpItem.Owner);
            Label tmpLabel = (Label)(tmpStrip.SourceControl);
            string[] tmpTags = tmpLabel.Tag.ToString().Split(',');

            EyeOpeRsv tmpRsv = new EyeOpeRsv();
            tmpRsv.OpeKind = tmpTags[0];
            tmpRsv.OpeDate = tmpTags[1];
            tmpRsv.OpeWaku = tmpTags[2];

            tmpRsv.RsvKind = "1";

            FormControl.FormInput_ModeChange(FormInput.Mode.Comment);

            if (FormControl.FormInput_ShowDialog() == DialogResult.OK)
            {
                tmpRsv.Comment = FormControl.FormInput_CommentGet();
                tmpRsv.Save();

                this.RsvTableShow();
                FormControl.FormInput_CommentClear();
            }
        }

        private void RsvMenuItem2_Click(object sender, EventArgs e)
        {
            ToolStripItem tmpItem = (ToolStripItem)(sender);
            ContextMenuStrip tmpStrip = (ContextMenuStrip)(tmpItem.Owner);
            Label tmpLabel = (Label)(tmpStrip.SourceControl);
            string[] tmpTags = tmpLabel.Tag.ToString().Split(',');

            EyeOpeRsv tmpRsv = new EyeOpeRsv();
            tmpRsv.OpeKind = tmpTags[0];
            tmpRsv.OpeDate = tmpTags[1];
            tmpRsv.OpeWaku = tmpTags[2];

            tmpRsv.RsvKind = "2";

            FormControl.FormInput_ModeChange(FormInput.Mode.Comment);

            if (FormControl.FormInput_ShowDialog() == DialogResult.OK)
            {
                tmpRsv.Comment = FormControl.FormInput_CommentGet();
                tmpRsv.Save();

                this.RsvTableShow();
                FormControl.FormInput_CommentClear();
            }
        }

        private void RsvMenuItem3_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("診察・休診設定を削除しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                ToolStripItem tmpItem = (ToolStripItem)(sender);
                ContextMenuStrip tmpStrip = (ContextMenuStrip)(tmpItem.Owner);
                Label tmpLabel = (Label)(tmpStrip.SourceControl);
                string[] tmpTags = tmpLabel.Tag.ToString().Split(',');

                EyeOpeRsv.Delete(tmpTags[1], tmpTags[2], tmpTags[0]);

                this.RsvTableShow();
            }
        }

        private void RsvMenuItem4_Click(object sender, EventArgs e)
        {
            ToolStripItem tmpItem = (ToolStripItem)(sender);
            ContextMenuStrip tmpStrip = (ContextMenuStrip)(tmpItem.Owner);
            Label tmpLabel = (Label)(tmpStrip.SourceControl);
            string[] tmpTags = tmpLabel.Tag.ToString().Split(',');

            if (Dict.HolidayDict.ContainsKey(tmpTags[1]))
            {
                if (MessageBox.Show("休診日です。予約しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                {
                    return;
                }
            }

            FormControl.FormInput_ModeChange(FormInput.Mode.PatientId);

            if (FormControl.FormInput_ShowDialog() == DialogResult.OK)
            {
                FormControl.FormPat_Show_ByNewRecord(FormControl.FormInput_CommentGet(), tmpTags[0], tmpTags[1], tmpTags[2]);
                FormControl.FormInput_CommentClear();
            }
        }

        private void PrevWeekButton_Click(object sender, EventArgs e)
        {
            this.CritDateTimePicker.Value = this.CritDateTimePicker.Value.AddDays(-7);
        }

        private void NextWeekButton_Click(object sender, EventArgs e)
        {
            this.CritDateTimePicker.Value = this.CritDateTimePicker.Value.AddDays(7);
        }
    }
}