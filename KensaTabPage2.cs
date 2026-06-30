using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    class KensaTabPage2 : KensaTabPageBase
    {
        /// <summary>
        /// 親フォーム（FormPat）を取得する
        /// </summary>
        FormPat P_FormPat
        {
            get
            {
                FormPat f = null;
                Control c = this.Parent;

                while (c != null)
                {
                    if (c is FormPat)
                    {
                        f = (FormPat)c;
                        break;
                    }

                    c = c.Parent;
                }

                return f;
            }
        }

        /// <summary>
        /// 親フォーム（FormPat）の患者IDを取得する
        /// </summary>
        string PtId
        {
            get
            {
                string pt_id = "";

                if (P_FormPat != null) pt_id = P_FormPat.Pat.Id;

                return pt_id;
            }
        }

        /// <summary>
        /// 親コントロール（検査TabPage）の検査日を取得する
        /// </summary>
        string KensaDate
        {
            get
            {
                string kensa_date = "";
                Control c = this.Parent;

                while (c != null)
                {
                    if (c is TabPage && c.Controls.ContainsKey("KensaDate"))
                    {
                        kensa_date = ((DateTimePicker)c.Controls["KensaDate"]).Value.ToString("yyyyMMdd");
                        break;
                    }

                    c = c.Parent;
                }

                return kensa_date;
            }
        }

        /// <summary>
        /// 値が変更されたら true になる
        /// </summary>
        bool _Edited = false;

        public bool Edited
        {
            get
            {
                return this._Edited;
            }
            set
            {
                this._Edited = value;
            }
        }

        public KensaTabPage2(string kensa_id)
        {
            this.KensaId = kensa_id;
            this.KensaPageRow = EyeDict.EyeSet.Tables["KensaPage"].Select("ID = '" + KensaId + "'")[0];

            this.ControlShow(kensa_id);
        }

        protected override void ControlShow(string kensa_id)
        {
            this.KensaId = kensa_id;
            this.Controls.Clear();

            if (!KensaPageRow["PageVisible"].ToString().Equals("1"))
            {
                return;
            }

            if (KensaPageRow["Tenkey"].ToString().Length > 0)
            {
                TenkeyPanel tenkeyPanel = new TenkeyPanel(KensaPageRow["Tenkey"].ToString());
                tenkeyPanel.Name = "TenkeyPanel";
                this.Controls.Add(tenkeyPanel);
            }

            // 検査データ
            DataRow[] tmpRows = EyeDict.EyeSet.Tables["KensaContBox"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            TextBox ContBox = new TextBox();
            ContBox.Name = "ContBox";
            ContBox.Multiline = true;
            ContBox.Font = new Font("ＭＳ ゴシック", 9);
            ContBox.ScrollBars = ScrollBars.Vertical;
//            ContBox.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
//            ContBox.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            ContBox.Size = new Size(this.Width - 20, this.Height - 60);
            ContBox.Location = new Point(5, 30);
            ContBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            ContBox.TextChanged += new EventHandler(Ctl_Changed);

            this.Controls.Add(ContBox);

            // SEQボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaSEQBox"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            ComboBox SEQBox = new ComboBox();
            SEQBox.Name = "SEQBox";
            SEQBox.DropDownStyle = ComboBoxStyle.DropDownList;

            for (int i = 1; i <= int.Parse(tmpRows[0]["Max"].ToString()); i++)
            {
                SEQBox.Items.Add(i);
            }

            SEQBox.Text = "1";
            SEQBox.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            SEQBox.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            SEQBox.SelectedIndexChanged += new EventHandler(SEQBox_SelectedIndexChanged);

            this.Controls.Add(SEQBox);

            // 作成者ラベル
            tmpRows = EyeDict.EyeSet.Tables["KensaStaffLabel"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            Label staffLabel = new Label();
            staffLabel.Name = "StaffLabel";
            staffLabel.BackColor = Color.LightYellow;
            staffLabel.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            staffLabel.TextAlign = ContentAlignment.MiddleCenter;
//            staffLabel.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            staffLabel.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), this.Height - 23);
            staffLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            this.Controls.Add(staffLabel);

            // 登録ボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaRegButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            Button regButton = new Button();
            regButton.Name = "RegButton";
            regButton.Text = "登録";
            regButton.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            regButton.TextAlign = ContentAlignment.MiddleCenter;
            regButton.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            regButton.Click += new EventHandler(RegButton_Click);

            this.Controls.Add(regButton);

            // 削除ボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaDelButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            Button delButton = new Button();
            delButton.Name = "DelButton";
            delButton.Text = "削除";
            delButton.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            delButton.TextAlign = ContentAlignment.MiddleCenter;
//            delButton.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            delButton.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), this.Height - 25);
            delButton.Click += new EventHandler(DelButton_Click);
            delButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;

            this.Controls.Add(delButton);

            // 複数ボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaMultiButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            Button multiButton = new Button();
            multiButton.Name = "MultiButton";
            multiButton.Text = "複数";
            multiButton.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            multiButton.TextAlign = ContentAlignment.MiddleCenter;
            multiButton.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            multiButton.Click += new EventHandler(MultiButton_Click);

            this.Controls.Add(multiButton);

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

                    this.Controls.Add(refButton);
                }
            }
        }

        /// <summary>
        /// 値が変更された場合
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Ctl_Changed(object sender, EventArgs e)
        {
            this._Edited = true;
        }

        void SEQBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EyeKensa2 tmpKensa = EyeKensa2.LoadByPatKensaDateSEQ(this.PtId, KensaId, this.KensaDate, this.Controls["SEQBox"].Text);

            this.Controls["ContBox"].Text = tmpKensa.Cont;
        }

        /// <summary>
        /// レフケラト取込ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RefKrtButton_Click(object sender, EventArgs e)
        {
            if (this.Controls["ContBox"].Text.Length > 0)
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

            EyeRefKrt.Read(((Button)sender).Tag.ToString(), (TextBox)(this.Controls["ContBox"]));
        }

        /// <summary>
        /// 検査データの複数ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MultiButton_Click(object sender, EventArgs e)
        {
            if (this.PtId.Length > 0)
            {
                try
                {
                    FormKensa2 fk = new FormKensa2(this.PtId, KensaId, this.KensaDate);
                    fk.ShowDialog();
                    P_FormPat.PtKensaHistoryShow();
//                    EyeDict.FormPat_KensaHistoryShow(this.PtId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        /// <summary>
        /// 検査データの登録ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RegButton_Click(object sender, EventArgs e)
        {
            if (this.PtId.Length == 0)
            {
                MessageBox.Show("患者番号を入力してください");
                return;
            }

            if (!DateTime.Now.ToString("yyyyMMdd").Equals(this.KensaDate))
            {
                if (MessageBox.Show("検査日が今日ではありません。登録しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                {
                    return;
                }
            }

            if (this.PtId.Length > 0 && this.KensaDate.Length == 8)
            {
                EyeKensa2 tmpKensa = new EyeKensa2();
                tmpKensa.PtId = this.PtId;
                tmpKensa.KensaId = KensaId;
                tmpKensa.KensaDate = this.KensaDate;

                tmpKensa.KensaSEQ = this.Controls["SEQBox"].Text;
                tmpKensa.Cont = this.Controls["ContBox"].Text;
                tmpKensa.Staff = LoginUser.Id;

                tmpKensa.Save();

                this._Edited = false;
                MessageBox.Show("保存しました");

                if (this.Controls.ContainsKey("StaffLabel"))
                {
                    this.Controls["StaffLabel"].Text = LoginUser.Name;
                }

                P_FormPat.PtKensaHistoryShow();
//                EyeDict.FormPat_KensaHistoryShow(tmpKensa.PtId);
            }
        }

        /// <summary>
        /// 検査データの削除ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DelButton_Click(object sender, EventArgs e)
        {
            DateTime dt = DateTime.Now;

            // 検査日が取得できなければ終了
            if (!DateTime.TryParse(DateTimeAgent.DateFormat(this.KensaDate, DateTimeAgent.DateFormatKind.LONG), out dt))
            {
                return;
            }

            if (DateTime.Now.Subtract(dt).Days > 7)
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

            if (this.PtId.Length > 0 && this.KensaDate.Length == 8)
            {
                EyeKensa2.Delete(this.PtId, KensaId, this.KensaDate, this.Controls["SEQBox"].Text);

                MessageBox.Show("削除しました");

                this.KensaClear();

                P_FormPat.PtKensaHistoryShow();
//                EyeDict.FormPat_KensaHistoryShow(this.PtId);
            }
        }

        public override void KensaShow(EyeKensa2 kensa, int count)
        {
            if (!KensaId.Equals(kensa.KensaId))
            {
                return;
            }

            if (this.Controls.ContainsKey("ContBox"))
            {
                this.Controls["ContBox"].Text = kensa.Cont;
            }

            if (this.Controls.ContainsKey("MultiButton") && count > 1)
            {
                this.Controls["MultiButton"].BackColor = Color.LightPink;
            }
            else
            {
                this.Controls["MultiButton"].BackColor = Color.LightYellow;
            }

            if (this.Controls.ContainsKey("StaffLabel") && Dict.StaffDict.ContainsKey(kensa.Staff))
            {
                this.Controls["StaffLabel"].Text = Dict.StaffDict[kensa.Staff].Name;
            }

            // 削除ボタンはPDF保存されているか、１週間経過以降は不可とする
            int k = 0;

            if (kensa.PDFSave.Equals("1") || !int.TryParse(kensa.KensaDate, out k) || int.Parse(kensa.KensaDate) < int.Parse(DateTime.Now.AddDays(-7).ToString("yyyyMMdd")))
            {
                this.Controls["DelButton"].Enabled = false;
            }
            else
            {
                this.Controls["DelButton"].Enabled = true;
            }

            // 登録ボタンはPDF保存されていれば不可とする
            if (kensa.PDFSave.Equals("1"))
            {
                this.Controls["RegButton"].Enabled = false;
            }
            else
            {
                this.Controls["RegButton"].Enabled = true;
            }

            this._Edited = false;
        }

        public override void KensaClear()
        {
            if (this.Controls.ContainsKey("MultiButton"))
            {
                this.Controls["MultiButton"].BackColor = Color.LightYellow;
            }

            if (this.Controls.ContainsKey("ContBox"))
            {
                this.Controls["ContBox"].Text = "";
            }

            if (this.Controls.ContainsKey("StaffLabel"))
            {
                this.Controls["StaffLabel"].Text = "";
            }

            if (this.Controls.ContainsKey("SEQBox"))
            {
                this.Controls["SEQBox"].Text = "1";
            }

            this._Edited = false;
        }
    }
}
