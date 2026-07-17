using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public class KensaPanelDetail : Panel
    {
        // 親フォーム（FormPat）の患者IDを取得する
        string PtId
        {
            get
            {
                string pt_id = "";
                Control c = this.Parent;

                while (c != null)
                {
                    if (c is FormPat)
                    {
                        pt_id = ((FormPat)c).Pat.Id;
                        break;
                    }

                    c = c.Parent;
                }

                return pt_id;
            }
        }

        // 親コントロール（検査TabPage）の検査日を取得する
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

        string KensaId = "";

        int panel_width = 0;
        int PanelHeight = 0;

        DataRow KensaPageRow;

        /// <summary>
        /// フォーカスが当たっているコントロール（TextBox）
        /// </summary>
        public string FocusedBox = "";

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


        public KensaPanelDetail(string kensa_id)
        {
            this.KensaId = kensa_id;
            this.KensaPageRow = EyeDict.EyeSet.Tables["KensaPage"].Select("ID = '" + KensaId + "'")[0];

            this.ControlShow(kensa_id);
        }

        public void ControlShow(string kensa_id)
        {
            this.KensaId = kensa_id;
            this.Controls.Clear();

            // 検査データのコントロールを作成する
            DataRow[] tmpRows = EyeDict.EyeSet.Tables["KensaItem"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

            foreach (DataRow tmpRow in tmpRows)
            {
                if (tmpRow["Type"].ToString().Equals("Label"))
                {
                    Label tmpLabel = new Label();
                    tmpLabel.Name = tmpRow["Name"].ToString();
                    tmpLabel.Text = tmpRow["Text"].ToString();
                    tmpLabel.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));

                    tmpLabel.AutoSize = true;

                    this.Controls.Add(tmpLabel);
                }
                else if (tmpRow["Type"].ToString().Equals("TextBox"))
                {
                    TextBox tmpBox = new TextBox();
                    tmpBox.Name = tmpRow["Name"].ToString();
                    tmpBox.Text = tmpRow["Text"].ToString();
                    tmpBox.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));
                    tmpBox.Tag = tmpRow["Code"].ToString();

                    if (tmpRow["Width"].ToString().Length > 0)
                    {
                        tmpBox.Width = int.Parse(tmpRow["Width"].ToString());
                    }

                    if (tmpRow["Height"].ToString().Length > 0)
                    {
                        tmpBox.Height = int.Parse(tmpRow["Height"].ToString());
                    }

                    if (tmpRow["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (tmpRow["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else
                    {
                        tmpBox.ImeMode = ImeMode.NoControl;
                    }

                    if (tmpRow["Align"].ToString().Equals("Right"))
                    {
                        tmpBox.TextAlign = HorizontalAlignment.Right;
                    }
                    else if (tmpRow["Align"].ToString().Equals("Center"))
                    {
                        tmpBox.TextAlign = HorizontalAlignment.Center;
                    }
                    else
                    {
                        tmpBox.TextAlign = HorizontalAlignment.Left;
                    }

                    if (tmpRow["Multiline"].ToString().Equals("1"))
                    {
                        tmpBox.Multiline = true;
                    }
                    else
                    {
                        tmpBox.KeyDown += new KeyEventHandler(TextBox_KeyDown);
                    }

                    if (tmpRow["Scroll"].ToString().Equals("Both"))
                    {
                        tmpBox.ScrollBars = ScrollBars.Both;
                    }
                    else if (tmpRow["Scroll"].ToString().Equals("Vertical"))
                    {
                        tmpBox.ScrollBars = ScrollBars.Vertical;
                    }
                    else if (tmpRow["Scroll"].ToString().Equals("Horizontal"))
                    {
                        tmpBox.ScrollBars = ScrollBars.Horizontal;
                    }

                    tmpBox.Click += new EventHandler(TextBox_Click);
                    tmpBox.TextChanged += new EventHandler(Ctl_Changed);

                    // 眼圧の場合は平均値の計算式を入れる
                    if (KensaId.Equals("3") && tmpRow["Code"].ToString().Length == 4)
                    {
                        char t = tmpRow["Code"].ToString()[3];
                        char c = tmpRow["Code"].ToString()[2];

                        if (t != 'T' && (c == '1' || c == '2' || c == '3'))
                        {
                            tmpBox.Leave += new EventHandler(TensionBox_Leave);
                            tmpBox.TextChanged += new EventHandler(TensionBox_Leave);
                        }
                    }

                    this.Controls.Add(tmpBox);
                }
                else if (tmpRow["Type"].ToString().Equals("ComboBox"))
                {
                    ComboBox tmpBox = new ComboBox();
                    tmpBox.Name = tmpRow["Name"].ToString();
                    tmpBox.Text = tmpRow["Text"].ToString();
                    tmpBox.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));
                    tmpBox.Tag = tmpRow["Code"].ToString();

                    if (tmpRow["Width"].ToString().Length > 0)
                    {
                        tmpBox.Width = int.Parse(tmpRow["Width"].ToString());
                    }

                    if (tmpRow["Height"].ToString().Length > 0)
                    {
                        tmpBox.Height = int.Parse(tmpRow["Height"].ToString());
                    }

                    if (tmpRow["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (tmpRow["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else
                    {
                        tmpBox.ImeMode = ImeMode.NoControl;
                    }

                    foreach (string s in tmpRow["Item"].ToString().Split(','))
                    {
                        tmpBox.Items.Add(s);
                    }

                    tmpBox.Click += new EventHandler(ComboBox_Click);
                    tmpBox.KeyDown += new KeyEventHandler(ComboBox_KeyDown);
                    tmpBox.TextChanged += new EventHandler(Ctl_Changed);

                    this.Controls.Add(tmpBox);
                }
                else if (tmpRow["Type"].ToString().Equals("CheckBox"))
                {
                    CheckBox tmpBox = new CheckBox();
                    tmpBox.Name = tmpRow["Name"].ToString();
                    tmpBox.Text = tmpRow["Text"].ToString();
                    tmpBox.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));
                    tmpBox.Tag = tmpRow["Code"].ToString();
                    tmpBox.AutoSize = true;
                    tmpBox.CheckedChanged += new EventHandler(Ctl_Changed);

                    this.Controls.Add(tmpBox);
                }
            }

            // パネル外にデータがあることを示すラベル
            tmpRows = EyeDict.EyeSet.Tables["KensaPanel"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());
            this.panel_width = int.Parse(tmpRows[0]["Width"].ToString());
            this.PanelHeight = int.Parse(tmpRows[0]["Height"].ToString());

            Label overLabel = new Label();
            overLabel.Name = "OverLabel";
            overLabel.AutoSize = false;
            overLabel.Size = new Size(25, 12);
            overLabel.Location = new Point(panel_width - 48, 3);
            overLabel.BackColor = Color.Red;
            overLabel.Visible = false;
            this.Controls.Add(overLabel);

            if (KensaPageRow["Name"].ToString().Equals("眼圧"))
            {
                foreach (DataRow timeRow in EyeDict.EyeSet.Tables["TensionTimeButton"].Rows)
                {
                    Button timeButton = new Button();
                    timeButton.Text = timeRow["Text"].ToString();
                    timeButton.BackColor = Color.FromArgb(255, 255, 156);
                    timeButton.Size = new Size(int.Parse(timeRow["Width"].ToString()), int.Parse(timeRow["Height"].ToString()));
                    timeButton.TextAlign = ContentAlignment.MiddleCenter;
                    timeButton.Location = new Point(int.Parse(timeRow["X"].ToString()), int.Parse(timeRow["Y"].ToString()));
                    timeButton.Tag = timeRow["Code"].ToString();
                    timeButton.Click += new EventHandler(TensionTimeButton_Click);
                    this.Controls.Add(timeButton);
                }
            }

            if (KensaPageRow["Name"].ToString().Equals("コントラスト感度"))
            {
                this.Controls.Add(new ContrastPanel());
            }

            if (KensaPageRow["Name"].ToString().StartsWith("CL注文"))
            {
                // コンタクト注文シートの種類が増え、タブも増えたため対応 2015/04/06, sakane
                foreach (DataRow contactRow in EyeDict.EyeSet.Tables["ContactOrderButton"].Rows)
                {
                    if (KensaPageRow["ID"].ToString().Equals(contactRow["KensaID"].ToString()))
                    {
                        Button contactButton = new Button();
                        contactButton.Text = contactRow["Text"].ToString();
                        contactButton.BackColor = Color.FromArgb(255, 255, 156);
                        contactButton.Size = new Size(int.Parse(contactRow["Width"].ToString()), int.Parse(contactRow["Height"].ToString()));
                        contactButton.TextAlign = ContentAlignment.MiddleCenter;
                        contactButton.Location = new Point(int.Parse(contactRow["X"].ToString()), int.Parse(contactRow["Y"].ToString()));
                        contactButton.Click += new EventHandler(ContactButton_Click);
                        this.Controls.Add(contactButton);

                        break;
                    }
                }
            }

            if (KensaPageRow["Name"].ToString().Equals("レンズ視力"))
            {
                foreach (DataRow lensRow in EyeDict.EyeSet.Tables["LensMeterButton"].Rows)
                {
                    Button lensButton = new Button();
                    lensButton.Text = lensRow["Text"].ToString();
                    lensButton.BackColor = Color.FromArgb(255, 255, 156);
                    lensButton.Size = new Size(int.Parse(lensRow["Width"].ToString()), int.Parse(lensRow["Height"].ToString()));
                    lensButton.TextAlign = ContentAlignment.MiddleCenter;
                    lensButton.Location = new Point(int.Parse(lensRow["X"].ToString()), int.Parse(lensRow["Y"].ToString()));
                    lensButton.Tag = lensRow["Id"].ToString();
                    lensButton.Click += new EventHandler(LensButton_Click);
                    this.Controls.Add(lensButton);
                }
            }

            if (KensaPageRow["Name"].ToString().Equals("メガネ処方"))
            {
                foreach (DataRow glassRow in EyeDict.EyeSet.Tables["GlassPrescButton"].Rows)
                {
                    Button glassButton = new Button();
                    glassButton.Text = glassRow["Text"].ToString();
                    glassButton.BackColor = Color.FromArgb(255, 255, 156);
                    glassButton.Size = new Size(int.Parse(glassRow["Width"].ToString()), int.Parse(glassRow["Height"].ToString()));
                    glassButton.TextAlign = ContentAlignment.MiddleCenter;
                    glassButton.Location = new Point(int.Parse(glassRow["X"].ToString()), int.Parse(glassRow["Y"].ToString()));
                    glassButton.Click += new EventHandler(GlassButton_Click);
                    this.Controls.Add(glassButton);
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

        /// <summary>
        /// 眼圧平均値を計算する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TensionBox_Leave(object sender, EventArgs e)
        {
            TextBox tmpBox = (TextBox)sender;

            if (tmpBox.Tag.ToString().Length == 4)
            {
                string str = tmpBox.Tag.ToString().Substring(0, 2);
                char c = tmpBox.Tag.ToString()[2];
                char g = tmpBox.Tag.ToString()[3];

                if (c == '1' || c == '2' || c == '3')
                {
                    double t1 = 0;
                    double t2 = 0;
                    double t3 = 0;

                    string s1 = this.Controls[str + "1" + g].Text;
                    string s2 = this.Controls[str + "2" + g].Text;
                    string s3 = this.Controls[str + "3" + g].Text;

                    if (s1.Length > 0 && double.TryParse(s1, out t1)) { ; }
                    if (s2.Length > 0 && double.TryParse(s2, out t2)) { ; }
                    if (s3.Length > 0 && double.TryParse(s3, out t3)) { ; }

                    this.Controls[str + "4" + g].Text = EyeDict.CalcTensionAvg(t1, t2, t3).ToString();
                }
            }
        }

        /// <summary>
        /// 眼圧の測定時刻ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TensionTimeButton_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)sender;
            this.Controls[tmpButton.Tag.ToString()].Text = DateTime.Now.ToString("HH:mm");
        }

        /// <summary>
        /// コンタクト注文書ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContactButton_Click(object sender, EventArgs e)
        {
            if (this.PtId.Length > 0)
            {
                EyeDoc tmpDoc = new EyeDoc(this.PtId);

                // コンタクト注文シートの種類が増え、タブも増えたため対応 2015/04/06, sakane
                foreach (DataRow contactRow in EyeDict.EyeSet.Tables["ContactOrderButton"].Rows)
                {
                    if (this.KensaId.Equals(contactRow["KensaID"].ToString()))
                    {
                        tmpDoc.FileName = contactRow["File"].ToString();
                        break;
                    }
                }

                // コンタクト注文シートの種類が増え、タブも増えたため上記に変更 2015/04/06, sakane
                // tmpDoc.FileName = EyeDict.EyeSet.Tables["ContactOrderButton"].Rows[0]["File"].ToString();

                EyeDoc.Item item_date = new EyeDoc.Item();
                item_date.Kind = "コンタクト注文";
                item_date.Name = "注文日";
                item_date.Value = DateTimeAgent.DateFormat(this.KensaDate, DateTimeAgent.DateFormatKind.LONG);
                tmpDoc.ItemList.Add(item_date);

                foreach (Control c in this.Controls)
                {
                    if (c is TextBox || c is ComboBox)
                    {
                        EyeDoc.Item tmpItem = new EyeDoc.Item();
                        tmpItem.Kind = "コンタクト注文";
                        tmpItem.Name = c.Name;
                        tmpItem.Value = c.Text;
                        tmpDoc.ItemList.Add(tmpItem);
                    }
                }

                tmpDoc.ExcelOpen();
            }
        }

        /// <summary>
        /// レンズメーターのデータ取込ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LensButton_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);

            EyeLensMeter tmpMeter = new EyeLensMeter();
            string id = tmpButton.Tag.ToString();
            tmpMeter.ReadData(EyeDict.EyeSet.Tables["LensMeterButton"].Select("Id = '" + id + "'")[0]["File"].ToString());

            this.Controls["2" + id + "1R"].Text = tmpMeter.SPH_R;
            this.Controls["2" + id + "2R"].Text = tmpMeter.CYL_R;
            this.Controls["2" + id + "3R"].Text = tmpMeter.AXIS_R;
            this.Controls["2" + id + "4R"].Text = tmpMeter.ADD1_R;
            this.Controls["2" + id + "5R"].Text = tmpMeter.ADD2_R;
            this.Controls["2" + id + "6R"].Text = tmpMeter.PRISM_R;
            this.Controls["2" + id + "7R"].Text = tmpMeter.BASE_R;

            this.Controls["2" + id + "1L"].Text = tmpMeter.SPH_L;
            this.Controls["2" + id + "2L"].Text = tmpMeter.CYL_L;
            this.Controls["2" + id + "3L"].Text = tmpMeter.AXIS_L;
            this.Controls["2" + id + "4L"].Text = tmpMeter.ADD1_L;
            this.Controls["2" + id + "5L"].Text = tmpMeter.ADD2_L;
            this.Controls["2" + id + "6L"].Text = tmpMeter.PRISM_L;
            this.Controls["2" + id + "7L"].Text = tmpMeter.BASE_L;
        }

        /// <summary>
        /// メガネ処方のデータ取込ボタンを押したとき。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GlassButton_Click(object sender, EventArgs e)
        {
            if (this.PtId.Length > 0)
            {
                EyeDoc tmpDoc = new EyeDoc(this.PtId);

                tmpDoc.FileName = EyeDict.EyeSet.Tables["GlassPrescButton"].Rows[0]["File"].ToString();

                EyeDoc.Item item_date = new EyeDoc.Item();
                item_date.Kind = "メガネ処方";
                item_date.Name = "処方日";
                item_date.Value = DateTimeAgent.DateFormat(this.KensaDate, DateTimeAgent.DateFormatKind.LONG);
                tmpDoc.ItemList.Add(item_date);

                foreach (Control c in this.Controls)
                {
                    if (c is TextBox || c is ComboBox)
                    {
                        EyeDoc.Item tmpItem = new EyeDoc.Item();
                        tmpItem.Kind = "メガネ処方";
                        tmpItem.Name = c.Name;
                        tmpItem.Value = c.Text;
                        tmpDoc.ItemList.Add(tmpItem);
                    }
                }

                tmpDoc.ExcelOpen();
            }
        }

        void TextBox_Click(object sender, EventArgs e)
        {
            TextBox tmpBox = (TextBox)(sender);
            this.FocusedBox = tmpBox.Name;
        }

        void ComboBox_Click(object sender, EventArgs e)
        {
            ComboBox tmpBox = (ComboBox)(sender);
            this.FocusedBox = tmpBox.Name;
        }

        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.MoveNext();
            }
        }

        void ComboBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.MoveNext();
            }
        }

        public void KensaShow(EyeKensa kensa)
        {
            // 検査データを表示する
            Dictionary<string, string> kensaDict = new Dictionary<string, string>();

            string[] line = kensa.Cont.Split('\r', '\n');
            string value = "";

            foreach (string s in line)
            {
                string[] ss = s.Split(',');

                if (ss.Length >= 2)
                {
                    if (!kensaDict.ContainsKey(ss[0]))
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

                        kensaDict.Add(ss[0], value.Replace("<CR+LF>", "\r\n"));
                    }
                }
            }

            bool over_flg = false;

            foreach (Control c in this.Controls)
            {
                if (c is TextBox || c is ComboBox)
                {
                    if (kensaDict.ContainsKey(c.Tag.ToString()))
                    {
                        c.Text = kensaDict[c.Tag.ToString()];

                        if (c.Location.X >= this.panel_width - 10 || c.Location.Y >= this.PanelHeight - 10)
                        {
                            over_flg = true;
                        }
                    }
                }
                else if (c is CheckBox)
                {
                    if (kensaDict.ContainsKey(c.Tag.ToString()))
                    {
                        ((CheckBox)c).Checked = true;

                        if (c.Location.X >= this.panel_width - 10 || c.Location.Y >= this.PanelHeight - 10)
                        {
                            over_flg = true;
                        }
                    }
                }
            }

            if (over_flg)
            {
                ((Label)(this.Controls["OverLabel"])).Visible = true;
            }
            else
            {
                ((Label)(this.Controls["OverLabel"])).Visible = false;
            }

            if (KensaPageRow["Name"].ToString().Equals("コントラスト感度"))
            {
                if (this.Controls.ContainsKey("ContrastPanel"))
                {
                    ((ContrastPanel)(this.Controls["ContrastPanel"])).KensaShow();
                }
            }

            this._Edited = false;
        }

        public void KensaClear()
        {
            FocusedBox = "";

            foreach (Control c in this.Controls)
            {
                if (c is TextBox || c is ComboBox)
                {
                    c.Text = "";
                }
                else if (c is CheckBox)
                {
                    ((CheckBox)c).Checked = false;
                }
            }

            ((Label)(this.Controls["OverLabel"])).Visible = false;

            if (KensaPageRow["Name"].ToString().Equals("コントラスト感度"))
            {
                if (this.Controls.ContainsKey("ContrastPanel"))
                {
                    ((ContrastPanel)(this.Controls["ContrastPanel"])).KensaClear();
                }
            }

            this._Edited = false;
        }

        public void InputWord(string word)
        {
            if (FocusedBox.Length == 0) return;

            if (this.Controls.ContainsKey(FocusedBox))
            {
                this.Controls[FocusedBox].Text += word;
            }
        }

        public void InputClear()
        {
            if (FocusedBox.Length == 0) return;

            if (this.Controls.ContainsKey(FocusedBox))
            {
                this.Controls[FocusedBox].Text = "";
            }
        }

        public void MoveNext()
        {
            if (FocusedBox.Length == 0) return;

            bool loop_flg = true;

            for (int i = 0; i < this.Controls.Count && loop_flg; i++)
            {
                if (this.Controls[i].Name.Equals(FocusedBox))
                {
                    for (int j = i + 1; j < this.Controls.Count; j++)
                    {
                        if (this.Controls[j] is TextBox || this.Controls[j] is ComboBox)
                        {
                            this.Controls[j].Select();
                            FocusedBox = this.Controls[j].Name;
                            loop_flg = false;
                            break;
                        }
                    }
                }
            }
        }

        public void MovePrev()
        {
            if (FocusedBox.Length == 0) return;

            bool loop_flg = true;

            for (int i = 0; i < this.Controls.Count && loop_flg; i++)
            {
                if (this.Controls[i].Name.Equals(FocusedBox))
                {
                    for (int j = i - 1; j >= 0; j--)
                    {
                        if (this.Controls[j] is TextBox || this.Controls[j] is ComboBox)
                        {
                            this.Controls[j].Select();
                            FocusedBox = this.Controls[j].Name;
                            loop_flg = false;
                            break;
                        }
                    }
                }
            }
        }
    }
}
