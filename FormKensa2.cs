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
    public partial class FormKensa2 : Form
    {
        string PtId = "";
        string KensaId = "";
        string KensaDate = "";

        DataRow KensaPageRow;
        DataRow KensaMultiRow;
        List<EyeKensa2> KensaList;

        int panel_width = 0;
        int panel_height = 0;

        public FormKensa2(string pt_id, string kensa_id, string kensa_date)
        {
            InitializeComponent();

            PtId = pt_id;
            KensaId = kensa_id;
            KensaDate = kensa_date;
        }

        private void FormKensa2_Load(object sender, EventArgs e)
        {
            KensaPageRow = EyeDict.EyeSet.Tables["KensaPage"].Select("ID = '" + KensaId + "'")[0];

            this.Text += " " + KensaPageRow["Text"].ToString();
            this.DescLabel.Text = "検査日 : " + KensaDate.Insert(4, "/").Insert(7, "/");

            KensaMultiRow = EyeDict.EyeSet.Tables["KensaMultiForm"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString())[0];

            int form_width = int.Parse(KensaMultiRow["FormWidth"].ToString());
            int form_height = int.Parse(KensaMultiRow["FormHeight"].ToString());

            this.Size = new Size(form_width, form_height);
            this.Location = new Point(0, 0);

            panel_width = int.Parse(KensaMultiRow["PanelWidth"].ToString());
            panel_height = int.Parse(KensaMultiRow["PanelHeight"].ToString());

            int cont_move_height = int.Parse(KensaMultiRow["ContMoveHeight"].ToString());
            int multi_x = int.Parse(KensaMultiRow["MultiX"].ToString());

            // 検査データ
            DataRow[] tmpRows = EyeDict.EyeSet.Tables["KensaContBox"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());
            Size cont_size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()) + cont_move_height);
            Point cont_location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));

            // SEQボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaSEQBox"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());
            Size seq_size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            Point seq_location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            int seq_max = int.Parse(tmpRows[0]["Max"].ToString());

            // 作成者ラベル
            tmpRows = EyeDict.EyeSet.Tables["KensaStaffLabel"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());
            Size staff_size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            Point staff_location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()) + cont_move_height);

            // 登録ボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaRegButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());
            Size reg_size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            Point reg_location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));

            // 削除ボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaDelButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());
            Size del_size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            Point del_location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()) + cont_move_height);

            KensaList = EyeKensa2.LoadByPatKensaDate(PtId, KensaId, KensaDate);

            int x = 0;
            int y = 0;
            int counter = 0;

            Font f9 = new Font("ＭＳ ゴシック", 9);
            EyeKensa2 kensa;

            for (int i = 1; i <= seq_max; i++)
            {
                kensa = new EyeKensa2();

                // 同一の SEQ をもつ EyeKensa2 が存在するかチェックする
                for (int j = 0; j < KensaList.Count; j++)
                {
                    if (i.ToString().Equals(KensaList[j].KensaSEQ))
                    {
                        kensa = KensaList[j];
                        break;
                    }
                }

                Panel tmpPanel = new Panel();
                tmpPanel.Name = "Panel" + i;
                tmpPanel.Size = new Size(panel_width, panel_height);
                tmpPanel.Location = new Point(x, y);
                tmpPanel.AutoScroll = true;
                tmpPanel.BorderStyle = BorderStyle.FixedSingle;

                // 検査データ
                TextBox ContBox = new TextBox();
                ContBox.Name = "ContBox";
                ContBox.Multiline = true;
                ContBox.Font = f9;
                ContBox.ScrollBars = ScrollBars.Vertical;
                ContBox.Size = cont_size;
                ContBox.Location = cont_location;
                ContBox.Text = kensa.Cont;

                tmpPanel.Controls.Add(ContBox);

                // SEQボタン
                ComboBox SEQBox = new ComboBox();
                SEQBox.Name = "SEQBox";
                SEQBox.DropDownStyle = ComboBoxStyle.DropDownList;

                for (int j = 1; j <= seq_max; j++)
                {
                    SEQBox.Items.Add(j);
                }

                SEQBox.Text = i.ToString();
                SEQBox.Size = seq_size;
                SEQBox.Location = seq_location;

                tmpPanel.Controls.Add(SEQBox);

                // 作成者ラベル
                Label staffLabel = new Label();
                staffLabel.Name = "StaffLabel";
                staffLabel.BackColor = Color.LightYellow;
                staffLabel.Size = staff_size;
                staffLabel.TextAlign = ContentAlignment.MiddleCenter;
                staffLabel.Location = staff_location;

                if (Dict.StaffDict.ContainsKey(kensa.Staff))
                {
                    staffLabel.Text = Dict.StaffDict[kensa.Staff].Name;
                }

                tmpPanel.Controls.Add(staffLabel);

                // 登録ボタン
                Button regButton = new Button();
                regButton.Name = "RegButton";
                regButton.Text = "登録";
                regButton.Size = reg_size;
                regButton.TextAlign = ContentAlignment.MiddleCenter;
                regButton.Location = reg_location;
                regButton.Click += new EventHandler(RegButton_Click);

                // 登録ボタンはPDF保存されていれば不可とする
                int k = 0;

                if (kensa.PDFSave.Equals("1"))
                {
                    regButton.Enabled = false;
                }
                else
                {
                    regButton.Enabled = true;
                }

                tmpPanel.Controls.Add(regButton);

                // 削除ボタン
                Button delButton = new Button();
                delButton.Name = "DelButton";
                delButton.Text = "削除";
                delButton.Size = del_size;
                delButton.TextAlign = ContentAlignment.MiddleCenter;
                delButton.Location = del_location;
                delButton.Click += new EventHandler(DelButton_Click);

                tmpPanel.Controls.Add(delButton);

                // 削除ボタンはPDF保存されているか、１週間経過以降は不可とする
                if (kensa.PDFSave.Equals("1") || !int.TryParse(kensa.KensaDate, out k) || int.Parse(kensa.KensaDate) < int.Parse(DateTime.Now.AddDays(-7).ToString("yyyyMMdd")))
                {
                    delButton.Enabled = false;
                }
                else
                {
                    delButton.Enabled = true;
                }

                // レフケラト取込ボタン
                if (KensaId.Equals("18"))
                {
                    // 取込ボタン
                    foreach (DataRow tmpRow in EyeDict.EyeSet.Tables["RefKrtButton"].Rows)
                    {
                        Button refButton = new Button();
                        refButton.Name = "RefKrtButton";
                        refButton.Text = tmpRow["Text"].ToString();
                        refButton.Size = new Size(int.Parse(tmpRow["Width"].ToString()), int.Parse(tmpRow["Height"].ToString()));
                        refButton.TextAlign = ContentAlignment.MiddleCenter;
                        refButton.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));
                        refButton.Tag = tmpRow["File"].ToString();
                        refButton.Click += new EventHandler(RefKrtButton_Click);

                        tmpPanel.Controls.Add(refButton);
                    }
                }

                this.ContPanel.Controls.Add(tmpPanel);

                x += panel_width;
                counter++;

                if (counter > multi_x)
                {
                    counter = 0;
                    x = 0;
                    y += panel_height;
                }
            }
        }

        /// <summary>
        /// レフケラト取込ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RefKrtButton_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);

            if (tmpButton.Parent.Controls["ContBox"].Text.Length > 0)
            {
                if (MessageBox.Show("データが存在します。取り込むと上書きされますが、よろしいですか？", "確認", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                {
                    return;
                }
            }

            // Nidek の場合はデータファイルの変換・作成を行う
            if (NidekARK1.Settings.IsNidek)
            {
                NidekARK1.ConvertSaveLast();
            }

            EyeRefKrt.Read(tmpButton.Tag.ToString(), (TextBox)(tmpButton.Parent.Controls["ContBox"]));
        }

        /// <summary>
        /// 検査データの登録ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RegButton_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);

            EyeKensa2 tmpKensa = new EyeKensa2();
            tmpKensa.PtId = this.PtId;
            tmpKensa.KensaId = this.KensaId;
            tmpKensa.KensaDate = this.KensaDate;
            tmpKensa.KensaSEQ = tmpButton.Parent.Controls["SEQBox"].Text;
            tmpKensa.Cont = tmpButton.Parent.Controls["ContBox"].Text;
            tmpKensa.Staff = LoginUser.Id;

            tmpKensa.Save();

            MessageBox.Show("保存しました");

            if (tmpButton.Parent.Controls.ContainsKey("StaffLabel"))
            {
                tmpButton.Parent.Controls["StaffLabel"].Text = LoginUser.Name;
            }
        }

        /// <summary>
        /// 検査データの削除ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DelButton_Click(object sender, EventArgs e)
        {
            if (DateTime.Now.Subtract(DateTime.Parse(KensaDate.Insert(4, "/").Insert(7, "/"))).Days > 7)
            {
                if (MessageBox.Show("１週間以上前の検査は削除できません。削除しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                {
                    return;
                }
            }
            else if (MessageBox.Show("削除しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1) != DialogResult.OK)
            {
                return;
            }

            Button tmpButton = (Button)(sender);

            EyeKensa2.Delete(PtId, KensaId, KensaDate, tmpButton.Parent.Controls["SEQBox"].Text);

            MessageBox.Show("削除しました");

            tmpButton.Parent.Controls["ContBox"].Text = "";
            tmpButton.Parent.Controls["StaffLabel"].Text = "";
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}