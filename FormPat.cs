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
            // 下記は FormBase を継承する場合
            /*
            this.OriginalText = "眼科患者記録";
            this.FormTextShow();
             */

            if (!MacsProgram.Exists)
            {
                this.MacsButton.Enabled = false;
            }

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

            this.TermBox.Items.Add("6ヶ月");
            this.TermBox.Items.Add("12ヶ月");
            this.TermBox.Items.Add("すべて");
            this.TermBox.Text = "6ヶ月";

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

//            this.PtOpeHistoryWide();
//            this.PtKensaHistoryWide();

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

            tmpTable = dSet.Tables.Add("来院歴");
            tmpTable.Columns.Add("日付");
            tmpTable.Columns.Add("診療科");
            tmpTable.Columns.Add("医師");

            tmpTable = dSet.Tables.Add("入院歴");
            tmpTable.Columns.Add("入院日");
            tmpTable.Columns.Add("退院日");
            tmpTable.Columns.Add("転帰");
            tmpTable.Columns.Add("診療科");
            tmpTable.Columns.Add("医師");

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

            OpeRoomBox.Items.Clear();
            OpeRoomBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeRoom"].Rows)
            {
                OpeRoomBox.Items.Add(r["Value"].ToString());
            }

            OpeNameBox.Items.Clear();
            OpeNameBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeName"].Rows)
            {
                OpeNameBox.Items.Add(r["Value"].ToString());
            }

            DoctorBox.Items.Clear();
            DoctorBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["Doctor"].Rows)
            {
                DoctorBox.Items.Add(r["Value"].ToString());
            }

            PlanTimeBox.Items.Clear();
            PlanTimeBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["PlanTime"].Rows)
            {
                PlanTimeBox.Items.Add(r["Value"].ToString());
            }

            AnesBox.Items.Clear();
            AnesBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["Anes"].Rows)
            {
                AnesBox.Items.Add(r["Value"].ToString());
            }

            DiagBox.Items.Clear();
            DiagBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["Diag"].Rows)
            {
                DiagBox.Items.Add(r["Value"].ToString());
            }

            InOutBox.Items.Clear();
            InOutBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["InOut"].Rows)
            {
                InOutBox.Items.Add(r["Value"].ToString());
            }

            InRoomBoxChange();
            InDateChange();

            InTimeBox.Items.Clear();
            InTimeBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["InTime"].Rows)
            {
                InTimeBox.Items.Add(r["Value"].ToString());
            }

            InTermBox.Items.Clear();
            InTermBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["InTerm"].Rows)
            {
                InTermBox.Items.Add(r["Value"].ToString());
            }

            PostDealBox.Items.Clear();
            PostDealBox.Items.Add("");

            foreach (DataRow r in EyeDict.EyeSet.Tables["PostDeal"].Rows)
            {
                PostDealBox.Items.Add(r["Value"].ToString());
            }
        }

        /// <summary>
        /// 手術記録タブコントロールの初期化
        /// </summary>
        private void RecordTabControlInit()
        {
            RecordTabControl.TabPages.Clear();

            DataTable tmpTable = EyeDict.EyeSet.Tables["OpeTab"];

            foreach (DataRow r in tmpTable.Rows)
            {
                TabPage tmpPage = new TabPage(r["Name"].ToString());
                tmpPage.Name = r["Name"].ToString();

                DataRow[] tmpRows = EyeDict.EyeSet.Tables["OpeTabItem"].Select("OpeTab_id = " + r["OpeTab_id"].ToString());

                foreach (DataRow r2 in tmpRows)
                {
                    if (r2["Type"].ToString().Equals("Label"))
                    {
                        Label tmpLabel = new Label();
                        tmpLabel.Name = r2["Name"].ToString();
                        tmpLabel.Text = r2["Text"].ToString();

                        int x = 0;
                        int y = 0;

                        if (int.TryParse(r2["X"].ToString(), out x) && int.TryParse(r2["Y"].ToString(), out y))
                        {
                            tmpLabel.Location = new Point(x, y);
                        }

                        int w = 0;
                        int h = 0;

                        if (r2["Width"].ToString().Length > 0 && int.TryParse(r2["Width"].ToString(), out w))
                        {
                            tmpLabel.Width = int.Parse(r2["Width"].ToString());
                        }

                        if (r2["Height"].ToString().Length > 0 && int.TryParse(r2["Height"].ToString(), out h))
                        {
                            tmpLabel.Height = int.Parse(r2["Height"].ToString());
                        }

                        if (w == 0 && h == 0)
                        {
                            tmpLabel.AutoSize = true;
                        }

                        if (r2["Align"].ToString().Equals("Center", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                        }
                        else if (r2["Align"].ToString().Equals("Right", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpLabel.TextAlign = ContentAlignment.MiddleRight;
                        }
                        else if (r2["Align"].ToString().Equals("Left", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpLabel.TextAlign = ContentAlignment.MiddleLeft;
                        }

                        tmpPage.Controls.Add(tmpLabel);
                    }
                    else if (r2["Type"].ToString().Equals("TextBox"))
                    {
                        TextBox tmpBox = new TextBox();
                        tmpBox.Tag = r2["Code"].ToString();
                        tmpBox.Name = r2["Name"].ToString();
                        tmpBox.Text = r2["Text"].ToString();

                        int x = 0;
                        int y = 0;

                        if (int.TryParse(r2["X"].ToString(), out x) && int.TryParse(r2["Y"].ToString(), out y))
                        {
                            tmpBox.Location = new Point(x, y);
                        }

                        int w = 0;
                        int h = 0;

                        if (r2["Width"].ToString().Length > 0 && int.TryParse(r2["Width"].ToString(), out w))
                        {
                            tmpBox.Width = int.Parse(r2["Width"].ToString());
                        }

                        if (r2["Height"].ToString().Length > 0 && int.TryParse(r2["Height"].ToString(), out h))
                        {
                            tmpBox.Height = int.Parse(r2["Height"].ToString());
                        }

                        if (r2["Ime"].ToString().Equals("Hiragana", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.ImeMode = ImeMode.Hiragana;
                        }
                        else if (r2["Ime"].ToString().Equals("Disable", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.ImeMode = ImeMode.Disable;
                        }
                        else if (r2["Ime"].ToString().Equals("Off", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.ImeMode = ImeMode.Off;
                        }
                        else
                        {
                            tmpBox.ImeMode = ImeMode.NoControl;
                        }

                        if (r2["Align"].ToString().Equals("Center", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.TextAlign = HorizontalAlignment.Center;
                        }
                        else if (r2["Align"].ToString().Equals("Right", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.TextAlign = HorizontalAlignment.Right;
                        }
                        else if (r2["Align"].ToString().Equals("Left", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.TextAlign = HorizontalAlignment.Left;
                        }
                        
                        // データの自動計算。（IOL誤差）
                        if (r2["Name"].ToString().Equals("IOL_予想屈折TextBox"))
                        {
                            tmpBox.Leave += new EventHandler(PassDataBox_Leave);
                        }

                        tmpPage.Controls.Add(tmpBox);
                    }
                    else if (r2["Type"].ToString().Equals("ComboBox"))
                    {
                        ComboBox tmpBox = new ComboBox();
                        tmpBox.Tag = r2["Code"].ToString();
                        tmpBox.Name = r2["Name"].ToString();
                        tmpBox.Text = r2["Text"].ToString();

                        int x = 0;
                        int y = 0;

                        if (int.TryParse(r2["X"].ToString(), out x) && int.TryParse(r2["Y"].ToString(), out y))
                        {
                            tmpBox.Location = new Point(x, y);
                        }

                        int w = 0;
                        int h = 0;

                        if (r2["Width"].ToString().Length > 0 && int.TryParse(r2["Width"].ToString(), out w))
                        {
                            tmpBox.Width = int.Parse(r2["Width"].ToString());
                        }

                        if (r2["Height"].ToString().Length > 0 && int.TryParse(r2["Height"].ToString(), out h))
                        {
                            tmpBox.Height = int.Parse(r2["Height"].ToString());
                        }

                        if (r2["Ime"].ToString().Equals("Hiragana", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.ImeMode = ImeMode.Hiragana;
                        }
                        else if (r2["Ime"].ToString().Equals("Disable", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.ImeMode = ImeMode.Disable;
                        }
                        else if (r2["Ime"].ToString().Equals("Off", StringComparison.CurrentCultureIgnoreCase))
                        {
                            tmpBox.ImeMode = ImeMode.Off;
                        }
                        else
                        {
                            tmpBox.ImeMode = ImeMode.NoControl;
                        }

                        foreach (string s in r2["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(s);
                        }

                        tmpPage.Controls.Add(tmpBox);
                    }
                    else if (r2["Type"].ToString().Equals("CheckBox"))
                    {
                        CheckBox tmpBox = new CheckBox();
                        tmpBox.Tag = r2["Code"].ToString();
                        tmpBox.Name = r2["Name"].ToString();
                        tmpBox.Text = r2["Text"].ToString();
                        tmpBox.Location = new Point(int.Parse(r2["X"].ToString()), int.Parse(r2["Y"].ToString()));
                        tmpBox.AutoSize = true;

                        tmpPage.Controls.Add(tmpBox);
                    }
                }

                RecordTabControl.TabPages.Add(tmpPage);
            }
        }

        /// <summary>
        /// 経過記録パネルの初期化
        /// </summary>
        private void PassPanelControlInit()
        {
            PassPanel.Controls.Clear();
            passDict.Clear();

            int i = 0;

            for (i = KensaHistoryMenuStrip.Items.Count - 1; i >= 9;  i--)
            {
                KensaHistoryMenuStrip.Items.RemoveAt(i);
            }

            DataTable timeTable = EyeDict.EyeSet.Tables["OpePassTime"];
            DataTable itemTable = EyeDict.EyeSet.Tables["OpePassItem"];

            const int TIME_X_START = 115;
            const int TIME_Y_POS = 3;
            const int TIME_X_INTERVAL = 60;

            const int TIME_WIDTH = TIME_X_INTERVAL - 2;
            const int TIME_HEIGHT = 32;

            const int ITEM_X_POS = 2;
            const int ITEM_Y_START = TIME_Y_POS + TIME_HEIGHT + 3;
            const int ITEM_Y_INTERVAL = 25;

            const int ITEM_WIDTH = TIME_X_START - ITEM_X_POS - 2;
            const int ITEM_HEIGHT = ITEM_Y_INTERVAL - 5;

            Size time_size = new Size(TIME_WIDTH, TIME_HEIGHT);
            Size item_size = new Size(ITEM_WIDTH, ITEM_HEIGHT);

            Dictionary<string, int> pos_x_dict = new Dictionary<string, int>();
            string kensa_date = "";
            string kensa_add_kind = "";
            string kensa_add_value = "";
            int j = 0;

            int x_count = 0;
            int y_count = 0;
            const int X_COUNT_INTERVAL = 6;
            const int Y_COUNT_INTERVAL = 8;

            int pos_x = 0;
            int pos_y = 0;

            // 経過日時のラベルを生成する。
            for (i = 0; i < itemTable.Rows.Count;)
            {
                pos_x = TIME_X_START;
                x_count = 0;

                foreach (DataRow r in timeTable.Rows)
                {
                    if (x_count > X_COUNT_INTERVAL)
                    {
                        pos_x += TIME_X_START;
                        x_count = 0;
                    }

                    Label tmpLabel = new Label();
                    tmpLabel.Name = r["Name"].ToString() + "_" + i;
//                    tmpLabel.Name = r["Name"].ToString() + i;

                    kensa_date = "";
                    kensa_add_kind = "";
                    kensa_add_value = "";

                    if (r["Date"].ToString().Length > 0 && r["Date"].ToString().Split(',').Length >= 2)
                    {
                        kensa_add_kind = r["Date"].ToString().Split(',')[0];
                        kensa_add_value = r["Date"].ToString().Split(',')[1];

                        if (int.TryParse(kensa_add_value, out j))
                        {
                            if (kensa_add_kind.Equals("AddDays", StringComparison.CurrentCultureIgnoreCase))
                            {
                                kensa_date = OpeDateTimePicker.Value.AddDays(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                            }
                            else if (kensa_add_kind.Equals("AddMonths", StringComparison.CurrentCultureIgnoreCase))
                            {
                                kensa_date = OpeDateTimePicker.Value.AddMonths(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                            }
                            else if (kensa_add_kind.Equals("AddYears", StringComparison.CurrentCultureIgnoreCase))
                            {
                                kensa_date = OpeDateTimePicker.Value.AddYears(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                            }
                        }
                    }

                    tmpLabel.Text = r["Text"].ToString() + "\r\n" + kensa_date;
                    tmpLabel.AutoSize = false;
                    tmpLabel.BackColor = Color.LightCyan;
                    tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                    tmpLabel.Size = time_size;
                    tmpLabel.Location = new Point(pos_x, pos_y);

                    PassPanel.Controls.Add(tmpLabel);

                    if (i == 0)
                    {
                        KensaHistoryMenuStrip.Items.Add(new ToolStripSeparator());

                        ToolStripMenuItem tmpItem1 = new ToolStripMenuItem();
                        tmpItem1.Text = "右を " + r["Text"].ToString() + " にコピー";
                        tmpItem1.Tag = "R";
                        tmpItem1.Click += new EventHandler(KensaCopy);
                        KensaHistoryMenuStrip.Items.Add(tmpItem1);

                        ToolStripMenuItem tmpItem2 = new ToolStripMenuItem();
                        tmpItem2.Text = "左を " + r["Text"].ToString() + " にコピー";
                        tmpItem2.Tag = "L";
                        tmpItem2.Click += new EventHandler(KensaCopy);
                        KensaHistoryMenuStrip.Items.Add(tmpItem2);

                        passDict.Add(r["Text"].ToString(), r["Name"].ToString());
                        pos_x_dict.Add(r["Name"].ToString(), pos_x);
                    }

                    pos_x += TIME_X_INTERVAL;
                    x_count++;
                }

                pos_y += ITEM_Y_START;

                while (y_count <= Y_COUNT_INTERVAL)
                {
                    i++;
                    y_count++;
                    pos_y += ITEM_Y_INTERVAL;
                }

                y_count = 0;
            }

            x_count = 0;
            y_count = 0;
            pos_x = 0;
            pos_y = 0;

            // 検査項目のラベルを作成する
            for (i = 0; i < timeTable.Rows.Count;)
            {
                pos_y = ITEM_Y_START;
                y_count = 0;

                foreach (DataRow r in itemTable.Rows)
                {
                    if (y_count > Y_COUNT_INTERVAL)
                    {
                        pos_y += ITEM_Y_START;
                        y_count = 0;
                    }

                    Label itemLabel = new Label();
//                    itemLabel.Name = r["Name"].ToString() + i;
                    itemLabel.Name = r["Name"].ToString() + "_" + i;
                    itemLabel.Text = r["Text"].ToString();

                    if (r["Unit"].ToString().Length > 0)
                    {
                        itemLabel.Text += "(" + r["Unit"].ToString() + ")";
                    }

                    itemLabel.AutoSize = false;
                    itemLabel.BackColor = Color.LightYellow;
                    itemLabel.TextAlign = ContentAlignment.MiddleCenter;
                    itemLabel.Size = item_size;
                    itemLabel.Location = new Point(pos_x, pos_y);

                    PassPanel.Controls.Add(itemLabel);

                    pos_y += ITEM_Y_INTERVAL;
                    y_count++;
                }

                pos_x += TIME_X_START;

                while (x_count <= X_COUNT_INTERVAL)
                {
                    i++;
                    x_count++;
                    pos_x += TIME_X_INTERVAL;
                }

                x_count = 0;
            }

            x_count = 0;
            y_count = 0;
            pos_x = 0;
            pos_y = ITEM_Y_START;

            // ツールチップをいったんすべてクリアする。
            passTip.RemoveAll();

            foreach (DataRow r in itemTable.Rows)
            {
                if (y_count > Y_COUNT_INTERVAL)
                {
                    pos_y += ITEM_Y_START;
                    y_count = 0;
                }

                foreach (string s in r["Data"].ToString().Split(','))
                {
                    if (r["Type"].ToString().Equals("TextBox"))
                    {
                        TextBox tmpBox = new TextBox();
                        tmpBox.Tag = r["Code"].ToString() + "_" + s;
                        tmpBox.Name = r["Name"].ToString() + "_" + s;
                        tmpBox.Width = TIME_WIDTH;
                        tmpBox.Location = new Point(pos_x_dict[s], pos_y);
                        tmpBox.MaxLength = 12;

                        if (r["Ime"].ToString().Equals("Off"))
                        {
                            tmpBox.ImeMode = ImeMode.Off;
                        }
                        else if (r["Ime"].ToString().Equals("Hiragana"))
                        {
                            tmpBox.ImeMode = ImeMode.Hiragana;
                        }
                        else
                        {
                            tmpBox.ImeMode = ImeMode.Off;
                        }

                        tmpBox.TextAlign = HorizontalAlignment.Center;

                        // データの自動計算。（等価球面度数、IOL誤差等）
                        if (r["Name"].ToString().StartsWith("KyumenDosu") || r["Name"].ToString().StartsWith("EnchuDosu"))
                        {
                            tmpBox.Leave += new EventHandler(PassDataBox_Leave);
                        }
                        
                        passTip.SetToolTip(tmpBox, r["Text"].ToString() + " " + s);

                        PassPanel.Controls.Add(tmpBox);
                    }
                    else if (r["Type"].ToString().Equals("ComboBox"))
                    {
                        ComboBox tmpBox = new ComboBox();
                        tmpBox.Tag = r["Code"].ToString() + "_" + s;
                        tmpBox.Name = r["Name"].ToString() + "_" + s;
                        tmpBox.Width = TIME_WIDTH;
                        tmpBox.Location = new Point(pos_x_dict[s], pos_y);
                        tmpBox.MaxLength = 12;

                        if (r["Ime"].ToString().Equals("Off"))
                        {
                            tmpBox.ImeMode = ImeMode.Off;
                        }
                        else if (r["Ime"].ToString().Equals("Hiragana"))
                        {
                            tmpBox.ImeMode = ImeMode.Hiragana;
                        }
                        else
                        {
                            tmpBox.ImeMode = ImeMode.Off;
                        }

                        foreach (string ss in r["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(ss);
                        }

                        // データの自動計算。（等価球面度数、IOL誤差等）
                        if (r["Name"].ToString().StartsWith("KyumenDosu") || r["Name"].ToString().StartsWith("EnchuDosu"))
                        {
                            tmpBox.Leave += new EventHandler(PassDataBox_Leave);
                        }

                        passTip.SetToolTip(tmpBox, r["Text"].ToString() + " " + s);

                        PassPanel.Controls.Add(tmpBox);
                    }
                }

                pos_y += ITEM_Y_INTERVAL;
                y_count++;
            }
        }

        /// <summary>
        /// 経過記録パネルの日付ラベル変更
        /// </summary>
        void PassPanelControlDateChange()
        {
            DataTable tmpTable = EyeDict.EyeSet.Tables["OpePassTime"];

            string kensa_date = "";
            string kensa_add_kind = "";
            string kensa_add_value = "";
            int j = 0;

            foreach (DataRow r in tmpTable.Rows)
            {
                foreach (Control c in PassPanel.Controls)
                {
                    if (c.GetType().Name.Equals("Label") && c.Name.StartsWith(r["Name"].ToString() + "_"))
                    {
                        Label tmpLabel = (Label)c;

                        kensa_date = "";
                        kensa_add_kind = "";
                        kensa_add_value = "";

                        if (r["Date"].ToString().Length > 0 && r["Date"].ToString().Split(',').Length >= 2)
                        {
                            kensa_add_kind = r["Date"].ToString().Split(',')[0];
                            kensa_add_value = r["Date"].ToString().Split(',')[1];

                            if (int.TryParse(kensa_add_value, out j))
                            {
                                if (kensa_add_kind.Equals("AddDays", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    kensa_date = OpeDateTimePicker.Value.AddDays(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                                }
                                else if (kensa_add_kind.Equals("AddMonths", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    kensa_date = OpeDateTimePicker.Value.AddMonths(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                                }
                                else if (kensa_add_kind.Equals("AddYears", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    kensa_date = OpeDateTimePicker.Value.AddYears(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                                }
                            }
                        }

                        tmpLabel.Text = r["Text"].ToString() + "\r\n" + kensa_date;

                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// データの自動計算。（等価球面度数、IOL誤差等）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PassDataBox_Leave(object sender, EventArgs e)
        {
            this.CalcPassData();
        }

        /// <summary>
        /// データの自動計算。（等価球面度数、IOL誤差等）
        /// </summary>
        private void CalcPassData()
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables["OpePassTime"].Rows)
            {
                // 等価球面度数の計算
                if (PassPanel.Controls.ContainsKey("ToukaKyumenDosu_" + r["Name"].ToString()) && PassPanel.Controls.ContainsKey("KyumenDosu_" + r["Name"].ToString()) && PassPanel.Controls.ContainsKey("EnchuDosu_" + r["Name"].ToString()))
                {
                    Control c1 = PassPanel.Controls["KyumenDosu_" + r["Name"].ToString()];
                    Control c2 = PassPanel.Controls["EnchuDosu_" + r["Name"].ToString()];

                    double d1 = 0;
                    double d2 = 0;

                    if (c1.Text.Length > 0 && double.TryParse(c1.Text, out d1) && c2.Text.Length > 0 && double.TryParse(c2.Text, out d2))
                    {
                        PassPanel.Controls["ToukaKyumenDosu_" + r["Name"].ToString()].Text = EyeDict.CalcSE(d1, d2).ToString();

                        // IOL誤差の計算
                        if (PassPanel.Controls.ContainsKey("IOLGosa_" + r["Name"].ToString()) && RecordTabControl.TabPages.ContainsKey("白内障") && RecordTabControl.TabPages["白内障"].Controls.ContainsKey("IOL_予想屈折TextBox"))
                        {
                            Control c3 = RecordTabControl.TabPages["白内障"].Controls["IOL_予想屈折TextBox"];
                            double d3 = 0;

                            if (c3.Text.Length > 0 && double.TryParse(c3.Text, out d3))
                            {
                                PassPanel.Controls["IOLGosa_" + r["Name"].ToString()].Text = EyeDict.CalcIOLGosa(EyeDict.CalcSE(d1, d2), d3).ToString();
                            }
                        }
                    }
                }
            }
        }

        void KensaCopy(object sender, EventArgs e)
        {
            if (KensaHistoryView.SelectedRows.Count > 0)
            {
                ToolStripMenuItem tmpItem = (ToolStripMenuItem)(sender);
                string side = tmpItem.Tag.ToString();
                string time = passDict[tmpItem.Text.Split(' ')[1]];

                for (int i = 13; i < KensaHistoryView.Columns.Count; i++)
                {
                    DataGridViewCell cell = KensaHistoryView.SelectedRows[0].Cells[i];
                    string col_name = KensaHistoryView.Columns[cell.ColumnIndex].Name;

                    if (col_name.Contains("_" + side))
                    {
                        if (PassPanel.Controls.ContainsKey(col_name.Split('_')[0] + "_" + time))
                        {
                            PassPanel.Controls[col_name.Split('_')[0] + "_" + time].Text = cell.Value.ToString();
                        }
                    }
                }
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
                    if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
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
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
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
            if (InOutBox.Text.Equals("わかば") || InOutBox.Text.Equals("さくら") || InOutBox.Text.Equals("あやめ"))
            {
                InDateTimePicker.Enabled = true;
            }
            else
            {
                InDateTimePicker.Enabled = false;
            }
        }

        /// <summary>
        /// 入外の変更に応じて病室コンボボックスを変更する。
        /// </summary>
        private void InRoomBoxChange()
        {
            InRoomBox.Text = "";
            InRoomBox.Items.Clear();
            InRoomBox.Items.Add("");

            if (InOutBox.Text.Equals("外来"))
            {
                foreach (DataRow r in EyeDict.EyeSet.Tables["InRoom"].Select("InOut = '外来'"))
                {
                    InRoomBox.Items.Add(r["Value"].ToString());
                }
            }
            else if (InOutBox.Text.Equals("わかば"))
            {
                foreach (DataRow r in EyeDict.EyeSet.Tables["InRoom"].Select("InOut = 'わかば'"))
                {
                    InRoomBox.Items.Add(r["Value"].ToString());
                }
            }
            else if (InOutBox.Text.Equals("さくら"))
            {
                foreach (DataRow r in EyeDict.EyeSet.Tables["InRoom"].Select("InOut = 'さくら'"))
                {
                    InRoomBox.Items.Add(r["Value"].ToString());
                }
            }
            else if (InOutBox.Text.Equals("あやめ"))
            {
                foreach (DataRow r in EyeDict.EyeSet.Tables["InRoom"].Select("InOut = 'あやめ'"))
                {
                    InRoomBox.Items.Add(r["Value"].ToString());
                }
            }
        }

        /// <summary>
        /// 種類と手術日・時刻のチェックを行う。
        /// </summary>
        private bool OpeCheck()
        {
            int i = 0;

            if (OpeTimeBox.Text.Length == 0)
            {
                MessageBox.Show("時刻を入力してください");
                return false;
            }

            if (!int.TryParse(OpeTimeBox.Text, out i) || i < 0 || i > 2400)
            {
                MessageBox.Show("正しい時刻を入力してください");
                return false;
            }

            return true;
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

        private void OpeTimeBox_Leave(object sender, EventArgs e)
        {
            this.OpeCheck();
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

            // 紹介元を取得
            this.IntroBox.Text = "";

            List<Intro> introList = Intro.GetList(this.Pat.Id);

            foreach (Intro p in introList)
            {
                if ((p.Kind.Equals("2") || p.Kind.Equals("5") || p.Kind.Equals("6"))
                    && p.DeptFromCode.Equals("7"))
                {
                    this.IntroBox.Text = p.Hospital + " " + p.DeptTo + " " + p.DoctorTo + " 先生";
                    break;
                }
            }

            this.PtOpeHistoryShow();
            this.PtComeHistoryShow();
            this.PtInHistoryShow();
            this.PtKensaHistoryShow();

            this.AllOpeClear();
            this.AllKensaClear();

            this._SumPage.Show(this.Pat.Id);

            // 検査データを表示
            KensaShow(this.Pat.Id, KensaDate.Value.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// 来院歴を表示する
        /// </summary>
        private void PtComeHistoryShow()
        {
            if (this.Pat.Id.Length == 0)
            {
                return;
            }

            string start_date = "20060901";

            if (TermBox.Text.Equals("6ヶ月"))
            {
                start_date = DateTime.Now.AddMonths(-6).ToString("yyyyMMdd");
            }
            else if (TermBox.Text.Equals("12ヶ月"))
            {
                start_date = DateTime.Now.AddYears(-1).ToString("yyyyMMdd");
            }

            DataTable tmpTable = dSet.Tables["来院歴"];
            tmpTable.Rows.Clear();

            List<PatOut> tmpList = PatOut.GetHistory(this.Pat.Id, start_date);

            foreach (PatOut tmpPat in tmpList)
            {
                DataRow r = tmpTable.NewRow();

                r["日付"] = tmpPat.ComeDate.PadRight(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                r["診療科"] = tmpPat.DeptName;
                r["医師"] = tmpPat.DoctorName;

                tmpTable.Rows.Add(r);
            }

            this.PtComeHistoryFormat();
        }

        /// <summary>
        /// 来院歴の表示制御
        /// </summary>
        private void PtComeHistoryFormat()
        {
            DataView tmpView = new DataView(dSet.Tables["来院歴"]);

            if (AllDeptBox.Checked)
            {
                tmpView.RowFilter = "";
            }
            else
            {
                tmpView.RowFilter = "診療科 = '眼科'";
            }

            ComeHistoryView.DataSource = tmpView;

            ComeHistoryView.Columns["日付"].Width = 60;
            ComeHistoryView.Columns["日付"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            ComeHistoryView.Columns["診療科"].HeaderText = "科";
            ComeHistoryView.Columns["診療科"].Width = 45;
            ComeHistoryView.Columns["診療科"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            ComeHistoryView.Columns["医師"].Width = 55;
            ComeHistoryView.Columns["医師"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// 入院歴を表示する
        /// </summary>
        private void PtInHistoryShow()
        {
            if (this.Pat.Id.Length == 0)
            {
                return;
            }

            DataTable tmpTable = dSet.Tables["入院歴"];
            tmpTable.Rows.Clear();

            List<PatIn> list = PatIn.GetHistory(this.Pat.Id);

            foreach (PatIn obj in list)
            {
                DataRow r = tmpTable.NewRow();

                r["入院日"] = obj.InDateStringShort;
                r["退院日"] = obj.OutDateStringShort;
                r["転帰"] = obj.OutKindString;
                r["医師"] = obj.DoctorName;
                r["診療科"] = obj.DeptName;

                tmpTable.Rows.Add(r);
            }

            this.PtInHistoryFormat();
        }

        /// <summary>
        /// 入院歴の表示制御
        /// </summary>
        private void PtInHistoryFormat()
        {
            DataView tmpView = new DataView(dSet.Tables["入院歴"]);

            InHistoryView.DataSource = tmpView;

            InHistoryView.Columns["入院日"].Width = 55;
            InHistoryView.Columns["入院日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            InHistoryView.Columns["退院日"].Width = 55;
            InHistoryView.Columns["退院日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            InHistoryView.Columns["転帰"].Visible = false;

            InHistoryView.Columns["診療科"].HeaderText = "科";
            InHistoryView.Columns["診療科"].Width = 40;
            InHistoryView.Columns["診療科"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            InHistoryView.Columns["医師"].Width = 45;
            InHistoryView.Columns["医師"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }

        /// <summary>
        /// 手術歴を表示する
        /// </summary>
        private void PtOpeHistoryShow()
        {
            DataTable tmpTable = dSet.Tables["手術履歴"];
            tmpTable.Rows.Clear();

            List<EyeOpe> tmpList = EyeOpe.GetListByPatDates(this.Pat.Id, "", "");
            Dictionary<string, string> tmpRecDict = new Dictionary<string, string>();

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

                tmpRecDict.Clear();

                foreach (string line in tmpOpe.OpeRecord.Split('\r', '\n'))
                {
                    string[] s = line.Split(',');

                    if (s.Length >= 2)
                    {
                        string value = "";

                        for (int j = 1; j < s.Length; j++)
                        {
                            if (value.Length > 0)
                            {
                                value += ",";
                            }

                            value += s[j];
                        }

                        tmpRecDict.Add(s[0], value);
                    }
                }

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
        /// 検査歴を表示する
        /// </summary>
        public void PtKensaHistoryShow()
        {
            DataTable tmpTable = dSet.Tables["検査歴"];
            tmpTable.Rows.Clear();

            List<EyeKensa> tmpList = EyeKensa.LoadByPatient(this.Pat.Id, "");

            string tmpDate = "";
            DataRow tmpRow = tmpTable.NewRow();

            foreach (EyeKensa tmpKensa in tmpList)
            {
                // XMLマスターにない検査ならば飛ばす
                if (!tmpTable.Columns.Contains(tmpKensa.KensaId))
                {
                    continue;
                }

                if (!tmpDate.Equals(tmpKensa.KensaDate))
                {
                    if (tmpRow["検査日"].ToString().Length > 0)
                    {
                        tmpTable.Rows.Add(tmpRow);
                    }

                    tmpDate = tmpKensa.KensaDate;
                    tmpRow = tmpTable.NewRow();
                    tmpRow["KENSA_DATE"] = tmpKensa.KensaDate;
                    tmpRow["検査日"] = tmpKensa.KensaDate.PadRight(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                }

                tmpRow[tmpKensa.KensaId] = "○";

                // KensaId に応じて表示用の検査データを作成する
                if (tmpKensa.KensaId.Equals("1"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("101R", "");
                    tmpDict.Add("102R", "");
                    tmpDict.Add("103R", "");
                    tmpDict.Add("104R", "");
                    tmpDict.Add("105R", "");
                    tmpDict.Add("106R", "");
                    tmpDict.Add("101L", "");
                    tmpDict.Add("102L", "");
                    tmpDict.Add("103L", "");
                    tmpDict.Add("104L", "");
                    tmpDict.Add("105L", "");
                    tmpDict.Add("106L", "");
                    tmpDict.Add("107R", "");
                    tmpDict.Add("107L", "");
                    tmpDict.Add("108R", "");
                    tmpDict.Add("108L", "");
                    tmpDict.Add("109R", "");
                    tmpDict.Add("110R", "");
                    tmpDict.Add("109L", "");
                    tmpDict.Add("110L", "");
                    tmpDict.Add("109B", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // 遠見視力（右）データ作成
                    if (tmpDict["108R"].Equals("1"))
                    {
                        tmpRow["Sight_R"] = "RV=" + tmpDict["109R"] + "(" + tmpDict["110R"] + ")";
                    }
                    else if (tmpDict["107R"].Equals("1"))
                    {
                        tmpRow["Sight_R"] = "RV=" + tmpDict["101R"] + "(" + tmpDict["102R"] + "×" + tmpDict["103R"] + "D)";
                    }
                    else
                    {
                        tmpRow["Sight_R"] = "RV=" + tmpDict["101R"] + "(" + tmpDict["102R"] + "×" + tmpDict["103R"] + "D=cyl" + tmpDict["104R"] + "D Ax" + tmpDict["105R"] + "°)";
                    }

                    // 遠見視力（左）データ作成
                    if (tmpDict["108L"].Equals("1"))
                    {
                        tmpRow["Sight_L"] = "LV=" + tmpDict["109L"] + "(" + tmpDict["110L"] + ")";
                    }
                    else if (tmpDict["107L"].Equals("1"))
                    {
                        tmpRow["Sight_L"] = "LV=" + tmpDict["101L"] + "(" + tmpDict["102L"] + "×" + tmpDict["103L"] + "D)";
                    }
                    else
                    {
                        tmpRow["Sight_L"] = "LV=" + tmpDict["101L"] + "(" + tmpDict["102L"] + "×" + tmpDict["103L"] + "D=cyl" + tmpDict["104L"] + "D Ax" + tmpDict["105L"] + "°)";
                    }

                    // 視力（右）データ作成
                    if (tmpDict["102R"].Length == 0 || tmpDict["102R"].Contains("n.c"))
                    {
                        tmpRow["CorrectSight_R"] = tmpDict["101R"];
                    }
                    else
                    {
                        tmpRow["CorrectSight_R"] = tmpDict["102R"];
                    }

                    // 視力（左）データ作成
                    if (tmpDict["102L"].Length == 0 || tmpDict["102L"].Contains("n.c"))
                    {
                        tmpRow["CorrectSight_L"] = tmpDict["101L"];
                    }
                    else
                    {
                        tmpRow["CorrectSight_L"] = tmpDict["102L"];
                    }

                    // 球面度数データ作成
                    tmpRow["KyumenDosu_R"] = tmpDict["103R"];
                    tmpRow["KyumenDosu_L"] = tmpDict["103L"];

                    // 円柱度数データ作成
                    tmpRow["EnchuDosu_R"] = tmpDict["104R"];
                    tmpRow["EnchuDosu_L"] = tmpDict["104L"];
                }
                else if (tmpKensa.KensaId.Equals("3"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("301R", "");
                    tmpDict.Add("302R", "");
                    tmpDict.Add("303R", "");
                    tmpDict.Add("304R", "");
                    tmpDict.Add("301L", "");
                    tmpDict.Add("302L", "");
                    tmpDict.Add("303L", "");
                    tmpDict.Add("304L", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // 眼圧（右）データ作成
                    tmpRow["TensionAvg_R"] = tmpDict["304R"];
                    tmpRow["Tension_R"] = "RT=" + tmpDict["301R"] + "," + tmpDict["302R"] + "," + tmpDict["303R"] + " (AVG)" + tmpDict["304R"] + "mmHg";

                    // 眼圧（左）データ作成
                    tmpRow["TensionAvg_L"] = tmpDict["304L"];
                    tmpRow["Tension_L"] = "LT=" + tmpDict["301L"] + "," + tmpDict["302L"] + "," + tmpDict["303L"] + " (AVG)" + tmpDict["304L"] + "mmHg";
                }
                else if (tmpKensa.KensaId.Equals("5"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("501R", "");
                    tmpDict.Add("501L", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // GATデータ作成
                    tmpRow["GAT_R"] = tmpDict["501R"];
                    tmpRow["GAT_L"] = tmpDict["501L"];
                }
                else if (tmpKensa.KensaId.Equals("7"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("701R", "");
                    tmpDict.Add("701L", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // 綿糸法データ作成
                    tmpRow["Menshihou_R"] = tmpDict["701R"];
                    tmpRow["Menshihou_L"] = tmpDict["701L"];
                }
                else if (tmpKensa.KensaId.Equals("8"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("801R", "");
                    tmpDict.Add("802R", "");
                    tmpDict.Add("801L", "");
                    tmpDict.Add("802L", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // Mチャートデータ作成
                    tmpRow["MChartTate_R"] = tmpDict["801R"];
                    tmpRow["MChartYoko_R"] = tmpDict["802R"];
                    tmpRow["MChartTate_L"] = tmpDict["801L"];
                    tmpRow["MChartYoko_L"] = tmpDict["802L"];
                }
                else if (tmpKensa.KensaId.Equals("9"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("901R", "");
                    tmpDict.Add("902R", "");
                    tmpDict.Add("903R", "");
                    tmpDict.Add("904R", "");
                    tmpDict.Add("905R", "");
                    tmpDict.Add("901L", "");
                    tmpDict.Add("902L", "");
                    tmpDict.Add("903L", "");
                    tmpDict.Add("904L", "");
                    tmpDict.Add("905L", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // コントラスト（右）データ作成
                    tmpRow["ContrastA_R"] = tmpDict["901R"];
                    tmpRow["ContrastB_R"] = tmpDict["902R"];
                    tmpRow["ContrastC_R"] = tmpDict["903R"];
                    tmpRow["ContrastD_R"] = tmpDict["904R"];
                    tmpRow["ContrastE_R"] = tmpDict["905R"];

                    // コントラスト（左）データ作成
                    tmpRow["ContrastA_L"] = tmpDict["901L"];
                    tmpRow["ContrastB_L"] = tmpDict["902L"];
                    tmpRow["ContrastC_L"] = tmpDict["903L"];
                    tmpRow["ContrastD_L"] = tmpDict["904L"];
                    tmpRow["ContrastE_L"] = tmpDict["905L"];
                }
                else if (tmpKensa.KensaId.Equals("31"))
                {
                    Dictionary<string, string> tmpDict = new Dictionary<string, string>();

                    tmpDict.Add("1110R", "");
                    tmpDict.Add("1110L", "");

                    string tmpValue = "";

                    foreach (string line in tmpKensa.Cont.Split('\r', '\n'))
                    {
                        string[] s = line.Split(',');

                        if (tmpDict.ContainsKey(s[0]))
                        {
                            tmpValue = "";

                            for (int i = 1; i < s.Length; i++)
                            {
                                if (tmpValue.Length > 0)
                                {
                                    tmpValue += ",";
                                }

                                tmpValue += s[i].Replace("<CR+LF>", "\r\n");
                            }

                            tmpDict[s[0]] = tmpValue;
                        }
                    }

                    // 網膜厚データ作成
                    tmpRow["Moumakukou_R"] = tmpDict["1110R"];
                    tmpRow["Moumakukou_L"] = tmpDict["1110L"];
                }
            }

            if (tmpRow["検査日"].ToString().Length > 0)
            {
                tmpTable.Rows.Add(tmpRow);
            }

            // EyeKensa2 のデータがあればテーブルに追加する。
            List<EyeKensa2> tmpList2 = EyeKensa2.LoadByPatient(this.Pat.Id, "");

            foreach (EyeKensa2 tmpKensa in tmpList2)
            {
                // tmpTable の行をチェックして、同一検査日のものがあればそこで処理する。
                bool no_flg = true;

                foreach (DataRow r in tmpTable.Rows)
                {
                    if (r["KENSA_DATE"].ToString().Equals(tmpKensa.KensaDate))
                    {
                        r[tmpKensa.KensaId] = "○";
                        no_flg = false;
                        break;
                    }
                }

                // tmpTable の行に、同一検査日のものがなければ行を追加する。
                if (no_flg)
                {
                    DataRow r = tmpTable.NewRow();
                    r["KENSA_DATE"] = tmpKensa.KensaDate;
                    r["検査日"] = tmpKensa.KensaDate.PadRight(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                    r[tmpKensa.KensaId] = "○";
                    tmpTable.Rows.Add(r);
                }
            }

            DataView tmpView = new DataView(tmpTable);
            tmpView.Sort = "KENSA_DATE desc";

            KensaHistoryView.DataSource = tmpView;

            this.PtKensaHistoryWide();
        }

        /// <summary>
        /// KensaHistoryView の大きさに応じてカラムの表示・非表示と幅を調整する。
        /// </summary>
        void PtKensaHistoryWide()
        {
            KensaHistoryView.Columns["KENSA_DATE"].Visible = false;

            KensaHistoryView.Columns["検査日"].Width = 55;
            KensaHistoryView.Columns["検査日"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["KensaPage"].Rows)
            {
                KensaHistoryView.Columns[tmpRow["ID"].ToString()].Width = 25;
                KensaHistoryView.Columns[tmpRow["ID"].ToString()].HeaderText = tmpRow["Header"].ToString();
                KensaHistoryView.Columns[tmpRow["ID"].ToString()].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            int width = 0;

            foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["KensaHistory"].Rows)
            {
                if (tmpRow["Visible"].ToString().Equals("1"))
                {
                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].Visible = true;
                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].HeaderText = tmpRow["Text"].ToString();

                    if (tmpRow["Width"].ToString().Length > 0 && int.TryParse(tmpRow["Width"].ToString(), out width))
                    {
                        KensaHistoryView.Columns[tmpRow["Name"].ToString()].Width = width;
                    }

                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                else
                {
                    KensaHistoryView.Columns[tmpRow["Name"].ToString()].Visible = false;
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

            if (ope.InDate.Length == 8)
            {
                this.InDateTimePicker.Value = DateTime.Parse(ope.InDate.Insert(4, "/").Insert(7, "/"));
            }

            this.InTimeBox.Text = ope.InTime;
            this.InTermBox.Text = ope.InTerm;

            if (ope.EyeR.Equals("1"))
            {
                this.EyeBoxR.Checked = true;
            }

            if (ope.EyeL.Equals("1"))
            {
                this.EyeBoxL.Checked = true;
            }

            this.HeightBox.Text = ope.Height;
            this.WeightBox.Text = ope.Weight;
            this.InfectionBox.Text = ope.Infection;
            this.PostDealBox.Text = ope.PostDeal;
            this.PastBox.Text = ope.Past;
            this.CommentBox.Text = ope.Comment;

            if (ope.AllCheck.Equals("1"))
            {
                this.AllCheckBox.Checked = true;
            }

            if (ope.Explain.Equals("1"))
            {
                this.ExplainBox.Checked = true;
            }

            if (ope.EyeDrop.Equals("1"))
            {
                this.EyeDropBox.Checked = true;
            }

            if (ope.Agree.Equals("1"))
            {
                this.AgreeBox.Checked = true;
            }

            if (ope.PreCheck.Equals("1"))
            {
                this.PreCheckBox.Checked = true;
            }

            if (ope.EarlierOK.Equals("1"))
            {
                this.EarlierOKBox.Checked = true;
            }

            if (Dict.StaffDict.ContainsKey(ope.Staff))
            {
                this.OpeStaffLabel.Text = Dict.StaffDict[ope.Staff].Name;
            }

            // 身長・体重の値から、体表面積・ビスダイン溶液・ブドウ糖液の量を計算する。
            this.BodyCalc();

            this.RecordShow(EyeOpeRecord.Load(ope.Id));
            this.DoctorShow(EyeOpeDoctor.Load(ope.Id));
            this.PassShow(EyeOpePass.Load(ope.Id));

            this.OpeModeChange(Mode.SHOW);
        }

        /// <summary>
        /// 手術記録の内容を表示する。
        /// </summary>
        /// <param name="record"></param>
        private void RecordShow(EyeOpeRecord record)
        {
            string[] conts = record.Cont.Split('\r', '\n');
            Dictionary<string, string> recordDict = new Dictionary<string, string>();

            string value = "";

            foreach (string s in conts)
            {
                string[] ss = s.Split(',');

                if (ss.Length >= 2)
                {
                    if (!recordDict.ContainsKey(ss[0]))
                    {
                        value = "";

                        for (int i = 1; i < ss.Length; i++)
                        {
                            if (value.Length > 0)
                            {
                                value += ",";
                            }

                            value += ss[i];
                        }

                        recordDict.Add(ss[0], value.Replace("<CR+LF>", "\r\n"));
                    }
                }
            }

            foreach (TabPage p in RecordTabControl.TabPages)
            {
                foreach (Control c in p.Controls)
                {
                    if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                    {
                        if (recordDict.ContainsKey(c.Tag.ToString()))
                        {
                            c.Text = recordDict[c.Tag.ToString()];
                        }
                    }
                    else if (c.GetType().Name.Equals("CheckBox"))
                    {
                        if (recordDict.ContainsKey(c.Tag.ToString()))
                        {
                            ((CheckBox)c).Checked = true;
                        }
                    }
                }
            }

            if (record.Status.Equals("1"))
            {
                RecordStatusBox.Checked = true;
            }
            else
            {
                RecordStatusBox.Checked = false;
            }

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

            if (doctor.Status.Equals("1"))
            {
                DoctorStatusBox.Checked = true;
            }
            else
            {
                DoctorStatusBox.Checked = false;
            }

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
            string[] conts = pass.Cont.Split('\r', '\n');
            Dictionary<string, string> passDict = new Dictionary<string, string>();

            string value = "";

            foreach (string s in conts)
            {
                string[] ss = s.Split(',');

                if (ss.Length >= 2)
                {
                    if (!passDict.ContainsKey(ss[0]))
                    {
                        value = "";

                        for (int i = 1; i < ss.Length; i++)
                        {
                            if (value.Length > 0)
                            {
                                value += ",";
                            }

                            value += ss[i];
                        }

                        passDict.Add(ss[0], value.Replace("<CR+LF>", "\r\n"));
                    }
                }
            }

            foreach (Control c in PassPanel.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
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
        /// 眼科同意書を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgreePrintButton_Click(object sender, EventArgs e)
        {
            this.ExcelOpen(EyeDict.EyeSet.Tables["OpeExcel"].Rows[0]["Agree"].ToString());
        }

        /// <summary>
        /// 眼科申し送り書を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NsPrintButton_Click(object sender, EventArgs e)
        {
            this.ExcelOpen(EyeDict.EyeSet.Tables["OpeExcel"].Rows[0]["Ns"].ToString());
        }

        /// <summary>
        /// オペ録を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordPrintButton_Click(object sender, EventArgs e)
        {
            this.ExcelOpen(EyeDict.EyeSet.Tables["OpeExcel"].Rows[0]["Record"].ToString());
        }

        private void ExcelOpen(string file_name)
        {
            EyeDoc tmpDoc = new EyeDoc(this.Pat.Id);

            tmpDoc.FileName = file_name;

            EyeDoc.Item item1 = new EyeDoc.Item();
            item1.Kind = "手術基本情報";
            item1.Name = "眼球";

            if (EyeBoxR.Checked && EyeBoxL.Checked)
            {
                item1.Value = "両";
            }
            else if (EyeBoxR.Checked)
            {
                item1.Value = "右";
            }
            else if (EyeBoxL.Checked)
            {
                item1.Value = "左";
            }

            tmpDoc.ItemList.Add(item1);

            EyeDoc.Item item2 = new EyeDoc.Item();
            item2.Kind = "手術基本情報";
            item2.Name = "手術日";
            item2.Value = this.OpeDateTimePicker.Value.ToString("yyyy/MM/dd");
            tmpDoc.ItemList.Add(item2);

            EyeDoc.Item item3 = new EyeDoc.Item();
            item3.Kind = "手術基本情報";
            item3.Name = "種別";
            item3.Value = this.OpeKindBox.Text;
            tmpDoc.ItemList.Add(item3);

            EyeDoc.Item item4 = new EyeDoc.Item();
            item4.Kind = "手術基本情報";
            item4.Name = "時刻";
            item4.Value = this.OpeTimeBox.Text;
            tmpDoc.ItemList.Add(item4);

            EyeDoc.Item item5 = new EyeDoc.Item();
            item5.Kind = "手術基本情報";
            item5.Name = "手術室";
            item5.Value = this.OpeRoomBox.Text;
            tmpDoc.ItemList.Add(item5);

            EyeDoc.Item item6 = new EyeDoc.Item();
            item6.Kind = "手術基本情報";
            item6.Name = "術者";
            item6.Value = this.DoctorBox.Text;
            tmpDoc.ItemList.Add(item6);

            EyeDoc.Item item7 = new EyeDoc.Item();
            item7.Kind = "手術基本情報";
            item7.Name = "予定時間";
            item7.Value = this.PlanTimeBox.Text;
            tmpDoc.ItemList.Add(item7);

            EyeDoc.Item item8 = new EyeDoc.Item();
            item8.Kind = "手術基本情報";
            item8.Name = "麻酔";
            item8.Value = this.AnesBox.Text;
            tmpDoc.ItemList.Add(item8);

            EyeDoc.Item item9 = new EyeDoc.Item();
            item9.Kind = "手術基本情報";
            item9.Name = "病名";
            item9.Value = this.DiagBox.Text;
            tmpDoc.ItemList.Add(item9);

            EyeDoc.Item item10 = new EyeDoc.Item();
            item10.Kind = "手術基本情報";
            item10.Name = "術式";
            item10.Value = this.OpeNameBox.Text;
            tmpDoc.ItemList.Add(item10);

            EyeDoc.Item item11 = new EyeDoc.Item();
            item11.Kind = "手術基本情報";
            item11.Name = "入外";
            item11.Value = this.InOutBox.Text;
            tmpDoc.ItemList.Add(item11);

            EyeDoc.Item item12 = new EyeDoc.Item();
            item12.Kind = "手術基本情報";
            item12.Name = "病室";
            item12.Value = this.InRoomBox.Text;
            tmpDoc.ItemList.Add(item12);

            EyeDoc.Item item13 = new EyeDoc.Item();
            item13.Kind = "手術基本情報";
            item13.Name = "入院日";

            if (this.InDateTimePicker.Enabled)
            {
                item13.Value = this.InDateTimePicker.Value.ToString("yyyy/MM/dd");
            }

            tmpDoc.ItemList.Add(item13);

            EyeDoc.Item item14 = new EyeDoc.Item();
            item14.Kind = "手術基本情報";
            item14.Name = "入院時刻";
            item14.Value = this.InTimeBox.Text;
            tmpDoc.ItemList.Add(item14);

            EyeDoc.Item item15 = new EyeDoc.Item();
            item15.Kind = "手術基本情報";
            item15.Name = "入院期間";
            item15.Value = this.InTermBox.Text;
            tmpDoc.ItemList.Add(item15);

            EyeDoc.Item item16 = new EyeDoc.Item();
            item16.Kind = "手術基本情報";
            item16.Name = "身長";
            item16.Value = this.HeightBox.Text;
            tmpDoc.ItemList.Add(item16);

            EyeDoc.Item item17 = new EyeDoc.Item();
            item17.Kind = "手術基本情報";
            item17.Name = "体重";
            item17.Value = this.WeightBox.Text;
            tmpDoc.ItemList.Add(item17);

            EyeDoc.Item item18 = new EyeDoc.Item();
            item18.Kind = "手術基本情報";
            item18.Name = "体表";
            item18.Value = this.SurfaceBox.Text;
            tmpDoc.ItemList.Add(item18);

            EyeDoc.Item item19 = new EyeDoc.Item();
            item19.Kind = "手術基本情報";
            item19.Name = "ビスダイン";
            item19.Value = this.VisdineBox.Text;
            tmpDoc.ItemList.Add(item19);

            EyeDoc.Item item20 = new EyeDoc.Item();
            item20.Kind = "手術基本情報";
            item20.Name = "ブドウ糖";
            item20.Value = this.GrapeBox.Text;
            tmpDoc.ItemList.Add(item20);

            EyeDoc.Item item21 = new EyeDoc.Item();
            item21.Kind = "手術基本情報";
            item21.Name = "DM単位";
            item21.Value = this.DmBox.Text;
            tmpDoc.ItemList.Add(item21);

            EyeDoc.Item item22 = new EyeDoc.Item();
            item22.Kind = "手術基本情報";
            item22.Name = "感染症";
            item22.Value = this.InfectionBox.Text;
            tmpDoc.ItemList.Add(item22);

            /*
            Doc.Item item23 = new Doc.Item();
            item23.Kind = "手術基本情報";
            item23.Name = "術前処置";
            item23.Value = this.PostDealBox.Text;
            tmpDoc.ItemList.Add(item23);

            Doc.Item item24 = new Doc.Item();
            item24.Kind = "手術基本情報";
            item24.Name = "全身検査";

            if (this.AllCheckBox.Checked)
            {
                item24.Value = "1";
            }
            else
            {
                item24.Value = "0";
            }

            tmpDoc.ItemList.Add(item24);

            Doc.Item item25 = new Doc.Item();
            item25.Kind = "手術基本情報";
            item25.Name = "手術説明";

            if (this.ExplainBox.Checked)
            {
                item25.Value = "1";
            }
            else
            {
                item25.Value = "0";
            }

            tmpDoc.ItemList.Add(item25);

            Doc.Item item26 = new Doc.Item();
            item26.Kind = "手術基本情報";
            item26.Name = "点眼処方";

            if (this.EyeDropBox.Checked)
            {
                item26.Value = "1";
            }
            else
            {
                item26.Value = "0";
            }

            tmpDoc.ItemList.Add(item26);

            Doc.Item item27 = new Doc.Item();
            item27.Kind = "手術基本情報";
            item27.Name = "禁忌";

            if (this.AgreeBox.Checked)
            {
                item27.Value = "1";
            }
            else
            {
                item27.Value = "0";
            }

            tmpDoc.ItemList.Add(item27);

             */

            EyeDoc.Item item28 = new EyeDoc.Item();
            item28.Kind = "手術基本情報";
            item28.Name = "術前チェック完了";

            if (this.PreCheckBox.Checked)
            {
                item28.Value = "1";
            }
            else
            {
                item28.Value = "0";
            }

            tmpDoc.ItemList.Add(item28);

            /*
            Doc.Item item29 = new Doc.Item();
            item29.Kind = "手術基本情報";
            item29.Name = "手術履歴";
            item29.Value = this.PastBox.Text;
            tmpDoc.ItemList.Add(item29);
            */

            EyeDoc.Item item30 = new EyeDoc.Item();
            item30.Kind = "手術基本情報";
            item30.Name = "備考";
            item30.Value = this.CommentBox.Text;
            tmpDoc.ItemList.Add(item30);

            foreach (Control c in this.RecordTabControl.TabPages[0].Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "術前検査";
                    tmpItem.Name = c.Name;
                    tmpItem.Value = c.Text;
                    tmpDoc.ItemList.Add(tmpItem);
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "術前検査";
                    tmpItem.Name = c.Name;

                    if (((CheckBox)c).Checked)
                    {
                        tmpItem.Value = "1";
                    }
                    else
                    {
                        tmpItem.Value = "0";
                    }

                    tmpDoc.ItemList.Add(tmpItem);
                }
            }

            foreach (Control c in this.RecordTabControl.TabPages[1].Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "術前アナムネ";
                    tmpItem.Name = c.Name;
                    tmpItem.Value = c.Text;
                    tmpDoc.ItemList.Add(tmpItem);
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "術前アナムネ";
                    tmpItem.Name = c.Name;

                    if (((CheckBox)c).Checked)
                    {
                        tmpItem.Value = "1";
                    }
                    else
                    {
                        tmpItem.Value = "0";
                    }

                    tmpDoc.ItemList.Add(tmpItem);
                }
            }

            foreach (Control c in this.RecordTabControl.TabPages[2].Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "共通記録";
                    tmpItem.Name = c.Name;
                    tmpItem.Value = c.Text;
                    tmpDoc.ItemList.Add(tmpItem);
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "共通記録";
                    tmpItem.Name = c.Name;

                    if (((CheckBox)c).Checked)
                    {
                        tmpItem.Value = "1";
                    }
                    else
                    {
                        tmpItem.Value = "0";
                    }

                    tmpDoc.ItemList.Add(tmpItem);
                }
            }

            EyeDoc.Item sum_item0 = new EyeDoc.Item();
            sum_item0.Kind = "サマリメイン";
            sum_item0.Name = "主病名";
            sum_item0.Value = SumDiagBox.Text;
            tmpDoc.SumList.Add(sum_item0);

            EyeDoc.Item sum_item1 = new EyeDoc.Item();
            sum_item1.Kind = "サマリメイン";
            sum_item1.Name = "分類1";
            sum_item1.Value = SumKindBox1.Text;
            tmpDoc.SumList.Add(sum_item1);

            EyeDoc.Item sum_item2 = new EyeDoc.Item();
            sum_item2.Kind = "サマリメイン";
            sum_item2.Name = "分類2";
            sum_item2.Value = SumKindBox2.Text;
            tmpDoc.SumList.Add(sum_item2);

            EyeDoc.Item sum_item3 = new EyeDoc.Item();
            sum_item3.Kind = "サマリメイン";
            sum_item3.Name = "分類3";
            sum_item3.Value = SumKindBox3.Text;
            tmpDoc.SumList.Add(sum_item3);

            foreach (Control c in this.SumPanel1.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "サマリメイン";
                    tmpItem.Name = c.Tag.ToString();
                    tmpItem.Value = c.Text;
                    tmpDoc.SumList.Add(tmpItem);
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "サマリメイン";
                    tmpItem.Name = c.Tag.ToString();

                    if (((CheckBox)c).Checked)
                    {
                        tmpItem.Value = "1";
                    }
                    else
                    {
                        tmpItem.Value = "0";
                    }

                    tmpDoc.SumList.Add(tmpItem);
                }
            }

            foreach (Control c in this.SumPanel3.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "サマリ入院管理";
                    tmpItem.Name = c.Tag.ToString();
                    tmpItem.Value = c.Text;
                    tmpDoc.SumList.Add(tmpItem);
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "サマリ入院管理";
                    tmpItem.Name = c.Tag.ToString();

                    if (((CheckBox)c).Checked)
                    {
                        tmpItem.Value = "1";
                    }
                    else
                    {
                        tmpItem.Value = "0";
                    }

                    tmpDoc.SumList.Add(tmpItem);
                }
            }

            foreach (Control c in this.SumPanel4.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "サマリ経過";
                    tmpItem.Name = c.Tag.ToString();
                    tmpItem.Value = c.Text;
                    tmpDoc.SumList.Add(tmpItem);
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    EyeDoc.Item tmpItem = new EyeDoc.Item();
                    tmpItem.Kind = "サマリ経過";
                    tmpItem.Name = c.Tag.ToString();

                    if (((CheckBox)c).Checked)
                    {
                        tmpItem.Value = "1";
                    }
                    else
                    {
                        tmpItem.Value = "0";
                    }

                    tmpDoc.SumList.Add(tmpItem);
                }
            }

            EyeDoc.Item sum_item4 = new EyeDoc.Item();
            sum_item4.Kind = "サマリ経過";
            sum_item4.Name = "経過";
            sum_item4.Value = SumPassBox.Text;
            tmpDoc.SumList.Add(sum_item4);

            EyeDoc.Item sum_item5 = new EyeDoc.Item();
            sum_item5.Kind = "サマリ経過";
            sum_item5.Name = "履歴";
            sum_item5.Value = SumHistBox.Text;
            tmpDoc.SumList.Add(sum_item5);

            EyeDoc.Item sum_item6 = new EyeDoc.Item();
            sum_item6.Kind = "サマリ治療方針";
            sum_item6.Name = "方針";
            sum_item6.Value = SumPlanBox.Text;
            tmpDoc.SumList.Add(sum_item6);

            foreach (Control c in this.SumPanel2.Controls)
            {
                if (c.GetType().Name.Equals("Label"))
                {
                    if (c.Name.EndsWith("_D"))
                    {
                        EyeDoc.Item tmpItem = new EyeDoc.Item();
                        tmpItem.Kind = "サマリ治療方針";
                        tmpItem.Name = c.Name;
                        tmpItem.Value = c.Text;
                        tmpDoc.SumList.Add(tmpItem);
                    }
                    else if (c.Name.EndsWith("_S"))
                    {
                        EyeDoc.Item tmpItem = new EyeDoc.Item();
                        tmpItem.Kind = "サマリ治療方針";
                        tmpItem.Name = c.Name;
                        tmpItem.Value = c.Text;
                        tmpDoc.SumList.Add(tmpItem);
                    }
                }
            }

            tmpDoc.ExcelOpen();
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
//            MessageBox.Show("電子カルテから次のデータを取り込みます。\r\n\r\n　・感染症\r\n　・患者情報の「短期入院」の身長\r\n　・患者情報の「短期入院」の体重\r\n\r\n※ただし先に入力欄に入力されていれば取り込まれません。");

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

            /*
                        int i = 0;

                        if (this.PtIdBox.Text.Length > 0 && Int32.TryParse(this.PtIdBox.Text, out i))
                        {
                            // 感染症のデータ取り込み
                            if (this.InfectionBox.Text.Length == 0)
                            {
                                this.InfectionBox.Text = AgentlabUtilityLibrary.Pat.GetInfection(this.PtIdBox.Text);
                            }

                            // 身長のデータ取り込み
                            if (this.HeightBox.Text.Length == 0)
                            {
                                this.HeightBox.Text = AppString.ZenToHan(AgentlabUtilityLibrary.Pat.GetPatInfo(this.PtIdBox.Text, AgentlabUtilityLibrary.Pat.InfoKind.SHORT_HEIGHT));
                            }

                            // 体重のデータ取り込み
                            if (this.WeightBox.Text.Length == 0)
                            {
                                this.WeightBox.Text = AppString.ZenToHan(AgentlabUtilityLibrary.Pat.GetPatInfo(this.PtIdBox.Text, AgentlabUtilityLibrary.Pat.InfoKind.SHORT_WEIGHT));
                            }

                            // 体表面積・ビスダイン溶液・ブドウ糖液の量を計算する。
                            BodyCalc();
                        }
             */
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

        private void AllDeptBox_CheckedChanged(object sender, EventArgs e)
        {
            this.PtComeHistoryFormat();
        }

        private void TermBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.PtComeHistoryShow();
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

            if (this.EyeBoxR.Checked)
            {
                ope.EyeR = "1";
            }

            if (this.EyeBoxL.Checked)
            {
                ope.EyeL = "1";
            }

            ope.Height = this.HeightBox.Text;
            ope.Weight = this.WeightBox.Text;
            ope.Infection = this.InfectionBox.Text;
            ope.PostDeal = this.PostDealBox.Text;
            ope.Past = this.PastBox.Text;
            ope.Comment = this.CommentBox.Text;

            if (this.AllCheckBox.Checked)
            {
                ope.AllCheck = "1";
            }

            if (this.ExplainBox.Checked)
            {
                ope.Explain = "1";
            }

            if (this.EyeDropBox.Checked)
            {
                ope.EyeDrop = "1";
            }

            if (this.AgreeBox.Checked)
            {
                ope.Agree = "1";
            }

            if (this.PreCheckBox.Checked)
            {
                ope.PreCheck = "1";
            }

            if (this.EarlierOKBox.Checked)
            {
                ope.EarlierOK = "1";
            }

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

            string cont = "";

            foreach (TabPage p in RecordTabControl.TabPages)
            {
                foreach (Control c in p.Controls)
                {
                    if ((c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox")) && c.Text.Length > 0 && c.Tag.ToString().Length > 0)
                    {
                        if (cont.Length > 0)
                        {
                            cont += "\r\n";
                        }

                        cont += c.Tag.ToString() + "," + c.Text.Replace("\r\n", "<CR+LF>");
                    }
                    else if (c.GetType().Name.Equals("CheckBox") && ((CheckBox)c).Checked)
                    {
                        if (cont.Length > 0)
                        {
                            cont += "\r\n";
                        }

                        cont += c.Tag.ToString() + ",1";
                    }
                }
            }

            ope.Cont = cont;
            ope.Staff = LoginUser.Id;

            if (RecordStatusBox.Checked)
            {
                ope.Status = "1";
            }
            else
            {
                ope.Status = "2";
            }

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

            if (DoctorStatusBox.Checked)
            {
                ope.Status = "1";
            }
            else
            {
                ope.Status = "2";
            }

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

            string cont = "";

            foreach (Control c in PassPanel.Controls)
            {
                if ((c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox")) && c.Text.Length > 0 && c.Tag.ToString().Length > 0)
                {
                    if (cont.Length > 0)
                    {
                        cont += "\r\n";
                    }

                    cont += c.Tag.ToString() + "," + c.Text.Replace("\r\n", "<CR+LF>");
                }
            }

            ope.Cont = cont;
            ope.Staff = LoginUser.Id;
            ope.Status = "1";

            ope.Save();

            MessageBox.Show("保存しました");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void ListButton_Click(object sender, EventArgs e)
        {
            FormControl.FormList_Show();
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

        private void MacsButton_Click(object sender, EventArgs e)
        {
            if (!MacsProgram.KarteShow(this.Pat.Id))
            {
                MessageBox.Show("カルテが起動していません");
            }
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
                // 来院歴を非表示にする
                ComeHistoryLabel.Visible = false;
                AllDeptBox.Visible = false;
                TermBox.Visible = false;

                // 入院歴を非表示にする
                InHistoryLabel.Visible = false;

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
                // 来院歴を非表示にする
                ComeHistoryLabel.Visible = false;
                AllDeptBox.Visible = false;
                TermBox.Visible = false;

                // 入院歴を非表示にする
                InHistoryLabel.Visible = false;

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
                // 来院歴を表示する
                ComeHistoryLabel.Visible = true;
                AllDeptBox.Visible = true;
                TermBox.Visible = true;

                // 入院歴を表示する
                InHistoryLabel.Visible = true;

                // 手術歴を表示する
                OpeHistoryLabel.Location = new Point(389, 35);
                OpeClearButton.Location = new Point(440, 29);
                OpeHistoryView.Location = new Point(389, 50);
                OpeHistoryView.Width = 275;
                OpeHistoryLabel.Visible = true;
                OpeClearButton.Visible = true;
                OpeWideBox.Visible = true;

                // 検査歴の表示する
                KensaHistoryLabel.Location = new Point(667, 35);
                KensaClearButton.Location = new Point(720, 29);
                KensaHistoryView.Location = new Point(667, 50);
                KensaHistoryView.Width = this.Width - 684;
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
                this.KensaDate.Value = DateTime.Parse(kensa_date);

                // KensaDate の値が変われば自動的に KensaShow() が呼ばれるため削除
//                this.KensaShow(this.Pat.Id, kensa_date.Replace("/", ""));

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

        private void IntroBox_DoubleClick(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0)
            {
                List<Intro> introList = Intro.GetList(this.Pat.Id);

                string intro = "";

                foreach (Intro p in introList)
                {
                    intro += p.IntroDate.Insert(4, "/").Insert(7, "/");

                    if (p.Kind.Equals("1"))
                    {
                        intro += " 紹介";
                    }
                    else if (p.Kind.Equals("2"))
                    {
                        intro += " 返事（眼科）";
                    }
                    else if (p.Kind.Equals("3"))
                    {
                        intro += " 依頼";
                    }
                    else if (p.Kind.Equals("4"))
                    {
                        intro += " 返信Fax";
                    }
                    else if (p.Kind.Equals("5"))
                    {
                        intro += " 返事（途中）";
                    }
                    else if (p.Kind.Equals("6"))
                    {
                        intro += " 返事（最終）";
                    }

                    intro += "　" + p.Hospital + " " + p.DeptTo + " " + p.DoctorTo + " 先生（差出人　" + p.DeptFromName + " " + p.DoctorFromName + "）\r\n";
                }

                FormString1 f1 = new FormString1("紹介状", "過去の紹介状", intro);
                f1.Size = new Size(500, 200);
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
                            if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                            {
                                if (kensa_dict.ContainsKey(c.Tag.ToString()))
                                {
                                    c.Text = kensa_dict[c.Tag.ToString()];
                                }
                            }
                            else if (c.GetType().Name.Equals("CheckBox"))
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

        private void KensaDate_ValueChanged(object sender, EventArgs e)
        {
            if (this.KensaDate.Value.ToString("yyyyMMdd").Equals(this._KensaDate))
            {
                return;
            }

            bool b = true;

            if (this.KensaEdited)
            {
                // 実装困難なため外す 2018/08/17
                /*
                if (MessageBox.Show("検査データの値が変更されている箇所があります。続けますか？", "確認", MessageBoxButtons.OKCancel) != DialogResult.OK)
                {
                    b = false;
                }
                 */
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