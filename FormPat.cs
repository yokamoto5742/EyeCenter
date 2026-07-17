using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormPat : StdForm1
    {
        ControlSumPage _SumPage = new ControlSumPage();
        ControlIVPage _IVPage = new ControlIVPage();

        // 手術歴・検査歴の横幅（設定ファイル EyeCenter.exe.config で変更可能。検査歴の 0 は画面右端まで自動調整）
        readonly int _OpeHistoryWidth = AppConfig.GetInt("OpeHistoryViewWidth", 275);
        readonly int _KensaHistoryWidth = AppConfig.GetInt("KensaHistoryViewWidth", 0);

        string _KensaDate = "";

        bool KensaEdited
        {
            get
            {
                bool b = false;

                if (this.KensaTab.Controls.ContainsKey("KensaPanel") && ((KensaPanel)(this.KensaTab.Controls["KensaPanel"])).Edited)
                {
                    b = true;
                }

                return b;
            }
        }

        DataSet dSet = new DataSet();

        Dictionary<string, string> outcomeDict = new Dictionary<string,string>();

        /// <summary>
        /// 経過記録のテキスト（術前, 6M 等）とコントロール名（Pre, M6 等）の対応
        /// </summary>
        Dictionary<string, string> passDict = new Dictionary<string, string>();

        /// <summary>
        /// ポップアップヘルプを表示するツールチップ。
        /// </summary>
        ToolTip passTip;

        /// <summary>
        /// 画面編集モード
        /// </summary>
        public enum Mode : int
        {
            NEW = 0,
            SHOW = 1
        }

        /// <summary>
        /// タブ種別
        /// </summary>
        public enum Tab : int
        {
            SUMMARY = 0,
            OPE = 1,
            KENSA = 2
        }

        public FormPat()
        {
            InitializeComponent();

            this.DSetInit();
            this.OpeInit();
        }

        public override void PatSet(PatBase p)
        {
            base.PatSet(p);

            this.stdControlPat11.PatSet(p);

            this.PtShow();
        }

        /// <summary>
        /// 該当患者の記録を開く場合。
        /// </summary>
        /// <param name="pt_id"></param>
        /// <param name="mode"></param>
        public void ShowByPat(string pt_id, Mode mode)
        {
            this.Show();

            if (!this.Pat.Id.Equals(pt_id))
            {
                this.PatSet(PatBase.Load(pt_id));
            }
        }

        /// <summary>
        /// 既存記録を開く場合。
        /// </summary>
        /// <param name="record_id"></param>
        public void ShowByRecord(string record_id)
        {
            this.Show();

            EyeOpe tmpOpe = EyeOpe.Load(record_id);

            if (!this.Pat.Id.Equals(tmpOpe.PtId))
            {
                this.PatSet(PatBase.Load(tmpOpe.PtId));
            }

            // record_id が示す行を選択する
            foreach (DataGridViewRow r in OpeHistoryView.Rows)
            {
                if (r.Cells["ID"].Value.ToString().Equals(record_id))
                {
                    r.Selected = true;
                    break;
                }
            }

            this.OpeShow(tmpOpe);
        }

        /// <summary>
        /// 予約を新規入力（手術記録を新規作成）する場合。
        /// </summary>
        /// <param name="ope_kind"></param>
        /// <param name="ope_date"></param>
        /// <param name="ope_time"></param>
        public void ShowByNewRecord(string pt_id, string ope_kind, string ope_date, string ope_time)
        {
            this.Show();

            int i = 0;

            if (pt_id.Length > 0 && int.TryParse(pt_id, out i))
            {
                if (!this.Pat.Id.Equals(pt_id))
                {
                    this.PatSet(PatBase.Load(pt_id));
                }
            }
            else
            {
                return;
            }

            if (EyeDict.OpeKindDict.ContainsKey(ope_kind))
            {
                OpeKindBox.Text = ope_kind + " " + EyeDict.OpeKindDict[ope_kind];
            }

            if (ope_date.Length == 8)
            {
                OpeDateTimePicker.Value = DateTime.Parse(ope_date.Insert(4, "/").Insert(7, "/"));
            }

            if (ope_time.Contains("-"))
            {
                OpeTimeBox.Text = ope_time.Split('-')[0];
            }
            else
            {
                OpeTimeBox.Text = ope_time;
            }
        }

        private void FormPat_Load(object sender, EventArgs e)
        {
            // 問診入力・問診履歴の横幅を設定ファイル(EyeCenter.exe.config)から反映する
            int ivContWidth = AppConfig.GetInt("IVContBoxWidth", this.IVContBox.Width);
            int ivHistWidth = AppConfig.GetInt("IVHistoryViewWidth", this.IVHistoryView.Width);
            this.IVContBox.Width = ivContWidth;
            this.label53.Left = this.IVContBox.Left + ivContWidth + 8;
            this.IVHistoryView.Left = this.IVContBox.Left + ivContWidth + 8;
            this.IVHistoryView.Width = ivHistWidth;
            this.IVContBox.MaxLength = 1999;

            // 手術歴・検査歴の横幅設定を初期表示に反映する
            this.PtHistoryWide();

            if (!InnoProgram.Exists)
            {
                this.InnoButton.Enabled = false;
            }

            outcomeDict.Add("0", "");
            outcomeDict.Add("1", "治癒");
            outcomeDict.Add("2", "死亡");
            outcomeDict.Add("3", "中止");
            outcomeDict.Add("4", "転医");
            outcomeDict.Add("5", "軽快");
            outcomeDict.Add("6", "転院");
            outcomeDict.Add("7", "一時退院");
            outcomeDict.Add("8", "不変");

            passTip = new ToolTip();
            passTip.ShowAlways = true;

            this.RecordTabControlInit();
            this.PassPanelControlInit();
            this.KensaTabInit();

            MainTabControl.SelectedIndex = 0;
            SumTabControl.SelectedIndex = 2;

            this._SumPage.Init(this, this._IVPage);
            this._IVPage.Init(this);

            this.OrgSize();
        }

        /// <summary>
        /// 元のサイズ（横1280 or 1024, 縦最大）に戻す。
        /// </summary>
        public void OrgSize()
        {
            this.Height = Screen.PrimaryScreen.WorkingArea.Height;

            if (Screen.PrimaryScreen.WorkingArea.Width >= 1280)
            {
                this.Width = 1280;
                this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - 1280) / 2, 0);
            }
            else
            {
                this.Width = 1024;
                this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - 1024) / 2, 0);
            }
        }

        private void FormPat_Shown(object sender, EventArgs e)
        {
            this.stdControlPat11.Focus();
        }

        private void ReloadButton_Click(object sender, EventArgs e)
        {
            EyeDict.Init();

            this.RecordTabControlInit();
            this.PassPanelControlInit();
            this.KensaTabInit();

            this.AllOpeClear();
        }

        /// <summary>
        /// dSet 初期化。手術履歴テーブルのカラムを作成する。
        /// コンストラクタ以外では使用しない。
        /// </summary>
        private void DSetInit()
        {
            DataTable tmpTable = dSet.Tables.Add("手術履歴");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("OPE_DATE");
            tmpTable.Columns.Add("手術日");
            tmpTable.Columns.Add("OPE_TIME");
            tmpTable.Columns.Add("時刻");
            tmpTable.Columns.Add("OPE_KIND");
            tmpTable.Columns.Add("種別");
            tmpTable.Columns.Add("手術室");
            tmpTable.Columns.Add("手術");
            tmpTable.Columns.Add("医師");
            tmpTable.Columns.Add("麻酔");
            tmpTable.Columns.Add("病名");
            tmpTable.Columns.Add("入外");
            tmpTable.Columns.Add("年齢");
            tmpTable.Columns.Add("眼");
            tmpTable.Columns.Add("感染");
            tmpTable.Columns.Add("禁忌");
            tmpTable.Columns.Add("備考");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
            {
                tmpTable.Columns.Add(r["Text"].ToString());
            }

            tmpTable = dSet.Tables.Add("検査歴");
            tmpTable.Columns.Add("KENSA_DATE");
            tmpTable.Columns.Add("検査日");

            foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["KensaPage"].Rows)
            {
                tmpTable.Columns.Add(tmpRow["ID"].ToString());
            }

            tmpTable.Columns.Add("Sight_R");
            tmpTable.Columns.Add("Sight_L");

            tmpTable.Columns.Add("CorrectSight_R");
            tmpTable.Columns.Add("CorrectSight_L");

            tmpTable.Columns.Add("KyumenDosu_R");
            tmpTable.Columns.Add("KyumenDosu_L");

            tmpTable.Columns.Add("EnchuDosu_R");
            tmpTable.Columns.Add("EnchuDosu_L");

            tmpTable.Columns.Add("TensionAvg_R");
            tmpTable.Columns.Add("TensionAvg_L");

            tmpTable.Columns.Add("Tension_R");
            tmpTable.Columns.Add("Tension_L");

            tmpTable.Columns.Add("GAT_R");
            tmpTable.Columns.Add("GAT_L");

            tmpTable.Columns.Add("ContrastA_R");
            tmpTable.Columns.Add("ContrastB_R");
            tmpTable.Columns.Add("ContrastC_R");
            tmpTable.Columns.Add("ContrastD_R");
            tmpTable.Columns.Add("ContrastE_R");

            tmpTable.Columns.Add("ContrastA_L");
            tmpTable.Columns.Add("ContrastB_L");
            tmpTable.Columns.Add("ContrastC_L");
            tmpTable.Columns.Add("ContrastD_L");
            tmpTable.Columns.Add("ContrastE_L");

            tmpTable.Columns.Add("Menshihou_R");
            tmpTable.Columns.Add("Menshihou_L");

            tmpTable.Columns.Add("MChartTate_R");
            tmpTable.Columns.Add("MChartYoko_R");

            tmpTable.Columns.Add("MChartTate_L");
            tmpTable.Columns.Add("MChartYoko_L");

            // 網膜厚
            tmpTable.Columns.Add("Moumakukou_R");
            tmpTable.Columns.Add("Moumakukou_L");
        }

        /// <summary>
        /// タブ切り替え
        /// </summary>
        /// <param name="t"></param>
        private void TabChange(Tab t)
        {
            MainTabControl.SelectTab((int)t);
        }

        private void KensaTabInit()
        {
            this._KensaDate = DateTime.Now.ToString("yyyyMMdd");
            this.KensaDate.Value = DateTime.Now;

            if (KensaTab.Controls.ContainsKey("KensaPanel"))
            {
                KensaTab.Controls.RemoveByKey("KensaPanel");
            }

            KensaTab.Controls.Add(new KensaPanel());
        }

        /// <summary>
        /// 手術基本情報を初期化。
        /// </summary>
        private void OpeInit()
        {
            OpeKindBox.Items.Clear();
            OpeKindBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeKind"].Rows)
            {
                OpeKindBox.Items.Add(r["ID"] + " " + r["Name"]);
            }

            OpeTimeBoxChange();

            FillCombo(OpeRoomBox, "OpeRoom");
            FillCombo(OpeNameBox, "OpeName");
            FillCombo(DoctorBox, "Doctor");
            FillCombo(PlanTimeBox, "PlanTime");
            FillCombo(AnesBox, "Anes");
            FillCombo(DiagBox, "Diag");
            FillCombo(InOutBox, "InOut");

            InRoomBoxChange();
            InDateChange();

            FillCombo(InTimeBox, "InTime");
            FillCombo(InTermBox, "InTerm");
            FillCombo(PostDealBox, "PostDeal");
        }

        /// <summary>
        /// 辞書テーブルの Value 列でコンボボックスの項目を作り直す（先頭は空欄）。
        /// </summary>
        void FillCombo(ComboBox box, string tableName)
        {
            box.Items.Clear();
            box.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables[tableName].Rows)
            {
                box.Items.Add(r["Value"].ToString());
            }
        }

        /// <summary>
        /// 手術記録全体をクリア。
        /// </summary>
        private void AllOpeClear()
        {
            this.OpeClear();
            this.RecordClear();
            this.DoctorClear();
            this.PassClear();
        }

        /// <summary>
        /// 手術基本情報のすべてをクリア。
        /// </summary>
        private void OpeClear()
        {
            OpeDateTimePicker.Value = DateTime.Now;
            OpeKindBox.Text = "";
            OpeTimeBox.Text = "";

            this.OpeClear_Wo_KindDateTime();
        }

        /// <summary>
        /// 手術基本情報の内容をクリア。
        /// ただし種別・時刻はクリアしない。
        /// </summary>
        private void OpeClear_Wo_KindDateTime()
        {
            OpeIdBox.Clear();

            OpeRoomBox.Text = "";
            OpeNameBox.Text = "";
            DoctorBox.Text = "";
            PlanTimeBox.Text = "";
            AnesBox.Text = "";
            DiagBox.Text = "";
            InOutBox.Text = "";
            InTimeBox.Text = "";
            InTermBox.Text = "";
            EyeBoxR.Checked = false;
            EyeBoxL.Checked = false;
            HeightBox.Text = "";
            WeightBox.Text = "";
            SurfaceBox.Text = "";
            VisdineBox.Text = "";
            GrapeBox.Text = "";
            DmBox.Text = "";
            InfectionBox.Text = "";
            PostDealBox.Text = "";
            PastBox.Clear();
            CommentBox.Clear();
            AllCheckBox.Checked = false;
            ExplainBox.Checked = false;
            EyeDropBox.Checked = false;
            AgreeBox.Checked = false;
            PreCheckBox.Checked = false;
            EarlierOKBox.Checked = false;
            OpeStaffLabel.Text = "";

            this.OpeModeChange(Mode.NEW);
        }

        /// <summary>
        /// 手術記録タブの内容クリア
        /// </summary>
        private void RecordClear()
        {
            foreach (TabPage p in RecordTabControl.TabPages)
            {
                foreach (Control c in p.Controls)
                {
                    if (c is TextBox || c is ComboBox)
                    {
                        c.Text = "";
                    }
                }
            }

            this.RecordStatusBox.Checked = false;
            this.RecordStaffLabel.Text = "";
        }

        /// <summary>
        /// 医師記録の内容クリア
        /// </summary>
        private void DoctorClear()
        {
            PreContBox.Clear();
            DoContBox.Clear();
            DoctorStatusBox.Checked = false;
            DoctorStaffLabel.Text = "";
        }

        /// <summary>
        /// 経過記録パネルの内容クリア
        /// </summary>
        private void PassClear()
        {
            foreach (Control c in PassPanel.Controls)
            {
                if (c is TextBox || c is ComboBox)
                {
                    c.Text = "";
                }
            }

            this.PassStaffLabel.Text = "";
        }

        private void OpeKindBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.OpeTimeBoxChange();
        }

        private void OpeDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            this.OpeTimeBoxChange();
        }

        /// <summary>
        /// 手術日と種別に応じて時間枠コンボボックスを変更する。
        /// 最初から入っている値は消去しない。
        /// </summary>
        private void OpeTimeBoxChange()
        {
            OpeTimeBox.Items.Clear();
            OpeTimeBox.Items.Add("");

            if (OpeKindBox.Text.Contains(" "))
            {
                string[] wakus = EyeDict.GetWakus(OpeKindBox.Text.Split(' ')[0], OpeDateTimePicker.Value.ToString("yyyyMMdd"));
                string[] wakuNums = EyeDict.GetWakuNums(OpeKindBox.Text.Split(' ')[0], OpeDateTimePicker.Value.ToString("yyyyMMdd"));

                for (int i = 0; i < wakus.Length; i++)
                {
                    if (i < wakuNums.Length && !wakuNums[i].Equals("0"))
                    {
                        OpeTimeBox.Items.Add(wakus[i].Split('-')[0].PadLeft(4, '0'));
                    }
                }
            }

            // 経過記録パネルの日付を変更する。
            this.PassPanelControlDateChange();
        }

        private void InOutBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.InRoomBoxChange();
            this.InDateChange();
        }

        /// <summary>
        /// 入外の変更に応じて入院日コンボボックスを変更する。
        /// </summary>
        private void InDateChange()
        {
            InDateTimePicker.Enabled = InOutBox.Text.Equals("わかば") || InOutBox.Text.Equals("さくら") || InOutBox.Text.Equals("あやめ");
        }

        /// <summary>
        /// 入外の変更に応じて病室コンボボックスを変更する。
        /// </summary>
        private void InRoomBoxChange()
        {
            InRoomBox.Text = "";
            InRoomBox.Items.Clear();
            InRoomBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["InRoom"].Select("InOut = '" + InOutBox.Text + "'"))
            {
                InRoomBox.Items.Add(r["Value"].ToString());
            }
        }

        private void PtIdBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.PtShow();
            }
            else if (e.KeyCode == Keys.F3)
            {
                this.PatSet(FormFindPat.FindPat());
            }
        }

        /// <summary>
        /// 患者情報（ID, 氏名, 手術履歴）を表示する
        /// </summary>
        private void PtShow()
        {
            if (this.Pat.Id.Length == 0)
            {
                return;
            }

            this.Text = this.Pat.Name;

            // 伝達情報が存在すれば「伝達」ボタンが赤くなる。それ以外は黄色。
            Memo tmpInfo = Memo.Load(this.Pat.Id, "0");

            if (tmpInfo.Cont.Length > 0)
            {
                this.InfoShareButton.BackColor = Color.FromArgb(255, 192, 192);
            }
            else
            {
                this.InfoShareButton.BackColor = Color.FromArgb(255, 255, 192);
            }

            // 家族連絡先情報が存在すれば「家族」ボタンが赤くなる。それ以外は黄色。
            List<PatContact> tmpList = PatContact.GetList(this.Pat.Id);

            if (tmpList.Count > 0)
            {
                string con = "";

                foreach (PatContact c in tmpList)
                {
                    if (con.Length > 0)
                    {
                        con += "\r\n";
                    }

                    con += c.ShowSEQ + " " + c.Name;

                    if (c.RelationVal.Length > 0)
                    {
                        con += ", " + c.RelationVal;

                        if (c.RelationComment.Length > 0)
                        {
                            con += "（" + c.RelationComment + "）";
                        }
                    }

                    if (c.Tel1.Length > 0)
                    {
                        con += ", " + c.Tel1;

                        if (c.KindVal1.Length > 0)
                        {
                            con += "（" + c.KindVal1 + "）";
                        }
                    }

                    if (c.Tel2.Length > 0)
                    {
                        con += ", " + c.Tel2;

                        if (c.KindVal2.Length > 0)
                        {
                            con += "（" + c.KindVal2 + "）";
                        }
                    }

                    if (c.Tel3.Length > 0)
                    {
                        con += ", " + c.Tel3;

                        if (c.KindVal3.Length > 0)
                        {
                            con += "（" + c.KindVal3 + "）";
                        }
                    }

                    if (c.Cont.Length > 0)
                    {
                        con += ", " + c.Cont;
                    }
                }

                FamilyButton.Tag = con;
                FamilyButton.BackColor = Color.FromArgb(255, 192, 192);
            }
            else
            {
                FamilyButton.Tag = "";
                FamilyButton.BackColor = Color.FromArgb(255, 255, 192);
            }

            // 禁忌情報を取得
            List<AllergyData> allergy_list = AllergyData.GetList(this.Pat.Id);

            if (allergy_list.Count > 0)
            {
                string con = "";

                foreach (AllergyData allergy in allergy_list)
                {
                    if (con.Length > 0)
                    {
                        con += "\r\n";
                    }

                    con += allergy.GroupName + " " + allergy.Name + " " + allergy.Cont;
                }

                AllergyButton.Tag = con;
                AllergyButton.BackColor = Color.FromArgb(255, 192, 192);
            }
            else
            {
                AllergyButton.Tag = "";
                AllergyButton.BackColor = Color.FromArgb(255, 255, 192);
            }

            this.PtOpeHistoryShow();
            this.PtKensaHistoryShow();

            this.AllOpeClear();
            this.AllKensaClear();

            this._SumPage.Show(this.Pat.Id);

            // 検査データを表示
            KensaShow(this.Pat.Id, KensaDate.Value.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// 手術歴を表示する
        /// </summary>
        private void PtOpeHistoryShow()
        {
            DataTable tmpTable = dSet.Tables["手術履歴"];
            tmpTable.Rows.Clear();

            List<EyeOpe> tmpList = EyeOpe.GetListByPatDates(this.Pat.Id, "", "");
            Dictionary<string, string> tmpRecDict;

            foreach (EyeOpe tmpOpe in tmpList)
            {
                DataRow r = tmpTable.NewRow();

                r["ID"] = tmpOpe.Id;
                r["OPE_DATE"] = tmpOpe.OpeDate;
                r["手術日"] = DateTimeAgent.DateFormat(tmpOpe.OpeDate, DateTimeAgent.DateFormatKind.SHORT);
                r["OPE_TIME"] = tmpOpe.OpeTime;
                r["時刻"] = tmpOpe.OpeTime.PadLeft(4, '0').Insert(2, ":");

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

                r["年齢"] = tmpOpe.Pat.AgeCalc(tmpOpe.OpeDate);

                if (tmpOpe.EyeR.Equals("1") && tmpOpe.EyeL.Equals("1"))
                {
                    r["眼"] = "B";
                }
                else if (tmpOpe.EyeR.Equals("1"))
                {
                    r["眼"] = "R";
                }
                else if (tmpOpe.EyeL.Equals("1"))
                {
                    r["眼"] = "L";
                }
                else
                {
                    r["眼"] = "";
                }

                if (tmpOpe.Infection.Contains("+"))
                {
                    r["感染"] = "+";
                }
                else
                {
                    r["感染"] = "-";
                }

                if (tmpOpe.Agree.Equals("1"))
                {
                    r["禁忌"] = "○";
                }
                else
                {
                    r["禁忌"] = "";
                }

                r["備考"] = tmpOpe.Comment;

                tmpRecDict = ContData.Parse(tmpOpe.OpeRecord);

                foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
                {
                    if (tmpRecDict.ContainsKey(tmpRow["Code"].ToString()))
                    {
                        r[tmpRow["Text"].ToString()] = tmpRecDict[tmpRow["Code"].ToString()];
                    }
                }

                tmpTable.Rows.Add(r);
            }

            OpeHistoryView.DataSource = new DataView(tmpTable);

            PtOpeHistoryWide();
        }

        /// <summary>
        /// OpeHistoryView の大きさに応じてカラムの表示・非表示と幅を調整する。
        /// </summary>
        void PtOpeHistoryWide()
        {
            if (OpeHistoryView.ColumnCount > 0)
            {
                if (OpeHistoryView.Width > 300)
                {
                    OpeHistoryView.Columns["ID"].Visible = false;

                    OpeHistoryView.Columns["OPE_DATE"].Visible = false;

                    OpeHistoryView.Columns["手術日"].Width = 55;
                    OpeHistoryView.Columns["手術日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["OPE_TIME"].Visible = false;

                    OpeHistoryView.Columns["時刻"].Visible = false;

                    OpeHistoryView.Columns["OPE_KIND"].Visible = false;

                    OpeHistoryView.Columns["種別"].Visible = false;

                    OpeHistoryView.Columns["手術室"].Visible = false;

                    OpeHistoryView.Columns["手術"].Width = 220;

                    OpeHistoryView.Columns["医師"].HeaderText = "Dr";
                    OpeHistoryView.Columns["医師"].Width = 55;

                    OpeHistoryView.Columns["麻酔"].Visible = false;

                    OpeHistoryView.Columns["病名"].Visible = false;

                    OpeHistoryView.Columns["入外"].Visible = false;

                    OpeHistoryView.Columns["年齢"].Visible = false;

                    OpeHistoryView.Columns["眼"].Width = 25;
                    OpeHistoryView.Columns["眼"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["感染"].Visible = false;

                    OpeHistoryView.Columns["禁忌"].Visible = false;

                    OpeHistoryView.Columns["備考"].Visible = false;

                    int width = 0;

                    foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
                    {
                        if (tmpRow["Width"].ToString().Length > 0 && int.TryParse(tmpRow["Width"].ToString(), out width))
                        {
                            OpeHistoryView.Columns[tmpRow["Text"].ToString()].Visible = true;
                            OpeHistoryView.Columns[tmpRow["Text"].ToString()].Width = width;
                        }
                    }
                }
                else
                {
                    OpeHistoryView.Columns["ID"].Visible = false;

                    OpeHistoryView.Columns["OPE_DATE"].Visible = false;

                    OpeHistoryView.Columns["手術日"].Width = 55;
                    OpeHistoryView.Columns["手術日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["OPE_TIME"].Visible = false;

                    OpeHistoryView.Columns["時刻"].Visible = false;

                    OpeHistoryView.Columns["OPE_KIND"].Visible = false;

                    OpeHistoryView.Columns["種別"].Visible = false;

                    OpeHistoryView.Columns["手術室"].Visible = false;

                    OpeHistoryView.Columns["手術"].Width = 145;

                    OpeHistoryView.Columns["医師"].HeaderText = "Dr";
                    OpeHistoryView.Columns["医師"].Width = 30;

                    OpeHistoryView.Columns["麻酔"].Visible = false;

                    OpeHistoryView.Columns["病名"].Visible = false;

                    OpeHistoryView.Columns["入外"].Visible = false;

                    OpeHistoryView.Columns["年齢"].Visible = false;

                    OpeHistoryView.Columns["眼"].Width = 25;
                    OpeHistoryView.Columns["眼"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["感染"].Visible = false;

                    OpeHistoryView.Columns["禁忌"].Visible = false;

                    OpeHistoryView.Columns["備考"].Visible = false;

                    foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
                    {
                        OpeHistoryView.Columns[tmpRow["Text"].ToString()].Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// 手術基本情報の内容を表示する。
        /// </summary>
        /// <param name="record"></param>
        private void OpeShow(EyeOpe ope)
        {
            this.TabChange(Tab.OPE);
            this.AllOpeClear();

            this.OpeIdBox.Text = ope.Id;

            this.OpeDateTimePicker.Value = DateTime.Parse(ope.OpeDate.Insert(4, "/").Insert(7, "/"));

            this.OpeBaseShow(ope);

            if (ope.InDate.Length == 8)
            {
                this.InDateTimePicker.Value = DateTime.Parse(ope.InDate.Insert(4, "/").Insert(7, "/"));
            }

            if (Dict.StaffDict.ContainsKey(ope.Staff))
            {
                this.OpeStaffLabel.Text = Dict.StaffDict[ope.Staff].Name;
            }

            this.RecordShow(EyeOpeRecord.Load(ope.Id));
            this.DoctorShow(EyeOpeDoctor.Load(ope.Id));
            this.PassShow(EyeOpePass.Load(ope.Id));

            this.OpeModeChange(Mode.SHOW);
        }

        /// <summary>
        /// 手術基本情報の内容を表示する。
        /// ただしID・手術日・入院日・スタッフは設定しない。
        /// </summary>
        /// <param name="ope"></param>
        private void OpeBaseShow(EyeOpe ope)
        {
            if (EyeDict.OpeKindDict.ContainsKey(ope.OpeKind))
            {
                this.OpeKindBox.Text = ope.OpeKind + " " + EyeDict.OpeKindDict[ope.OpeKind];
            }

            // この処理は OpeDateTimePicker と OpeKindBox の後にしなければならない。（OpeTimeBox がクリアされてしまうため）
            if (ope.OpeTime.Length > 0)
            {
                this.OpeTimeBox.Text = ope.OpeTime.PadLeft(4, '0');
            }

            this.OpeRoomBox.Text = ope.OpeRoom;
            this.OpeNameBox.Text = ope.OpeName;
            this.DoctorBox.Text = ope.Doctor;
            this.PlanTimeBox.Text = ope.PlanTime;
            this.AnesBox.Text = ope.Anes;
            this.DiagBox.Text = ope.Diag;

            this.InOutBox.Text = ope.InOut;
            this.InRoomBox.Text = ope.InRoom;
            this.InTimeBox.Text = ope.InTime;
            this.InTermBox.Text = ope.InTerm;

            this.EyeBoxR.Checked = ope.EyeR.Equals("1");
            this.EyeBoxL.Checked = ope.EyeL.Equals("1");

            this.HeightBox.Text = ope.Height;
            this.WeightBox.Text = ope.Weight;
            this.InfectionBox.Text = ope.Infection;
            this.PostDealBox.Text = ope.PostDeal;
            this.PastBox.Text = ope.Past;
            this.CommentBox.Text = ope.Comment;

            this.AllCheckBox.Checked = ope.AllCheck.Equals("1");
            this.ExplainBox.Checked = ope.Explain.Equals("1");
            this.EyeDropBox.Checked = ope.EyeDrop.Equals("1");
            this.AgreeBox.Checked = ope.Agree.Equals("1");
            this.PreCheckBox.Checked = ope.PreCheck.Equals("1");
            this.EarlierOKBox.Checked = ope.EarlierOK.Equals("1");

            // 身長・体重の値から、体表面積・ビスダイン溶液・ブドウ糖液の量を計算する。
            this.BodyCalc();
        }

        /// <summary>
        /// 手術記録の内容を表示する。
        /// </summary>
        /// <param name="record"></param>
        private void RecordShow(EyeOpeRecord record)
        {
            Dictionary<string, string> recordDict = ContData.Parse(record.Cont);

            foreach (TabPage p in RecordTabControl.TabPages)
            {
                foreach (Control c in p.Controls)
                {
                    if (c is TextBox || c is ComboBox)
                    {
                        if (recordDict.ContainsKey(c.Tag.ToString()))
                        {
                            c.Text = recordDict[c.Tag.ToString()];
                        }
                    }
                    else if (c is CheckBox)
                    {
                        if (recordDict.ContainsKey(c.Tag.ToString()))
                        {
                            ((CheckBox)c).Checked = true;
                        }
                    }
                }
            }

            RecordStatusBox.Checked = record.Status.Equals("1");

            if (Dict.StaffDict.ContainsKey(record.Staff))
            {
                this.RecordStaffLabel.Text = Dict.StaffDict[record.Staff].Name;
            }
        }

        /// <summary>
        /// 医師記録の内容を表示する。
        /// </summary>
        /// <param name="doctor"></param>
        private void DoctorShow(EyeOpeDoctor doctor)
        {
            PreContBox.Text = doctor.PreCont;
            DoContBox.Text = doctor.DoCont;

            DoctorStatusBox.Checked = doctor.Status.Equals("1");

            if (Dict.StaffDict.ContainsKey(doctor.Staff))
            {
                this.DoctorStaffLabel.Text = Dict.StaffDict[doctor.Staff].Name;
            }
        }

        /// <summary>
        /// 経過記録の内容を表示する。
        /// </summary>
        /// <param name="pass"></param>
        private void PassShow(EyeOpePass pass)
        {
            Dictionary<string, string> passDict = ContData.Parse(pass.Cont);

            foreach (Control c in PassPanel.Controls)
            {
                if (c is TextBox || c is ComboBox)
                {
                    if (passDict.ContainsKey(c.Tag.ToString()))
                    {
                        c.Text = passDict[c.Tag.ToString()];
                    }
                }
            }

            if (Dict.StaffDict.ContainsKey(pass.Staff))
            {
                this.PassStaffLabel.Text = Dict.StaffDict[pass.Staff].Name;
            }
        }

        /// <summary>
        /// 画面編集モード変更。
        /// </summary>
        /// <param name="mode"></param>
        private void OpeModeChange(Mode mode)
        {
            if (mode == Mode.NEW)
            {
                OpeRegButton.Enabled = true;
                RecordRegButton.Enabled = false;
                DoctorRegButton.Enabled = false;
                PassRegButton.Enabled = false;
            }
            else if (mode == Mode.SHOW)
            {
                OpeRegButton.Enabled = true;
                RecordRegButton.Enabled = true;
                DoctorRegButton.Enabled = true;
                PassRegButton.Enabled = true;
            }
        }

        /// <summary>
        /// 電子カルテからデータを取り込む。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetKarteDataButton_Click(object sender, EventArgs e)
        {
            // 感染症
            this.InfectionBox.Text = InfectionData.GetInfectionData(this.Pat.Id).ResultString;

            // 患者基本情報
            Dictionary<string, List<BaseInfo>> dict = BaseInfo.GetDict(this.Pat.Id);

            // 身長
            if (dict.ContainsKey(LibSettings.Current.BaseInfoCodes.Height))
            {
                this.HeightBox.Text = AppString.ZenToHan(dict[LibSettings.Current.BaseInfoCodes.Height][0].Value);
            }

            // 体重
            if (dict.ContainsKey(LibSettings.Current.BaseInfoCodes.Weight))
            {
                this.WeightBox.Text = AppString.ZenToHan(dict[LibSettings.Current.BaseInfoCodes.Weight][0].Value);
            }

            // 体表面積・ビスダイン溶液・ブドウ糖液の量を計算する。
            BodyCalc();
        }

        /// <summary>
        /// 入力された身長・体重から、体表面積・ビスダイン溶液・ブドウ糖液・DM単位を計算する。
        /// </summary>
        private void BodyCalc()
        {
            double d1 = 0.0;
            double d2 = 0.0;

            if (double.TryParse(HeightBox.Text, out d1) && double.TryParse(WeightBox.Text, out d2))
            {
                SurfaceBox.Text = Math.Round(Math.Pow(double.Parse(HeightBox.Text), 0.725) * Math.Pow(double.Parse(WeightBox.Text), 0.425) * 0.007184, 3).ToString();
                VisdineBox.Text = EyeDict.CalcVisdine(double.Parse(HeightBox.Text), double.Parse(WeightBox.Text)).ToString();
                GrapeBox.Text = EyeDict.CalcGrape(double.Parse(HeightBox.Text), double.Parse(WeightBox.Text)).ToString();

                DmBox.Text = Math.Round(Math.Pow(double.Parse(HeightBox.Text) / 100, 2) * 22 * 27 / 80, 1).ToString();
            }
        }

        private void HeightBox_Leave(object sender, EventArgs e)
        {
            this.BodyCalc();
        }

        private void WeightBox_Leave(object sender, EventArgs e)
        {
            this.BodyCalc();
        }

        /// <summary>
        /// 手術記録の新規作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryNewMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("手術記録を新規作成しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                this.TabChange(Tab.OPE);
                this.AllOpeClear();
            }
        }

        /// <summary>
        /// 選択された手術記録の基本情報をコピーして新規作成する。
        /// 手術日は本日の日付とする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (OpeHistoryView.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("選択された手術記録の基本情報をコピーして新規作成しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    EyeOpe tmpOpe = EyeOpe.Load(OpeHistoryView.SelectedRows[0].Cells["ID"].Value.ToString());

                    this.TabChange(Tab.OPE);

                    // AllOpeClear で ID がクリアされ、手術日が本日になる。
                    this.AllOpeClear();

                    // 入院日は引き継がず本日にリセットする。
                    this.InDateTimePicker.Value = DateTime.Now;

                    this.OpeBaseShow(tmpOpe);
                }
            }
        }

        /// <summary>
        /// 手術記録の新規作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeClearButton_Click(object sender, EventArgs e)
        {
            this.TabChange(Tab.OPE);
            this.AllOpeClear();
        }

        /// <summary>
        /// 手術記録を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryShowMenuItem_Click(object sender, EventArgs e)
        {
            if (OpeHistoryView.SelectedRows.Count > 0)
            {
                EyeOpe tmpOpe = EyeOpe.Load(OpeHistoryView.SelectedRows[0].Cells["ID"].Value.ToString());
                this.OpeShow(tmpOpe);
                this.TabChange(Tab.OPE);
            }
        }

        /// <summary>
        /// 手術記録を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (LoginUser.IsDoctor || MessageBox.Show("クリックされた記録を開きますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                EyeOpe tmpOpe = EyeOpe.Load(OpeHistoryView.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                this.OpeShow(tmpOpe);
                this.TabChange(Tab.OPE);
            }
        }

        /// <summary>
        /// 手術記録を削除する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryDeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (OpeHistoryView.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("削除しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    EyeOpe.Delete(OpeHistoryView.SelectedRows[0].Cells["ID"].Value.ToString(), LoginUser.Id);
                    MessageBox.Show("削除しました");

                    this.AllOpeClear();
                    this.PtOpeHistoryShow();
                }
            }
        }

        private void OpeRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("患者IDを入力してください");
                return;
            }

            int time = 0;

            if (this.PlanTimeBox.Text.Length > 0)
            {
                if (!int.TryParse(this.PlanTimeBox.Text, out time))
                {
                    MessageBox.Show("時間には数値を入力してください");
                    this.PlanTimeBox.Focus();
                    return;
                }
            }

            if (this.OpeTimeBox.Text.Length > 0)
            {
                if (!int.TryParse(this.OpeTimeBox.Text, out time))
                {
                    MessageBox.Show("時刻には数値を入力してください");
                    this.OpeTimeBox.Focus();
                    return;
                }
            }

            if (this.InTimeBox.Text.Length > 0)
            {
                if (!int.TryParse(this.InTimeBox.Text, out time))
                {
                    MessageBox.Show("時刻には数値を入力してください");
                    this.InTimeBox.Focus();
                    return;
                }
            }

            foreach (DataGridViewRow r in OpeHistoryView.Rows)
            {
                if (r.Cells["OPE_DATE"].Value.ToString().Equals(this.OpeDateTimePicker.Value.ToString("yyyyMMdd")))
                {
                    DialogResult result = MessageBox.Show("既に同日に手術登録がされています。登録しますか？\r\n　はい(Y) … 登録する\r\n　いいえ(N) … 登録しない＋画面クリア\r\n　キャンセル … 登録しない", "確認", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);

                    if (result == DialogResult.Yes)
                    {
                        break;
                    }
                    else if (result == DialogResult.No)
                    {
                        this.AllOpeClear();
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            EyeOpe ope = new EyeOpe();

            ope.Id = this.OpeIdBox.Text;
            ope.PtId = this.Pat.Id;
            ope.OpeDate = this.OpeDateTimePicker.Value.ToString("yyyyMMdd");
            ope.OpeTime = this.OpeTimeBox.Text;

            if (this.OpeKindBox.Text.Contains(" "))
            {
                ope.OpeKind = this.OpeKindBox.Text.Split(' ')[0];
            }

            ope.OpeRoom = this.OpeRoomBox.Text;
            ope.OpeName = this.OpeNameBox.Text;
            ope.Doctor = this.DoctorBox.Text;
            ope.PlanTime = this.PlanTimeBox.Text;
            ope.Anes = this.AnesBox.Text;
            ope.Diag = this.DiagBox.Text;
            ope.InOut = this.InOutBox.Text;

            if (!ope.InOut.Contains("外来"))
            {
                ope.InRoom = this.InRoomBox.Text;
                ope.InDate = this.InDateTimePicker.Value.ToString("yyyyMMdd");
                ope.InTime = this.InTimeBox.Text;
                ope.InTerm = this.InTermBox.Text;
            }

            ope.EyeR = this.EyeBoxR.Checked ? "1" : "0";
            ope.EyeL = this.EyeBoxL.Checked ? "1" : "0";

            ope.Height = this.HeightBox.Text;
            ope.Weight = this.WeightBox.Text;
            ope.Infection = this.InfectionBox.Text;
            ope.PostDeal = this.PostDealBox.Text;
            ope.Past = this.PastBox.Text;
            ope.Comment = this.CommentBox.Text;

            ope.AllCheck = this.AllCheckBox.Checked ? "1" : "0";
            ope.Explain = this.ExplainBox.Checked ? "1" : "0";
            ope.EyeDrop = this.EyeDropBox.Checked ? "1" : "0";
            ope.Agree = this.AgreeBox.Checked ? "1" : "0";
            ope.PreCheck = this.PreCheckBox.Checked ? "1" : "0";
            ope.EarlierOK = this.EarlierOKBox.Checked ? "1" : "0";

            ope.Staff = LoginUser.Id;
            ope.Status = "1";

            ope.Save();

            MessageBox.Show("保存しました");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void RecordRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("患者IDを入力してください");
                return;
            }

            if (this.OpeIdBox.Text.Length == 0)
            {
                MessageBox.Show("先に基本情報を保存してください");
                return;
            }

            EyeOpeRecord ope = new EyeOpeRecord();

            ope.Id = this.OpeIdBox.Text;

            ope.Cont = ContData.Build(RecordTabControl.TabPages);
            ope.Staff = LoginUser.Id;
            ope.Status = RecordStatusBox.Checked ? "1" : "2";

            ope.Save();

            MessageBox.Show("保存しました");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void DoctorRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("患者IDを入力してください");
                return;
            }

            if (this.OpeIdBox.Text.Length == 0)
            {
                MessageBox.Show("先に基本情報を保存してください");
                return;
            }

            EyeOpeDoctor ope = new EyeOpeDoctor();

            ope.Id = this.OpeIdBox.Text;
            ope.PreCont = PreContBox.Text;
            ope.DoCont = DoContBox.Text;
            ope.Staff = LoginUser.Id;

            ope.Status = DoctorStatusBox.Checked ? "1" : "2";

            ope.Save();

            MessageBox.Show("保存しました");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void PassRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("患者IDを入力してください");
                return;
            }

            if (this.OpeIdBox.Text.Length == 0)
            {
                MessageBox.Show("先に基本情報を保存してください");
                return;
            }

            EyeOpePass ope = new EyeOpePass();

            ope.Id = this.OpeIdBox.Text;

            ope.Cont = ContData.Build(PassPanel.Controls);
            ope.Staff = LoginUser.Id;
            ope.Status = "1";

            ope.Save();

            MessageBox.Show("保存しました");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void RsvButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0)
            {
                FormControl.FormOpeRsv_Show(this.Pat.Id);
            }
            else
            {
                FormControl.FormOpeRsv_Show();
            }
        }

        private void PDFButton_Click(object sender, EventArgs e)
        {
            this.Pat.WritePatCSV();
            Launcher.PdfViewer();
        }

        private void InnoButton_Click(object sender, EventArgs e)
        {
            if (!InnoProgram.KarteShow(this.Pat.Id))
            {
                MessageBox.Show("カルテが起動していません");
            }
        }

        private void GrapaButton_Click(object sender, EventArgs e)
        {
            this.Pat.WritePatCSV();
            Launcher.Start("Grapa/GraphicPavilion.exe");
        }

        private void KensaWideBox_CheckedChanged(object sender, EventArgs e)
        {
            if (KensaWideBox.Checked && OpeWideBox.Checked)
            {
                OpeWideBox.Checked = false;
            }

            this.PtHistoryWide();
        }

        private void OpeWideBox_CheckedChanged(object sender, EventArgs e)
        {
            if (KensaWideBox.Checked && OpeWideBox.Checked)
            {
                KensaWideBox.Checked = false;
            }

            this.PtHistoryWide();
        }

        void PtHistoryWide()
        {
            if (KensaWideBox.Checked)
            {
                // 手術歴を非表示にする
                OpeHistoryLabel.Visible = false;
                OpeClearButton.Visible = false;
                OpeWideBox.Visible = false;

                // 検査歴の表示位置を変更する
                KensaHistoryLabel.Location = new Point(3, 35);
                KensaClearButton.Location = new Point(60, 29);
                KensaHistoryView.Location = new Point(3, 50);
                KensaHistoryView.Width = this.Width - 20;
                KensaHistoryLabel.Visible = true;
                KensaClearButton.Visible = true;
                KensaHistoryView.Visible = true;
                KensaWideBox.Visible = true;
            }
            else if (OpeWideBox.Checked)
            {
                // 手術歴の表示位置を変更する
                OpeHistoryLabel.Location = new Point(3, 35);
                OpeClearButton.Location = new Point(60, 29);
                OpeHistoryView.Location = new Point(3, 50);
                OpeHistoryView.Width = this.Width - 20;
                OpeHistoryLabel.Visible = true;
                OpeClearButton.Visible = true;
                OpeWideBox.Visible = true;

                // 検査歴を非表示にする
                KensaHistoryLabel.Visible = false;
                KensaClearButton.Visible = false;
                KensaHistoryView.Visible = false;
                KensaWideBox.Visible = false;
            }
            else
            {
                // 手術歴を表示する
                OpeHistoryLabel.Location = new Point(3, 35);
                OpeClearButton.Location = new Point(54, 29);
                OpeHistoryView.Location = new Point(3, 50);
                OpeHistoryView.Width = this._OpeHistoryWidth;
                OpeHistoryLabel.Visible = true;
                OpeClearButton.Visible = true;
                OpeWideBox.Visible = true;

                // 検査歴の表示する
                KensaHistoryLabel.Location = new Point(this._OpeHistoryWidth + 6, 35);
                KensaClearButton.Location = new Point(this._OpeHistoryWidth + 59, 29);
                KensaHistoryView.Location = new Point(this._OpeHistoryWidth + 6, 50);
                KensaHistoryView.Width = this._KensaHistoryWidth > 0 ? this._KensaHistoryWidth : this.Width - this._OpeHistoryWidth - 23;
                KensaHistoryLabel.Visible = true;
                KensaClearButton.Visible = true;
                KensaHistoryView.Visible = true;
                KensaWideBox.Visible = true;
            }

            PtOpeHistoryWide();
        }

        /// <summary>
        /// 検査データを開く。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KensaHistoryView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            string kensa_date = KensaHistoryView.Rows[e.RowIndex].Cells["KENSA_DATE"].Value.ToString().Insert(4, "/").Insert(7, "/");
            DateTime d = DateTime.Now;

            if (this.Pat.Id.Length == 0 || !DateTime.TryParse(kensa_date, out d))
            {
                return;
            }

            if (LoginUser.IsDoctor || MessageBox.Show("クリックされた日の記録を開きますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                // KensaDate の値が変われば自動的に KensaShow() が呼ばれる
                this.KensaDate.Value = DateTime.Parse(kensa_date);

                this.TabChange(Tab.KENSA);
            }
        }

        private void KensaShowButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                return;
            }

            this.KensaShow(this.Pat.Id, KensaDate.Value.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// 検査データを表示する。
        /// </summary>
        /// <param name="kensa"></param>
        private void KensaShow(string pt_id, string kensa_date)
        {
            if (pt_id.Length == 0)
            {
                return;
            }

            ((KensaPanel)(this.KensaTab.Controls["KensaPanel"])).KensaShow(pt_id, kensa_date);
            this.KensaRowSelect();
        }

        /// <summary>
        /// KensaHistoryView で _KensaDate に該当する日付があれば選択する
        /// </summary>
        private void KensaRowSelect()
        {
            foreach (DataGridViewRow r in this.KensaHistoryView.Rows)
            {
                if (r.Cells["KENSA_DATE"].Value.ToString().Equals(this._KensaDate))
                {
                    r.Selected = true;
                }
            }
        }

        /// <summary>
        /// 右視力データをクリップボードにコピーする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SightRCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (KensaHistoryView.SelectedRows.Count > 0)
            {
                Clipboard.SetDataObject(KensaHistoryView.SelectedRows[0].Cells["Sight_R"].Value.ToString());
            }
        }

        /// <summary>
        /// 左視力データをクリップボードにコピーする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SightLCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (KensaHistoryView.SelectedRows.Count > 0)
            {
                Clipboard.SetDataObject(KensaHistoryView.SelectedRows[0].Cells["Sight_L"].Value.ToString());
            }
        }

        /// <summary>
        /// 両視力データをクリップボードにコピーする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SightBCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (KensaHistoryView.SelectedRows.Count > 0)
            {
                Clipboard.SetDataObject(KensaHistoryView.SelectedRows[0].Cells["Sight_R"].Value.ToString() + "\r\n" + KensaHistoryView.SelectedRows[0].Cells["Sight_L"].Value.ToString());
            }
        }

        /// <summary>
        /// 両視力・眼圧・網膜厚データをクリップボードにコピーする。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TensionBCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (KensaHistoryView.SelectedRows.Count > 0)
            {
                string sight = KensaHistoryView.SelectedRows[0].Cells["Sight_R"].Value.ToString() + "\r\n" + KensaHistoryView.SelectedRows[0].Cells["Sight_L"].Value.ToString();
                string tension = KensaHistoryView.SelectedRows[0].Cells["Tension_R"].Value.ToString() + "\r\n" + KensaHistoryView.SelectedRows[0].Cells["Tension_L"].Value.ToString();

                Clipboard.SetDataObject(sight + "\r\n\r\n" + tension);
            }
        }

        /// <summary>
        /// 検査記録の新規作成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KensaClearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("検査結果を新規作成しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                this.TabChange(Tab.KENSA);
                this.AllKensaClear();
            }
        }

        /// <summary>
        /// 検査タブのクリア
        /// </summary>
        private void AllKensaClear()
        {
            this.KensaDate.Value = DateTime.Now;
            ((KensaPanel)(this.KensaTab.Controls["KensaPanel"])).KensaClear();
        }

        private void InfoShareButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0)
            {
                FormMemo f1 = new FormMemo();
                f1.PatSet(this.Pat);
                f1.ShowDialog();
            }
        }

        private void FamilyButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0)
            {
                FormString1 f1 = new FormString1("家族連絡先", "家族連絡先", ((Button)sender).Tag.ToString());
                f1.ShowDialog();
            }
        }

        private void AllergyButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0)
            {
                FormString1 f1 = new FormString1("禁忌", "禁忌", ((Button)sender).Tag.ToString());
                f1.ShowDialog();
            }
        }

        public void KensaDo(string kensa_id, Dictionary<string, string> kensa_dict)
        {
            foreach (Control tc in this.KensaTab.Controls["KensaPanel"].Controls)
            {
                foreach (TabPage tp in ((TabControl)tc).TabPages)
                {
                    if (tp.Tag.ToString().Equals(kensa_id))
                    {
                        ((KensaPanelDetail)(tp.Controls["KensaPanel"])).KensaClear();

                        foreach (Control c in tp.Controls["KensaPanel"].Controls)
                        {
                            if (c is TextBox || c is ComboBox)
                            {
                                if (kensa_dict.ContainsKey(c.Tag.ToString()))
                                {
                                    c.Text = kensa_dict[c.Tag.ToString()];
                                }
                            }
                            else if (c is CheckBox)
                            {
                                if (kensa_dict.ContainsKey(c.Tag.ToString()) && kensa_dict[c.Tag.ToString()].Equals("1"))
                                {
                                    ((CheckBox)c).Checked = true;
                                }
                            }
                        }

                        break;
                    }
                }
            }
        }

        private void SumRegButton_Click(object sender, EventArgs e)
        {
            if (this._SumPage.Save(this.Pat.Id))
            {
                MessageBox.Show("登録しました");
            }
        }

        private void IVRegButton_Click(object sender, EventArgs e)
        {
            if (this._IVPage.Save(this.Pat.Id))
            {
                MessageBox.Show("登録しました");
                this._IVPage.HistoryShow(this.Pat.Id);
            }
        }

        private void IVDeleteButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0 && this.IVIdBox.Text.Length > 0)
            {
                if (MessageBox.Show("問診を削除しますか？", "確認", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    if (this._IVPage.Delete(this.IVIdBox.Text))
                    {
                        MessageBox.Show("削除しました");
                        this._IVPage.HistoryShow(this.Pat.Id);
                    }
                }
            }
        }

        private void IVHistoryView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow r = IVHistoryView.Rows[e.RowIndex];

            IVIdBox.Text = r.Cells["ID"].Value.ToString();
            IVDate.Value = DateTime.Parse(r.Cells["IV_DATE"].Value.ToString().Insert(4, "/").Insert(7, "/"));
            IVContBox.Text = r.Cells["内容"].Value.ToString();

            if (Dict.StaffDict.ContainsKey(r.Cells["STAFF"].Value.ToString()))
            {
                IVStaffLabel.Text = Dict.StaffDict[r.Cells["STAFF"].Value.ToString()].Name;
            }
        }

        private void IVClearButton_Click(object sender, EventArgs e)
        {
            this._IVPage.Clear();
        }

        private void Input2Button_Click(object sender, EventArgs e)
        {
            FormSumPlan1 fp1 = new FormSumPlan1(this);
            fp1.ShowDialog();
        }

        private void Clear2button_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("下の検査予定がすべてクリアされます。よろしいですか？", "確認", MessageBoxButtons.OK) == DialogResult.OK)
            {
                this._SumPage.Panel2Clear();
            }
        }

        private void SumInitValueButton3_Click(object sender, EventArgs e)
        {
            this._SumPage.InitValue3();
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

        private void FormPat_Resize(object sender, EventArgs e)
        {
            this.PtHistoryWide();
        }

        private void IVHistoryView_Resize(object sender, EventArgs e)
        {
            this._IVPage.HistoryFormat();
        }

        private void IVCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (IVHistoryView.SelectedRows.Count > 0)
            {
                string cont = IVHistoryView.SelectedRows[0].Cells["内容"].Value.ToString();

                if (cont.Length > 0)
                {
                    Clipboard.SetText(cont);
                }
            }
        }

        private void KensaDate_ValueChanged(object sender, EventArgs e)
        {
            if (this.KensaDate.Value.ToString("yyyyMMdd").Equals(this._KensaDate))
            {
                return;
            }

            bool b = true;

            if (this.KensaEdited)
            {
                // 変更確認ダイアログは実装困難なため外す 2018/08/17
            }

            if (b)
            {
                this._KensaDate = this.KensaDate.Value.ToString("yyyyMMdd");
                this.KensaShow(this.Pat.Id, this._KensaDate);
            }
            else
            {
                this.KensaDate.Value = DateTime.Parse(DateTimeAgent.DateFormat(this._KensaDate, DateTimeAgent.DateFormatKind.LONG));
                this.KensaRowSelect();
            }
        }

        private void FormPat_FormClosed(object sender, FormClosedEventArgs e)
        {
            FormControl.FormPat_Remove(this);
        }
    }
}