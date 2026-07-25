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

        // ��p���E�������̉����i�ݒ�t�@�C�� EyeCenter.exe.config �ŕύX�\�B�������� 0 �͉�ʉE�[�܂Ŏ��������j
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
        /// �o�ߋL�^�̃e�L�X�g�i�p�O, 6M ���j�ƃR���g���[�����iPre, M6 ���j�̑Ή�
        /// </summary>
        Dictionary<string, string> passDict = new Dictionary<string, string>();

        /// <summary>
        /// �|�b�v�A�b�v�w���v��\������c�[���`�b�v�B
        /// </summary>
        ToolTip passTip;

        /// <summary>
        /// ��ʕҏW���[�h
        /// </summary>
        public enum Mode : int
        {
            NEW = 0,
            SHOW = 1
        }

        /// <summary>
        /// �^�u���
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
        /// �Y�����҂̋L�^���J���ꍇ�B
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
        /// �����L�^���J���ꍇ�B
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

            // record_id �������s��I������
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
        /// �\���V�K���́i��p�L�^��V�K�쐬�j����ꍇ�B
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
            // ��f���́E��f�����̉�����ݒ�t�@�C��(EyeCenter.exe.config)���甽�f����
            int ivContWidth = AppConfig.GetInt("IVContBoxWidth", this.IVContBox.Width);
            int ivHistWidth = AppConfig.GetInt("IVHistoryViewWidth", this.IVHistoryView.Width);
            this.IVContBox.Width = ivContWidth;
            this.label53.Left = this.IVContBox.Left + ivContWidth + 8;
            this.IVHistoryView.Left = this.IVContBox.Left + ivContWidth + 8;
            this.IVHistoryView.Width = ivHistWidth;
            this.IVContBox.MaxLength = 1999;

            // ��p���𗓂̉����E������ݒ�t�@�C��(EyeCenter.exe.config)���甽�f����
            int sumHistWidth = AppConfig.GetInt("SumHistBoxWidth", this.SumHistBox.Width);
            this.SumHistBox.Width = sumHistWidth;
            this.SumHistBox.Height = AppConfig.GetInt("SumHistBoxHeight", this.SumHistBox.Height);
            this.SumPanel4.Left = this.SumHistBox.Left + sumHistWidth + 8;

            // ��p��{���E��p�L�^��ʂ̉����E������ݒ�t�@�C��(EyeCenter.exe.config)���甽�f����
            this.OpeInfoPanel.Width = AppConfig.GetInt("OpeInfoPanelWidth", this.OpeInfoPanel.Width);
            this.OpeInfoPanel.Height = AppConfig.GetInt("OpeInfoPanelHeight", this.OpeInfoPanel.Height);
            this.OpeDoctorPanel.Left = this.OpeInfoPanel.Right + 1;

            this.OpeRecordPanel.Top = this.OpeInfoPanel.Bottom - 1;
            this.OpeRecordPanel.Width = AppConfig.GetInt("OpeRecordPanelWidth", this.OpeRecordPanel.Width);
            this.OpeRecordPanel.Height = AppConfig.GetInt("OpeRecordPanelHeight", this.OpeRecordPanel.Height);
            this.RecordTabControl.Width = this.OpeRecordPanel.Width - 10;
            this.OpePassPanel.Left = this.OpeRecordPanel.Right + 1;
            this.OpePassPanel.Top = this.OpeRecordPanel.Top;

            // ��p���E�������̉����ݒ�������\���ɔ��f����
            this.PtHistoryWide();

            if (!InnoProgram.Exists)
            {
                this.InnoButton.Enabled = false;
            }

            outcomeDict.Add("0", "");
            outcomeDict.Add("1", "����");
            outcomeDict.Add("2", "���S");
            outcomeDict.Add("3", "���~");
            outcomeDict.Add("4", "�]��");
            outcomeDict.Add("5", "�y��");
            outcomeDict.Add("6", "�]�@");
            outcomeDict.Add("7", "�ꎞ�މ@");
            outcomeDict.Add("8", "�s��");

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
        /// ���̃T�C�Y�ɖ߂��B
        /// �����E�����͐ݒ�t�@�C��(EyeCenter.exe.config)�� PatFormWidth / PatFormHeight �ŕύX�\�B
        /// ���ݒ莞�͏]���ǂ���i��1280 or 1024, �c�ő�j�B
        /// </summary>
        public void OrgSize()
        {
            this.Height = AppConfig.GetInt("PatFormHeight", Screen.PrimaryScreen.WorkingArea.Height);

            int defWidth = Screen.PrimaryScreen.WorkingArea.Width >= 1280 ? 1280 : 1024;
            this.Width = AppConfig.GetInt("PatFormWidth", defWidth);
            this.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - this.Width) / 2, 0);
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
        /// dSet �������B��p�����e�[�u���̃J�������쐬����B
        /// �R���X�g���N�^�ȊO�ł͎g�p���Ȃ��B
        /// </summary>
        private void DSetInit()
        {
            DataTable tmpTable = dSet.Tables.Add("��p����");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("OPE_DATE");
            tmpTable.Columns.Add("��p��");
            tmpTable.Columns.Add("OPE_TIME");
            tmpTable.Columns.Add("����");
            tmpTable.Columns.Add("OPE_KIND");
            tmpTable.Columns.Add("���");
            tmpTable.Columns.Add("��p��");
            tmpTable.Columns.Add("��p");
            tmpTable.Columns.Add("��t");
            tmpTable.Columns.Add("����");
            tmpTable.Columns.Add("�a��");
            tmpTable.Columns.Add("���O");
            tmpTable.Columns.Add("�N��");
            tmpTable.Columns.Add("��");
            tmpTable.Columns.Add("����");
            tmpTable.Columns.Add("�֊�");
            tmpTable.Columns.Add("���l");

            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
            {
                tmpTable.Columns.Add(r["Text"].ToString());
            }

            tmpTable = dSet.Tables.Add("������");
            tmpTable.Columns.Add("KENSA_DATE");
            tmpTable.Columns.Add("������");

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

            // �Ԗ���
            tmpTable.Columns.Add("Moumakukou_R");
            tmpTable.Columns.Add("Moumakukou_L");
        }

        /// <summary>
        /// �^�u�؂�ւ�
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
        /// ��p��{�����������B
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
        /// �����e�[�u���� Value ��ŃR���{�{�b�N�X�̍��ڂ���蒼���i�擪�͋󗓁j�B
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
        /// ��p�L�^�S�̂��N���A�B
        /// </summary>
        private void AllOpeClear()
        {
            this.OpeClear();
            this.RecordClear();
            this.DoctorClear();
            this.PassClear();
        }

        /// <summary>
        /// ��p��{���̂��ׂĂ��N���A�B
        /// </summary>
        private void OpeClear()
        {
            OpeDateTimePicker.Value = DateTime.Now;
            OpeKindBox.Text = "";
            OpeTimeBox.Text = "";

            this.OpeClear_Wo_KindDateTime();
        }

        /// <summary>
        /// ��p��{���̓��e���N���A�B
        /// ��������ʁE�����̓N���A���Ȃ��B
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
        /// ��p�L�^�^�u�̓��e�N���A
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
        /// ��t�L�^�̓��e�N���A
        /// </summary>
        private void DoctorClear()
        {
            PreContBox.Clear();
            DoContBox.Clear();
            DoctorStatusBox.Checked = false;
            DoctorStaffLabel.Text = "";
        }

        /// <summary>
        /// �o�ߋL�^�p�l���̓��e�N���A
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
        /// ��p���Ǝ�ʂɉ����Ď��Ԙg�R���{�{�b�N�X��ύX����B
        /// �ŏ���������Ă���l�͏������Ȃ��B
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

            // �o�ߋL�^�p�l���̓��t��ύX����B
            this.PassPanelControlDateChange();
        }

        private void InOutBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.InRoomBoxChange();
            this.InDateChange();
        }

        /// <summary>
        /// ���O�̕ύX�ɉ����ē��@���R���{�{�b�N�X��ύX����B
        /// </summary>
        private void InDateChange()
        {
            InDateTimePicker.Enabled = InOutBox.Text.Equals("�킩��") || InOutBox.Text.Equals("������") || InOutBox.Text.Equals("�����");
        }

        /// <summary>
        /// ���O�̕ύX�ɉ����ĕa���R���{�{�b�N�X��ύX����B
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
        /// ���ҏ��iID, ����, ��p�����j��\������
        /// </summary>
        private void PtShow()
        {
            if (this.Pat.Id.Length == 0)
            {
                return;
            }

            this.Text = this.Pat.Name;

            // �`�B��񂪑��݂���΁u�`�B�v�{�^�����Ԃ��Ȃ�B����ȊO�͉��F�B
            Memo tmpInfo = Memo.Load(this.Pat.Id, "0");

            if (tmpInfo.Cont.Length > 0)
            {
                this.InfoShareButton.BackColor = Color.FromArgb(255, 192, 192);
            }
            else
            {
                this.InfoShareButton.BackColor = Color.FromArgb(255, 255, 192);
            }

            // �Ƒ��A�����񂪑��݂���΁u�Ƒ��v�{�^�����Ԃ��Ȃ�B����ȊO�͉��F�B
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
                            con += "�i" + c.RelationComment + "�j";
                        }
                    }

                    if (c.Tel1.Length > 0)
                    {
                        con += ", " + c.Tel1;

                        if (c.KindVal1.Length > 0)
                        {
                            con += "�i" + c.KindVal1 + "�j";
                        }
                    }

                    if (c.Tel2.Length > 0)
                    {
                        con += ", " + c.Tel2;

                        if (c.KindVal2.Length > 0)
                        {
                            con += "�i" + c.KindVal2 + "�j";
                        }
                    }

                    if (c.Tel3.Length > 0)
                    {
                        con += ", " + c.Tel3;

                        if (c.KindVal3.Length > 0)
                        {
                            con += "�i" + c.KindVal3 + "�j";
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

            // �֊������擾
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

            // �����f�[�^��\��
            KensaShow(this.Pat.Id, KensaDate.Value.ToString("yyyyMMdd"));
        }

        /// <summary>
        /// ��p����\������
        /// </summary>
        private void PtOpeHistoryShow()
        {
            DataTable tmpTable = dSet.Tables["��p����"];
            tmpTable.Rows.Clear();

            List<EyeOpe> tmpList = EyeOpe.GetListByPatDates(this.Pat.Id, "", "");
            Dictionary<string, string> tmpRecDict;

            foreach (EyeOpe tmpOpe in tmpList)
            {
                DataRow r = tmpTable.NewRow();

                r["ID"] = tmpOpe.Id;
                r["OPE_DATE"] = tmpOpe.OpeDate;
                r["��p��"] = DateTimeAgent.DateFormat(tmpOpe.OpeDate, DateTimeAgent.DateFormatKind.SHORT);
                r["OPE_TIME"] = tmpOpe.OpeTime;
                r["����"] = tmpOpe.OpeTime.PadLeft(4, '0').Insert(2, ":");

                r["OPE_KIND"] = tmpOpe.OpeKind;

                if (EyeDict.OpeKindDict.ContainsKey(tmpOpe.OpeKind))
                {
                    r["���"] = EyeDict.OpeKindDict[tmpOpe.OpeKind];
                }
                else
                {
                    r["���"] = "";
                }

                r["��p��"] = tmpOpe.OpeRoom;
                r["��p"] = tmpOpe.OpeName;
                r["��t"] = tmpOpe.Doctor;
                r["����"] = tmpOpe.Anes;
                r["�a��"] = tmpOpe.Diag;
                r["���O"] = tmpOpe.InOut;

                r["�N��"] = tmpOpe.Pat.AgeCalc(tmpOpe.OpeDate);

                if (tmpOpe.EyeR.Equals("1") && tmpOpe.EyeL.Equals("1"))
                {
                    r["��"] = "B";
                }
                else if (tmpOpe.EyeR.Equals("1"))
                {
                    r["��"] = "R";
                }
                else if (tmpOpe.EyeL.Equals("1"))
                {
                    r["��"] = "L";
                }
                else
                {
                    r["��"] = "";
                }

                if (tmpOpe.Infection.Contains("+"))
                {
                    r["����"] = "+";
                }
                else
                {
                    r["����"] = "-";
                }

                if (tmpOpe.Agree.Equals("1"))
                {
                    r["�֊�"] = "��";
                }
                else
                {
                    r["�֊�"] = "";
                }

                r["���l"] = tmpOpe.Comment;

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
        /// OpeHistoryView �̑傫���ɉ����ăJ�����̕\���E��\���ƕ��𒲐�����B
        /// </summary>
        void PtOpeHistoryWide()
        {
            if (OpeHistoryView.ColumnCount > 0)
            {
                if (OpeHistoryView.Width > 300)
                {
                    OpeHistoryView.Columns["ID"].Visible = false;

                    OpeHistoryView.Columns["OPE_DATE"].Visible = false;

                    OpeHistoryView.Columns["��p��"].Width = 55;
                    OpeHistoryView.Columns["��p��"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["OPE_TIME"].Visible = false;

                    OpeHistoryView.Columns["����"].Visible = false;

                    OpeHistoryView.Columns["OPE_KIND"].Visible = false;

                    OpeHistoryView.Columns["���"].Visible = false;

                    OpeHistoryView.Columns["��p��"].Visible = false;

                    OpeHistoryView.Columns["��p"].Width = 220;

                    OpeHistoryView.Columns["��t"].HeaderText = "Dr";
                    OpeHistoryView.Columns["��t"].Width = 55;

                    OpeHistoryView.Columns["����"].Visible = false;

                    OpeHistoryView.Columns["�a��"].Visible = false;

                    OpeHistoryView.Columns["���O"].Visible = false;

                    OpeHistoryView.Columns["�N��"].Visible = false;

                    OpeHistoryView.Columns["��"].Width = 25;
                    OpeHistoryView.Columns["��"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["����"].Visible = false;

                    OpeHistoryView.Columns["�֊�"].Visible = false;

                    OpeHistoryView.Columns["���l"].Visible = false;

                    int width = 0;

                    foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
                    {
                        string colText = tmpRow["Text"].ToString();

                        // �ی��p���E���{�a���E��p���R�͕\�����Ȃ�
                        if (colText.Equals("�ی��p��") || colText.Equals("���{�a��") || colText.Equals("��p���R"))
                        {
                            OpeHistoryView.Columns[colText].Visible = false;
                            continue;
                        }

                        if (tmpRow["Width"].ToString().Length > 0 && int.TryParse(tmpRow["Width"].ToString(), out width))
                        {
                            OpeHistoryView.Columns[colText].Visible = true;
                            OpeHistoryView.Columns[colText].Width = width;
                        }
                    }
                }
                else
                {
                    OpeHistoryView.Columns["ID"].Visible = false;

                    OpeHistoryView.Columns["OPE_DATE"].Visible = false;

                    OpeHistoryView.Columns["��p��"].Width = 55;
                    OpeHistoryView.Columns["��p��"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["OPE_TIME"].Visible = false;

                    OpeHistoryView.Columns["����"].Visible = false;

                    OpeHistoryView.Columns["OPE_KIND"].Visible = false;

                    OpeHistoryView.Columns["���"].Visible = false;

                    OpeHistoryView.Columns["��p��"].Visible = false;

                    OpeHistoryView.Columns["��p"].Width = 145;

                    OpeHistoryView.Columns["��t"].HeaderText = "Dr";
                    OpeHistoryView.Columns["��t"].Width = 30;

                    OpeHistoryView.Columns["����"].Visible = false;

                    OpeHistoryView.Columns["�a��"].Visible = false;

                    OpeHistoryView.Columns["���O"].Visible = false;

                    OpeHistoryView.Columns["�N��"].Visible = false;

                    OpeHistoryView.Columns["��"].Width = 25;
                    OpeHistoryView.Columns["��"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                    OpeHistoryView.Columns["����"].Visible = false;

                    OpeHistoryView.Columns["�֊�"].Visible = false;

                    OpeHistoryView.Columns["���l"].Visible = false;

                    foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["OpeHistory"].Rows)
                    {
                        OpeHistoryView.Columns[tmpRow["Text"].ToString()].Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// ��p��{���̓��e��\������B
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
        /// ��p��{���̓��e��\������B
        /// ������ID�E��p���E���@���E�X�^�b�t�͐ݒ肵�Ȃ��B
        /// </summary>
        /// <param name="ope"></param>
        private void OpeBaseShow(EyeOpe ope)
        {
            if (EyeDict.OpeKindDict.ContainsKey(ope.OpeKind))
            {
                this.OpeKindBox.Text = ope.OpeKind + " " + EyeDict.OpeKindDict[ope.OpeKind];
            }

            // ���̏����� OpeDateTimePicker �� OpeKindBox �̌�ɂ��Ȃ���΂Ȃ�Ȃ��B�iOpeTimeBox ���N���A����Ă��܂����߁j
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

            // �g���E�̏d�̒l����A�̕\�ʐρE�r�X�_�C���n�t�E�u�h�E���t�̗ʂ��v�Z����B
            this.BodyCalc();
        }

        /// <summary>
        /// ��p�L�^�̓��e��\������B
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
        /// ��t�L�^�̓��e��\������B
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
        /// �o�ߋL�^�̓��e��\������B
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
        /// ��ʕҏW���[�h�ύX�B
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
        /// �d�q�J���e����f�[�^����荞�ށB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetKarteDataButton_Click(object sender, EventArgs e)
        {
            // ������
            this.InfectionBox.Text = InfectionData.GetInfectionData(this.Pat.Id).ResultString;

            // ���Ҋ�{���
            Dictionary<string, List<BaseInfo>> dict = BaseInfo.GetDict(this.Pat.Id);

            // �g��
            if (dict.ContainsKey(LibSettings.Current.BaseInfoCodes.Height))
            {
                this.HeightBox.Text = AppString.ZenToHan(dict[LibSettings.Current.BaseInfoCodes.Height][0].Value);
            }

            // �̏d
            if (dict.ContainsKey(LibSettings.Current.BaseInfoCodes.Weight))
            {
                this.WeightBox.Text = AppString.ZenToHan(dict[LibSettings.Current.BaseInfoCodes.Weight][0].Value);
            }

            // �̕\�ʐρE�r�X�_�C���n�t�E�u�h�E���t�̗ʂ��v�Z����B
            BodyCalc();
        }

        /// <summary>
        /// ���͂��ꂽ�g���E�̏d����A�̕\�ʐρE�r�X�_�C���n�t�E�u�h�E���t�EDM�P�ʂ��v�Z����B
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
        /// ��p�L�^�̐V�K�쐬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryNewMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("��p�L�^��V�K�쐬���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                this.TabChange(Tab.OPE);
                this.AllOpeClear();
            }
        }

        /// <summary>
        /// �I�����ꂽ��p�L�^�̊�{�����R�s�[���ĐV�K�쐬����B
        /// ��p���͖{���̓��t�Ƃ���B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryCopyMenuItem_Click(object sender, EventArgs e)
        {
            if (OpeHistoryView.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("�I�����ꂽ��p�L�^�̊�{�����R�s�[���ĐV�K�쐬���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    EyeOpe tmpOpe = EyeOpe.Load(OpeHistoryView.SelectedRows[0].Cells["ID"].Value.ToString());

                    this.TabChange(Tab.OPE);

                    // AllOpeClear �� ID ���N���A����A��p�����{���ɂȂ�B
                    this.AllOpeClear();

                    // ���@���͈����p�����{���Ƀ��Z�b�g����B
                    this.InDateTimePicker.Value = DateTime.Now;

                    this.OpeBaseShow(tmpOpe);
                }
            }
        }

        /// <summary>
        /// ��p�L�^�̐V�K�쐬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeClearButton_Click(object sender, EventArgs e)
        {
            this.TabChange(Tab.OPE);
            this.AllOpeClear();
        }

        /// <summary>
        /// ��p�L�^���J��
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
        /// ��p�L�^���J��
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            if (LoginUser.IsDoctor || MessageBox.Show("�N���b�N���ꂽ�L�^���J���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                EyeOpe tmpOpe = EyeOpe.Load(OpeHistoryView.Rows[e.RowIndex].Cells["ID"].Value.ToString());
                this.OpeShow(tmpOpe);
                this.TabChange(Tab.OPE);
            }
        }

        /// <summary>
        /// ��p�L�^���폜����
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpeHistoryDeleteMenuItem_Click(object sender, EventArgs e)
        {
            if (OpeHistoryView.SelectedRows.Count > 0)
            {
                if (MessageBox.Show("�폜���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1) == DialogResult.OK)
                {
                    EyeOpe.Delete(OpeHistoryView.SelectedRows[0].Cells["ID"].Value.ToString(), LoginUser.Id);
                    MessageBox.Show("�폜���܂���");

                    this.AllOpeClear();
                    this.PtOpeHistoryShow();
                }
            }
        }

        private void OpeRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("����ID����͂��Ă�������");
                return;
            }

            int time = 0;

            if (this.PlanTimeBox.Text.Length > 0)
            {
                if (!int.TryParse(this.PlanTimeBox.Text, out time))
                {
                    MessageBox.Show("���Ԃɂ͐��l����͂��Ă�������");
                    this.PlanTimeBox.Focus();
                    return;
                }
            }

            if (this.OpeTimeBox.Text.Length > 0)
            {
                if (!int.TryParse(this.OpeTimeBox.Text, out time))
                {
                    MessageBox.Show("�����ɂ͐��l����͂��Ă�������");
                    this.OpeTimeBox.Focus();
                    return;
                }
            }

            if (this.InTimeBox.Text.Length > 0)
            {
                if (!int.TryParse(this.InTimeBox.Text, out time))
                {
                    MessageBox.Show("�����ɂ͐��l����͂��Ă�������");
                    this.InTimeBox.Focus();
                    return;
                }
            }

            foreach (DataGridViewRow r in OpeHistoryView.Rows)
            {
                if (r.Cells["OPE_DATE"].Value.ToString().Equals(this.OpeDateTimePicker.Value.ToString("yyyyMMdd")))
                {
                    DialogResult result = MessageBox.Show("���ɓ����Ɏ�p�o�^������Ă��܂��B�o�^���܂����H\r\n�@�͂�(Y) �c �o�^����\r\n�@������(N) �c �o�^���Ȃ��{��ʃN���A\r\n�@�L�����Z�� �c �o�^���Ȃ�", "�m�F", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button3);

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

            if (!ope.InOut.Contains("�O��"))
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

            MessageBox.Show("�ۑ����܂���");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void RecordRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("����ID����͂��Ă�������");
                return;
            }

            if (this.OpeIdBox.Text.Length == 0)
            {
                MessageBox.Show("��Ɋ�{����ۑ����Ă�������");
                return;
            }

            EyeOpeRecord ope = new EyeOpeRecord();

            ope.Id = this.OpeIdBox.Text;

            ope.Cont = ContData.Build(RecordTabControl.TabPages);
            ope.Staff = LoginUser.Id;
            ope.Status = RecordStatusBox.Checked ? "1" : "2";

            ope.Save();

            MessageBox.Show("�ۑ����܂���");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void DoctorRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("����ID����͂��Ă�������");
                return;
            }

            if (this.OpeIdBox.Text.Length == 0)
            {
                MessageBox.Show("��Ɋ�{����ۑ����Ă�������");
                return;
            }

            EyeOpeDoctor ope = new EyeOpeDoctor();

            ope.Id = this.OpeIdBox.Text;
            ope.PreCont = PreContBox.Text;
            ope.DoCont = DoContBox.Text;
            ope.Staff = LoginUser.Id;

            ope.Status = DoctorStatusBox.Checked ? "1" : "2";

            ope.Save();

            MessageBox.Show("�ۑ����܂���");

            this.AllOpeClear();
            this.PtOpeHistoryShow();
        }

        private void PassRegButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length == 0)
            {
                MessageBox.Show("����ID����͂��Ă�������");
                return;
            }

            if (this.OpeIdBox.Text.Length == 0)
            {
                MessageBox.Show("��Ɋ�{����ۑ����Ă�������");
                return;
            }

            EyeOpePass ope = new EyeOpePass();

            ope.Id = this.OpeIdBox.Text;

            ope.Cont = ContData.Build(PassPanel.Controls);
            ope.Staff = LoginUser.Id;
            ope.Status = "1";

            ope.Save();

            MessageBox.Show("�ۑ����܂���");

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
                MessageBox.Show("�J���e���N�����Ă��܂���");
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
                // ��p�����\���ɂ���
                OpeHistoryLabel.Visible = false;
                OpeClearButton.Visible = false;
                OpeWideBox.Visible = false;

                // �������̕\���ʒu��ύX����
                KensaHistoryLabel.Location = new Point(3, 35);
                KensaClearButton.Location = new Point(60, 29);
                ReloadButton.Location = new Point(151, 29);
                KensaWideBox.Location = new Point(227, 33);
                KensaHistoryView.Location = new Point(3, 50);
                KensaHistoryView.Width = this.Width - 20;
                KensaHistoryLabel.Visible = true;
                KensaClearButton.Visible = true;
                KensaHistoryView.Visible = true;
                KensaWideBox.Visible = true;
            }
            else if (OpeWideBox.Checked)
            {
                // ��p���̕\���ʒu��ύX����
                OpeHistoryLabel.Location = new Point(3, 35);
                OpeClearButton.Location = new Point(60, 29);
                OpeHistoryView.Location = new Point(3, 50);
                OpeHistoryView.Width = this.Width - 20;
                OpeHistoryLabel.Visible = true;
                OpeClearButton.Visible = true;
                OpeWideBox.Visible = true;

                // ���������\���ɂ���
                KensaHistoryLabel.Visible = false;
                KensaClearButton.Visible = false;
                KensaHistoryView.Visible = false;
                KensaWideBox.Visible = false;
            }
            else
            {
                // ��p����\������
                OpeHistoryLabel.Location = new Point(3, 35);
                OpeClearButton.Location = new Point(54, 29);
                OpeHistoryView.Location = new Point(3, 50);
                OpeHistoryView.Width = this._OpeHistoryWidth;
                OpeHistoryLabel.Visible = true;
                OpeClearButton.Visible = true;
                OpeWideBox.Visible = true;

                // �������̕\������
                // �ēǍ��{�^���E�\�����`�F�b�N����p���̉����ɍ��킹�ĉE�ւ��炷�i�d�Ȃ�h�~�j
                KensaHistoryLabel.Location = new Point(this._OpeHistoryWidth + 6, 35);
                KensaClearButton.Location = new Point(this._OpeHistoryWidth + 59, 29);
                ReloadButton.Location = new Point(this._OpeHistoryWidth + 150, 29);
                KensaWideBox.Location = new Point(this._OpeHistoryWidth + 226, 33);
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
        /// �����f�[�^���J���B
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

            if (LoginUser.IsDoctor || MessageBox.Show("�N���b�N���ꂽ���̋L�^���J���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                // KensaDate �̒l���ς��Ύ����I�� KensaShow() ���Ă΂��
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
        /// �����f�[�^��\������B
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
        /// KensaHistoryView �� _KensaDate �ɊY��������t������ΑI������
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
        /// �E���̓f�[�^���N���b�v�{�[�h�ɃR�s�[����B
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
        /// �����̓f�[�^���N���b�v�{�[�h�ɃR�s�[����B
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
        /// �����̓f�[�^���N���b�v�{�[�h�ɃR�s�[����B
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
        /// �����́E�ሳ�E�Ԗ����f�[�^���N���b�v�{�[�h�ɃR�s�[����B
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
        /// �����L�^�̐V�K�쐬
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void KensaClearButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("�������ʂ�V�K�쐬���܂����H", "�m�F", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.OK)
            {
                this.TabChange(Tab.KENSA);
                this.AllKensaClear();
            }
        }

        /// <summary>
        /// �����^�u�̃N���A
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
                FormString1 f1 = new FormString1("�Ƒ��A����", "�Ƒ��A����", ((Button)sender).Tag.ToString());
                f1.ShowDialog();
            }
        }

        private void AllergyButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0)
            {
                FormString1 f1 = new FormString1("�֊�", "�֊�", ((Button)sender).Tag.ToString());
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
                MessageBox.Show("�o�^���܂���");
            }
        }

        private void IVRegButton_Click(object sender, EventArgs e)
        {
            if (this._IVPage.Save(this.Pat.Id))
            {
                MessageBox.Show("�o�^���܂���");
                this._IVPage.HistoryShow(this.Pat.Id);
            }
        }

        private void IVDeleteButton_Click(object sender, EventArgs e)
        {
            if (this.Pat.Id.Length > 0 && this.IVIdBox.Text.Length > 0)
            {
                if (MessageBox.Show("��f���폜���܂����H", "�m�F", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    if (this._IVPage.Delete(this.IVIdBox.Text))
                    {
                        MessageBox.Show("�폜���܂���");
                        this._IVPage.HistoryShow(this.Pat.Id);
                    }
                }
            }
        }

        private void IVHistoryView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridViewRow r = IVHistoryView.Rows[e.RowIndex];

            IVIdBox.Text = r.Cells["ID"].Value.ToString();
            IVDate.Value = DateTime.Parse(r.Cells["IV_DATE"].Value.ToString().Insert(4, "/").Insert(7, "/"));
            IVContBox.Text = r.Cells["���e"].Value.ToString();

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
            if (MessageBox.Show("���̌����\�肪���ׂăN���A����܂��B��낵���ł����H", "�m�F", MessageBoxButtons.OK) == DialogResult.OK)
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
                string cont = IVHistoryView.SelectedRows[0].Cells["���e"].Value.ToString();

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
                // �ύX�m�F�_�C�A���O�͎�������Ȃ��ߊO�� 2018/08/17
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