using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormList : FormBase
    {
        DataSet dSet = new DataSet();

        public FormList()
        {
            InitializeComponent();
        }

        private void FormList_Load(object sender, EventArgs e)
        {
            this.OriginalText = "患者一覧";
            this.FormTextShow();

            DeptBox1.Items.Add("");
            DeptBox2.Items.Add("");

            foreach (string k in Dict.DeptDict.Keys)
            {
                if (k.Length > 0 && !k.Equals("0"))
                {
                    DeptBox1.Items.Add(k + " " + Dict.DeptDict[k].ShortName);
                    DeptBox2.Items.Add(k + " " + Dict.DeptDict[k].ShortName);
                }
            }

            DeptBox1.Text = "7 眼科";
            DeptBox2.Text = "7 眼科";

            WardBox2.Items.Add("");

            foreach (string k in Dict.WardDict.Keys)
            {
                if (k.Length > 0 && !k.Equals("0") && !k.Equals("00"))
                {
                    WardBox2.Items.Add(k + " " + Dict.WardDict[k].Name);
                }
            }

            this.EyeDateTimePicker1.MaxDate = DateTime.Now;
            this.EyeDateTimePicker2.MaxDate = DateTime.Now.AddDays(7);

            DataTable tmpTable = dSet.Tables.Add("外来患者");
            tmpTable.Columns.Add("通番", typeof(int));
            tmpTable.Columns.Add("連番", typeof(int));
            tmpTable.Columns.Add("科番", typeof(int));
            tmpTable.Columns.Add("受付");
            tmpTable.Columns.Add("ID", typeof(int));
            tmpTable.Columns.Add("カナ");
            tmpTable.Columns.Add("氏名");
            tmpTable.Columns.Add("性別");
            tmpTable.Columns.Add("年齢", typeof(int));
            tmpTable.Columns.Add("種別");
            tmpTable.Columns.Add("医師");
            tmpTable.Columns.Add("開始");
            tmpTable.Columns.Add("中断");
            tmpTable.Columns.Add("検終");
            tmpTable.Columns.Add("終了");
            tmpTable.Columns.Add("会計");
            tmpTable.Columns.Add("病名");
            tmpTable.Columns.Add("紹介元");

            tmpTable = dSet.Tables.Add("紹介患者");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("病院");
            tmpTable.Columns.Add("科");
            tmpTable.Columns.Add("医師");

            tmpTable = dSet.Tables.Add("検査結果");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("視力１");
            tmpTable.Columns.Add("視力２");
            tmpTable.Columns.Add("眼圧１");
            tmpTable.Columns.Add("眼圧２");

            tmpTable = dSet.Tables.Add("入院患者");
            tmpTable.Columns.Add("病棟");
            tmpTable.Columns.Add("病室");
            tmpTable.Columns.Add("ID", typeof(int));
            tmpTable.Columns.Add("カナ");
            tmpTable.Columns.Add("氏名");
            tmpTable.Columns.Add("性別");
            tmpTable.Columns.Add("年齢", typeof(int));
            tmpTable.Columns.Add("診療科");
            tmpTable.Columns.Add("医師");
            tmpTable.Columns.Add("入院日");
            tmpTable.Columns.Add("退院日");
            tmpTable.Columns.Add("病名");
            tmpTable.Columns.Add("紹介元");
            tmpTable.Columns.Add("視力");
            tmpTable.Columns.Add("眼圧");

            this.Height = Screen.PrimaryScreen.WorkingArea.Height;
            this.Location = new Point(this.Location.X, 0);
        }

        public override void PatSet(PatBase p)
        {
            base.PatSet(p);

            this.stdControlPat11.PatSet(p);

            this.PtShow();
        }

        /// <summary>
        /// 入院患者の紹介元テーブルを作る。
        /// </summary>
        /// <param name="dept_id"></param>
        /// <param name="pt_list"></param>
        void IntroTableMake(string dept_id, List<string> pt_list)
        {
            if (AppString.ConcatList(pt_list, ",").Length > 0)
            {
                List<string> dept_list = new List<string>();

                if (dept_id.Length > 0)
                {
                    dept_list.Add(dept_id);
                }
                else
                {
                    dept_list.Add("7");
                }

                List<Intro> tmp_list = Intro.GetListByPatsDepts(pt_list, dept_list);

                DataTable tmpTable = dSet.Tables["紹介患者"];
                tmpTable.Clear();

                bool b = false;

                foreach (Intro tmp in tmp_list)
                {
                    // 眼科でなければ飛ばす
                    if (!tmp.DeptFromCode.Equals("7"))
                    {
                        continue;
                    }

                    // 返事でなければ飛ばす
                    if (!tmp.Kind.Equals("2") && !tmp.Kind.Equals("5") && !tmp.Kind.Equals("6"))
                    {
                        continue;
                    }

                    b = false;

                    foreach (DataRow tmpRow in tmpTable.Rows)
                    {
                        if (tmpRow["ID"].ToString().Equals(tmp.PtId))
                        {
                            b = true;
                            break;
                        }
                    }

                    if (!b)
                    {
                        DataRow r = tmpTable.NewRow();

                        r["ID"] = tmp.PtId;
                        r["病院"] = tmp.Hospital;
                        r["科"] = tmp.DeptTo;
                        r["医師"] = tmp.DoctorTo;

                        tmpTable.Rows.Add(r);
                    }
                }
            }
        }

        /// <summary>
        /// 入院患者の検査結果テーブルを作る。
        /// </summary>
        /// <param name="pt_list"></param>
        void KensaTableMake(List<string> pt_list)
        {
            if (AppString.ConcatList(pt_list, ",").Length == 0)
            {
                return;
            }

            List<string> kensa_id_list = new List<string>();
            kensa_id_list.Add("1");
            kensa_id_list.Add("3");

            List<EyeKensa> tmp_list = EyeKensa.LoadByPatsKensas(pt_list, kensa_id_list);

            DataTable tmpTable = dSet.Tables["検査結果"];
            tmpTable.Clear();

            bool b = false;
            DataRow r = null;

            foreach (EyeKensa tmp in tmp_list)
            {
                b = false;

                foreach (DataRow tmpRow in tmpTable.Rows)
                {
                    if (tmpRow["ID"].ToString().Equals(tmp.PtId))
                    {
                        r = tmpRow;
                        b = true;
                        break;
                    }
                }

                if (!b)
                {
                    r = tmpTable.NewRow();
                    tmpTable.Rows.Add(r);
                }

                // すでにデータが入っていれば飛ばす
                if (r != null &&
                    r["視力１"].ToString().Length > 0 && r["視力２"].ToString().Length > 0 &&
                    r["眼圧１"].ToString().Length > 0 && r["眼圧２"].ToString().Length > 0)
                {
                    continue;
                }

                r["ID"] = tmp.PtId;

                if (tmp.KensaId.Equals("1"))
                {
                    if (r["視力１"].ToString().Length > 0)
                    {
                        r["視力２"] = DateTimeAgent.DateFormat(tmp.KensaDate, DateTimeAgent.DateFormatKind.SHORT) + "\r\n" + EyeDict.GetSightString(tmp.Cont);
                    }
                    else
                    {
                        r["視力１"] = DateTimeAgent.DateFormat(tmp.KensaDate, DateTimeAgent.DateFormatKind.SHORT) + "\r\n" + EyeDict.GetSightString(tmp.Cont);
                    }
                }
                else if (tmp.KensaId.Equals("3"))
                {
                    if (r["眼圧１"].ToString().Length > 0)
                    {
                        r["眼圧２"] = DateTimeAgent.DateFormat(tmp.KensaDate, DateTimeAgent.DateFormatKind.SHORT) + "\r\n" + EyeDict.GetTensionString(tmp.Cont);
                    }
                    else
                    {
                        r["眼圧１"] = DateTimeAgent.DateFormat(tmp.KensaDate, DateTimeAgent.DateFormatKind.SHORT) + "\r\n" + EyeDict.GetTensionString(tmp.Cont);
                    }
                }
            }
        }

        public void EyeListViewShow1()
        {
            // スクロール位置を取得
            int y = this.EyeListView1.FirstDisplayedScrollingRowIndex;

            DataTable tmpTable = dSet.Tables["外来患者"];
            tmpTable.Clear();

            if (DeptBox1.Text.Length == 0 || !DeptBox1.Text.Contains(" "))
            {
                DeptBox1.Text = "7 眼科";
            }

            string dept = this.DeptBox1.Text.Split(' ')[0];

            List<PatOut> tmpList = PatOut.GetList(this.EyeDateTimePicker1.Value.ToString("yyyyMMdd"), dept, "");

            List<string> pt_list = new List<string>();

            foreach (PatOut tmpPat in tmpList)
            {
                DataRow r = tmpTable.NewRow();

                r["通番"] = tmpPat.Seq1;
                r["連番"] = tmpPat.Seq2;
                r["科番"] = tmpPat.Seq3;

                if (tmpPat.Time1.Length > 0 && !tmpPat.Time1.Equals("0"))
                {
                    r["受付"] = tmpPat.TimeString1;
                }

                r["ID"] = tmpPat.Id;
                r["カナ"] = tmpPat.Kana;
                r["氏名"] = tmpPat.Name;
                r["性別"] = tmpPat.SexNameShort;
                r["年齢"] = tmpPat.Age;
                r["種別"] = tmpPat.KindName;
                r["医師"] = tmpPat.DoctorName;

                if (tmpPat.Time2.Length > 0 && !tmpPat.Time2.Equals("0"))
                {
                    r["開始"] = tmpPat.TimeString2;
                }

                if (tmpPat.Time3.Length > 0 && !tmpPat.Time3.Equals("0"))
                {
                    r["中断"] = tmpPat.TimeString3;
                }

                if (tmpPat.Time4.Length > 0 && !tmpPat.Time4.Equals("0"))
                {
                    r["終了"] = tmpPat.TimeString4;
                }

                if (tmpPat.Time5.Length > 0 && !tmpPat.Time5.Equals("0"))
                {
                    r["会計"] = tmpPat.TimeString5;
                }

                tmpTable.Rows.Add(r);

                if (!pt_list.Contains(tmpPat.Id))
                {
                    pt_list.Add(tmpPat.Id);
                }
            }

            // サマリー病名と紹介元を追加
            if (this.DeptBox1.Text.Contains(" ") && pt_list.Count > 0)
            {
                List<EyeSummary> list = EyeSummary.GetListByPats(pt_list);

                foreach (DataRow r in tmpTable.Rows)
                {
                    foreach (EyeSummary sum in list)
                    {
                        if (r["ID"].ToString().Equals(sum.PtId))
                        {
                            r["病名"] = sum.Diag;
                            break;
                        }
                    }
                }

                this.IntroTableMake(this.DeptBox1.Text.Split(' ')[0], pt_list);

                foreach (DataRow r in tmpTable.Rows)
                {
                    DataRow[] rs1 = dSet.Tables["紹介患者"].Select("ID = '" + r["ID"] + "'");

                    if (rs1.Length > 0)
                    {
                        r["紹介元"] = rs1[0]["病院"].ToString() + " " + rs1[0]["科"].ToString() + " " + rs1[0]["医師"].ToString();
                    }
                }
            }

            this.EyeListViewFilter1();

            // スクロール位置の調整
            if (y >= 0 && y < this.EyeListView1.RowCount)
            {
                this.EyeListView1.FirstDisplayedScrollingRowIndex = y;
            }
        }

        private void EyeListViewFilter1()
        {
            DataView tmpView = new DataView(dSet.Tables["外来患者"]);

            List<string> filters = new List<string>();

            if (FilterBox1.Text.Length > 0)
            {
                filters.Add("(氏名 like '%" + this.FilterBox1.Text + "%' or カナ like '%" + this.FilterBox1.Text + "%')");
            }

            if (!ShowEndBox1.Checked)
            {
                filters.Add("(終了 is null or 終了 = '')");
            }

            if (filters.Count > 0)
            {
                tmpView.RowFilter = AppString.ConcatList(filters, " and ");
            }

            /* DataGridView コントロール上でソート順が指定されていればそれを保持する */
            int tmpColumn = 2;              // 現在ソート対象になっているカラムの番号。デフォルトは 2（科番）。
            ListSortDirection tmpDirection = ListSortDirection.Ascending;       // ソートの方向。

            if (EyeListView1.SortOrder == SortOrder.Ascending)
            {
                tmpColumn = EyeListView1.SortedColumn.Index;
                tmpDirection = ListSortDirection.Ascending;
            }
            else if (EyeListView1.SortOrder == SortOrder.Descending)
            {
                tmpColumn = EyeListView1.SortedColumn.Index;
                tmpDirection = ListSortDirection.Descending;
            }

            EyeListView1.DataSource = tmpView;

            EyeListView1.Columns[0].HeaderText = "通番";
            EyeListView1.Columns[0].Width = 40;
            EyeListView1.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[1].HeaderText = "連番";
            EyeListView1.Columns[1].Width = 40;
            EyeListView1.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            EyeListView1.Columns[1].Visible = false;

            EyeListView1.Columns[2].HeaderText = "科番";
            EyeListView1.Columns[2].Width = 40;
            EyeListView1.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[3].HeaderText = "受付";
            EyeListView1.Columns[3].Width = 40;
            EyeListView1.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[4].HeaderText = "ID";
            EyeListView1.Columns[4].Width = 55;
            EyeListView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            EyeListView1.Columns[5].Width = 80;

            EyeListView1.Columns[6].Width = 90;

            EyeListView1.Columns[7].HeaderText = "性別";
            EyeListView1.Columns[7].Width = 35;
            EyeListView1.Columns[7].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[8].HeaderText = "年齢";
            EyeListView1.Columns[8].Width = 35;
            EyeListView1.Columns[8].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[9].HeaderText = "種別";
            EyeListView1.Columns[9].Width = 55;
            EyeListView1.Columns[9].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[10].HeaderText = "医師";
            EyeListView1.Columns[10].Width = 80;
            EyeListView1.Columns[10].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[11].HeaderText = "開始";
            EyeListView1.Columns[11].Width = 40;
            EyeListView1.Columns[11].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[12].HeaderText = "中断";
            EyeListView1.Columns[12].Width = 40;
            EyeListView1.Columns[12].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 使われていないので非表示にする 2018/08/31
            EyeListView1.Columns[13].HeaderText = "検終";
            EyeListView1.Columns[13].Width = 40;
            EyeListView1.Columns[13].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            EyeListView1.Columns[13].Visible = false;

            EyeListView1.Columns[14].HeaderText = "終了";
            EyeListView1.Columns[14].Width = 40;
            EyeListView1.Columns[14].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView1.Columns[15].HeaderText = "会計";
            EyeListView1.Columns[15].Width = 40;
            EyeListView1.Columns[15].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 検査終了カラムを非表示にしたため幅を増やした 2018/08/31
            EyeListView1.Columns[16].HeaderText = "主病名";
//            EyeListView1.Columns[16].Width = 80;
            EyeListView1.Columns[16].Width = 100;
            EyeListView1.Columns[16].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // 検査終了カラムを非表示にしたため幅を増やした 2018/08/31
            EyeListView1.Columns[17].HeaderText = "紹介元";
//            EyeListView1.Columns[17].Width = 140;
            EyeListView1.Columns[17].Width = 160;
            EyeListView1.Columns[17].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            /* 前に表示されていた画面がソートされていれば、ソートフラグが立っているのでソートする。
             * 下のグリッドの色変更処理より後に行うと、ソートによって色が戻るため、先にソートする必要がある。 */
            EyeListView1.Sort(EyeListView1.Columns[tmpColumn], tmpDirection);

            /* グリッドの色変更を行う。ソートの後に行う必要がある */
            for (int i = 0; i < EyeListView1.Rows.Count; i++)
            {
                if (EyeListView1.Rows[i].Cells[14].Value.ToString().Length > 0)
                {
                    EyeListView1.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                }
                else if (EyeListView1.Rows[i].Cells[12].Value.ToString().Length > 0)
                {
                    EyeListView1.Rows[i].DefaultCellStyle.BackColor = Color.LightYellow;
                }
                else if (EyeListView1.Rows[i].Cells[11].Value.ToString().Length > 0)
                {
                    EyeListView1.Rows[i].DefaultCellStyle.BackColor = Color.LightYellow;
                }

                if (EyeListView1.Rows[i].Cells[7].Value.ToString() == "女")
                {
                    EyeListView1.Rows[i].Cells[4].Style.ForeColor = Color.Red;
                    EyeListView1.Rows[i].Cells[5].Style.ForeColor = Color.Red;
                    EyeListView1.Rows[i].Cells[6].Style.ForeColor = Color.Red;
                    EyeListView1.Rows[i].Cells[7].Style.ForeColor = Color.Red;
                    EyeListView1.Rows[i].Cells[8].Style.ForeColor = Color.Red;
                }
            }
        }

        private void FilterBox1_TextChanged(object sender, EventArgs e)
        {
            this.EyeListViewFilter1();
        }

        private void EyeListView1_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            this.EyeListViewFilter1();
        }

        /// <summary>
        /// 手術予約メニュー実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolOpeRsvMenuItem_Click(object sender, EventArgs e)
        {
            if (EyeListView1.SelectedRows.Count > 0)
            {
                FormControl.FormOpeRsv_Show(EyeListView1.SelectedRows[0].Cells["ID"].Value.ToString());
            }
            else
            {
                FormControl.FormOpeRsv_Show();
            }
        }

        /// <summary>
        /// 手術台帳メニュー実行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolPatMenuItem_Click(object sender, EventArgs e)
        {
            FormControl.FormPat_Show();
        }

        private void EyeListShowButton1_Click(object sender, EventArgs e)
        {
            this.EyeListViewShow1();
        }

        private void ShowEndBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.EyeListViewFilter1();
        }

        private void LoginChangeButton_Click(object sender, EventArgs e)
        {
            LoginChange lc = new LoginChange();
            lc.ShowDialog();
            this.FormTextShow();
        }

        private void EyeListView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FormControl.FormPat_Show(EyeListView1.Rows[e.RowIndex].Cells["ID"].Value.ToString(), FormPat.Mode.NEW);
        }

        private void ToolOpeFindMenuItem_Click(object sender, EventArgs e)
        {
            FormControl.FormFindOpeRecord_Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Minimized)
            {
                this.EyeListViewShow1();
            }
        }

        private void FileCloseMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void FileExitMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("終了しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void EyeListShowButton2_Click(object sender, EventArgs e)
        {
            this.EyeListViewShow2();
        }

        void EyeListViewShow2()
        {
            // スクロール位置を取得
            int y = this.EyeListView2.FirstDisplayedScrollingRowIndex;

            DataTable tmpTable = dSet.Tables["入院患者"];
            tmpTable.Clear();

            string ward = "";

            if (WardBox2.Text.Contains(" "))
            {
                ward = WardBox2.Text.Split(' ')[0];
            }

            if (DeptBox2.Text.Length == 0 || !DeptBox2.Text.Contains(" "))
            {
                DeptBox2.Text = "7 眼科";
            }

            string dept = this.DeptBox2.Text.Split(' ')[0];

            List<PatIn> tmpList = PatIn.GetListByDate(EyeDateTimePicker2.Value.ToString("yyyyMMdd"), ward, dept);

            List<string> pt_list = new List<string>();

            foreach (PatIn tmpPat in tmpList)
            {
                DataRow r = tmpTable.NewRow();

                if (Dict.WardDict.ContainsKey(tmpPat.Ward))
                {
                    r["病棟"] = Dict.WardDict[tmpPat.Ward].Name;
                }

                r["病室"] = tmpPat.Room;

                r["ID"] = tmpPat.Id;
                r["カナ"] = tmpPat.Kana;
                r["氏名"] = tmpPat.Kana + Environment.NewLine + tmpPat.Name;
                r["性別"] = tmpPat.SexNameShort;
                r["年齢"] = tmpPat.Age;
                r["診療科"] = tmpPat.DeptName;
                r["医師"] = tmpPat.DoctorName;
                r["入院日"] = tmpPat.InDateStringShort + (tmpPat.Status == PatInStatus.Yet ? Environment.NewLine + "(予)" : "");
                r["退院日"] = tmpPat.OutDateStringShort;

                tmpTable.Rows.Add(r);

                if (!pt_list.Contains(tmpPat.Id))
                {
                    pt_list.Add(tmpPat.Id);
                }
            }

            // サマリー病名・検査・紹介元データを追加する
            if (this.DeptBox2.Text.Contains(" ") && pt_list.Count > 0)
            {
                List<EyeSummary> list = EyeSummary.GetListByPats(pt_list);

                foreach (DataRow r in tmpTable.Rows)
                {
                    foreach (EyeSummary sum in list)
                    {
                        if (r["ID"].ToString().Equals(sum.PtId))
                        {
                            r["病名"] = sum.Diag;
                            break;
                        }
                    }
                }

                this.IntroTableMake(this.DeptBox2.Text.Split(' ')[0], pt_list);
                this.KensaTableMake(pt_list);

                foreach (DataRow r in tmpTable.Rows)
                {
                    DataRow[] rs1 = dSet.Tables["紹介患者"].Select("ID = '" + r["ID"] + "'");

                    if (rs1.Length > 0)
                    {
                        r["紹介元"] = rs1[0]["病院"].ToString() + " " + rs1[0]["科"].ToString() + " " + rs1[0]["医師"].ToString();
                    }

                    DataRow[] rs2 = dSet.Tables["検査結果"].Select("ID = '" + r["ID"] + "'");

                    if (rs2.Length > 0)
                    {
                        r["視力"] = rs2[0]["視力１"].ToString() + "\r\n\r\n" + rs2[0]["視力２"].ToString();
                        r["眼圧"] = rs2[0]["眼圧１"].ToString() + "\r\n\r\n" + rs2[0]["眼圧２"].ToString();
                    }
                }
            }

            this.EyeListViewFilter2();

            // スクロール位置の調整
            if (y >= 0 && y < this.EyeListView2.RowCount)
            {
                this.EyeListView2.FirstDisplayedScrollingRowIndex = y;
            }
        }

        void EyeListViewFilter2()
        {
            DataView tmpView = new DataView(dSet.Tables["入院患者"]);

            List<string> filters = new List<string>();

            if (FilterBox2.Text.Length > 0)
            {
                filters.Add("(氏名 like '%" + this.FilterBox2.Text + "%' or カナ like '%" + this.FilterBox2.Text + "%')");
            }

            if (WardBox2.Text.Contains(" "))
            {
                filters.Add("(病棟 = '" + WardBox2.Text.Split(' ')[1] + "')");
            }

            if (DeptBox2.Text.Contains(" "))
            {
                filters.Add("(診療科 = '" + DeptBox2.Text.Split(' ')[1] + "')");
            }

            if (filters.Count > 0)
            {
                tmpView.RowFilter = AppString.ConcatList(filters, " and ");
            }

            EyeListView2.DataSource = tmpView;

            EyeListView2.Columns["病棟"].Width = 45;
            EyeListView2.Columns["病棟"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["病室"].Width = 35;
            EyeListView2.Columns["病室"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["ID"].Width = 50;
            EyeListView2.Columns["ID"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            EyeListView2.Columns["カナ"].Visible = false;
            EyeListView2.Columns["カナ"].Width = 50;
            EyeListView2.Columns["カナ"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            EyeListView2.Columns["氏名"].Width = 80;
            EyeListView2.Columns["氏名"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            EyeListView2.Columns["性別"].Width = 30;
            EyeListView2.Columns["性別"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["年齢"].Width = 30;
            EyeListView2.Columns["年齢"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["診療科"].Visible = false;
            EyeListView2.Columns["診療科"].Width = 70;
            EyeListView2.Columns["診療科"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["医師"].Width = 45;
            EyeListView2.Columns["医師"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["入院日"].Width = 60;
            EyeListView2.Columns["入院日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["退院日"].Width = 60;
            EyeListView2.Columns["退院日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            EyeListView2.Columns["病名"].HeaderText = "主病名";
            EyeListView2.Columns["病名"].Width = 70;
            EyeListView2.Columns["病名"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            EyeListView2.Columns["紹介元"].Width = 85;
            EyeListView2.Columns["紹介元"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            EyeListView2.Columns["視力"].Width = 220;
            EyeListView2.Columns["視力"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            EyeListView2.Columns["眼圧"].Width = 160;
            EyeListView2.Columns["眼圧"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            for (int i = 0; i < EyeListView2.Rows.Count; i++)
            {
                if (EyeListView2.Rows[i].Cells[5].Value.ToString() == "女")
                {
                    EyeListView2.Rows[i].Cells[2].Style.ForeColor = Color.Red;
                    EyeListView2.Rows[i].Cells[3].Style.ForeColor = Color.Red;
                    EyeListView2.Rows[i].Cells[4].Style.ForeColor = Color.Red;
                    EyeListView2.Rows[i].Cells[5].Style.ForeColor = Color.Red;
                    EyeListView2.Rows[i].Cells[6].Style.ForeColor = Color.Red;
                }
            }
        }

        private void WardBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.EyeListView2.Columns.Count > 0)
            {
                this.EyeListViewFilter2();
            }
        }

        private void DeptBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.EyeListView2.Columns.Count > 0)
            {
                this.EyeListViewFilter2();
            }
        }

        private void FilterBox2_TextChanged(object sender, EventArgs e)
        {
            if (this.EyeListView2.Columns.Count > 0)
            {
                this.EyeListViewFilter2();
            }
        }

        private void EyeListView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            FormControl.FormPat_Show(EyeListView2.Rows[e.RowIndex].Cells["ID"].Value.ToString(), FormPat.Mode.NEW);
        }

        void PtShow()
        {
            if (this.Pat.Id.Length == 0)
            {
                return;
            }

            if (MessageBox.Show("この方の画面を開きますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                FormControl.FormPat_Show(this.Pat.Id, FormPat.Mode.NEW);
            }
        }

        private void WorksheetPrintMenuItem_Click(object sender, EventArgs e)
        {
            if (EyeListView2.SelectedRows.Count > 0)
            {
                DataGridViewRow r = EyeListView2.SelectedRows[0];
                FormControl.FormPrint_WorksheetPrint(r.Cells["病室"].Value.ToString(), r.Cells["ID"].Value.ToString(), r.Cells["氏名"].Value.ToString(), r.Cells["カナ"].Value.ToString());
            }
        }

        private void EyeListTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 入院患者一覧が開かれた時に、空の場合は表示する
            if (EyeListTabControl.SelectedIndex == 1 && EyeListView2.Rows.Count == 0)
            {
                this.EyeListViewShow2();
            }
        }
    }
}