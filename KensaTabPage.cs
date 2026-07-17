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
    class KensaTabPage : KensaTabPageBase
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
        /// 検査データが変更されたかどうか
        /// </summary>
        public bool Edited
        {
            get
            {
                bool b = false;

                if (this.Controls.ContainsKey("KensaPanel"))
                {
                    Control c = this.Controls["KensaPanel"];

                    if (c is KensaPanelDetail)
                    {
                        b = ((KensaPanelDetail)c).Edited;
                    }
                }

                return b;
            }
            set
            {
                if (this.Controls.ContainsKey("KensaPanel"))
                {
                    Control c = this.Controls["KensaPanel"];

                    if (c is KensaPanelDetail)
                    {
                        ((KensaPanelDetail)c).Edited = value;
                    }
                }
            }
        }

        public KensaTabPage(string kensa_id)
        {
            this.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

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

            // 検査パネル
            DataRow[] tmpRows = EyeDict.EyeSet.Tables["KensaPanel"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            KensaPanelDetail tmpPanel = new KensaPanelDetail(KensaId);
            tmpPanel.Name = "KensaPanel";
            tmpPanel.AutoScroll = true;
            tmpPanel.BorderStyle = BorderStyle.Fixed3D;
            tmpPanel.BackColor = Color.FromArgb(255, 255, 224);
            tmpPanel.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            tmpPanel.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            tmpPanel.Resize += new EventHandler(KensaPanel_Resize);
            tmpPanel.Tag = tmpRows[0]["Anchor"].ToString();

            if (tmpPanel.Tag.ToString().Equals("1"))
            {
                tmpPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            }

            this.Controls.Add(tmpPanel);

            // 履歴ボタン
            tmpRows = EyeDict.EyeSet.Tables["KensaHistoryButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            Button historyButton = new Button();
            historyButton.Name = "HistoryButton";
            historyButton.Text = "履歴";
            historyButton.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            historyButton.TextAlign = ContentAlignment.MiddleCenter;
            historyButton.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            historyButton.Click += new EventHandler(HistoryButton_Click);

            this.Controls.Add(historyButton);

            // 作成者ラベル
            tmpRows = EyeDict.EyeSet.Tables["KensaStaffLabel"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            Label staffLabel = new Label();
            staffLabel.Name = "StaffLabel";
            staffLabel.BackColor = Color.LightYellow;
            staffLabel.Size = new Size(int.Parse(tmpRows[0]["Width"].ToString()), int.Parse(tmpRows[0]["Height"].ToString()));
            staffLabel.TextAlign = ContentAlignment.MiddleCenter;
            staffLabel.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));

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
            delButton.Location = new Point(int.Parse(tmpRows[0]["X"].ToString()), int.Parse(tmpRows[0]["Y"].ToString()));
            delButton.Click += new EventHandler(DelButton_Click);

            this.Controls.Add(delButton);
        }

        void KensaPanel_Resize(object sender, EventArgs e)
        {
            KensaPanelDetail panel = (KensaPanelDetail)sender;

            if (panel.Tag.ToString().Equals("1"))
            {
                panel.Width = this.Width - 293;
                panel.Height = this.Height - 37;
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

            if (!this.KensaDate.Equals(DateTime.Now.ToString("yyyyMMdd")))
            {
                if (MessageBox.Show("検査日が今日ではありません。登録しますか？", "確認", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.Cancel)
                {
                    return;
                }
            }

            string cont = ContData.Build(this.Controls["KensaPanel"].Controls);

            if (this.PtId.Length > 0 && this.KensaDate.Length == 8)
            {
                EyeKensa tmpKensa = new EyeKensa();
                tmpKensa.PtId = this.PtId;
                tmpKensa.KensaId = KensaId;
                tmpKensa.KensaDate = this.KensaDate;

                tmpKensa.Cont = cont;
                tmpKensa.Staff = LoginUser.Id;

                tmpKensa.Save();

                this.Edited = false;
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
                EyeKensa.Delete(this.PtId, KensaId, this.KensaDate);

                MessageBox.Show("削除しました");

                this.KensaClear();

                P_FormPat.PtKensaHistoryShow();
//                EyeDict.FormPat_KensaHistoryShow(this.PtId);
            }
        }

        void HistoryButton_Click(object sender, EventArgs e)
        {
            if (this.PtId.Length > 0)
            {
                try
                {
                    FormKensaHistory fk = new FormKensaHistory(this.PtId, KensaId);
                    fk.ShowDialog(this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public override void KensaShow(EyeKensa kensa)
        {
            if (!KensaId.Equals(kensa.KensaId))
            {
                return;
            }

            if (this.Controls.ContainsKey("KensaPanel"))
            {
                ((KensaPanelDetail)(this.Controls["KensaPanel"])).KensaShow(kensa);
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
        }

        public override void KensaClear()
        {
            if (this.Controls.ContainsKey("StaffLabel"))
            {
                this.Controls["StaffLabel"].Text = "";
            }

            if (this.Controls.ContainsKey("KensaPanel"))
            {
                ((KensaPanelDetail)(this.Controls["KensaPanel"])).KensaClear();
            }
        }
    }
}
