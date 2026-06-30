using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormKensaHistory : Form
    {
        string PtId = "";
        string KensaId = "";

        DataRow KensaPageRow;
        DataRow KensaHistoryRow;
        DataRow KensaDoRow;
        List<EyeKensa> KensaList;

        int panel_width = 0;
        int panel_height = 0;

        public FormKensaHistory(string pt_id, string kensa_id)
        {
            InitializeComponent();

            PtId = pt_id;
            KensaId = kensa_id;
        }

        private void FormKensaHistory_Load(object sender, EventArgs e)
        {
            KensaPageRow = EyeDict.EyeSet.Tables["KensaPage"].Select("ID = '" + KensaId + "'")[0];

            this.Text += " " + KensaPageRow["Text"].ToString();

            KensaHistoryRow = EyeDict.EyeSet.Tables["KensaHistoryForm"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString())[0];
            KensaDoRow = EyeDict.EyeSet.Tables["KensaDoButton"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString())[0];

            int form_width = int.Parse(KensaHistoryRow["FormWidth"].ToString());
            int form_height = int.Parse(KensaHistoryRow["FormHeight"].ToString());

            this.Size = new Size(form_width, form_height);
            this.Location = new Point(10, 10);

            this.ExcelButton.Location = new Point(form_width - 135, 5);
            this.CloseButton.Location = new Point(form_width - 80, 5);

            KensaList = EyeKensa.LoadByPatKensa(PtId, KensaId);

            if (KensaList.Count > 0)
            {
                DescLabel.Text = "ピンクの日のデータが表示されます。";

                int x = 2;
                int y = 30;
                int counter = 0;

                foreach (EyeKensa kensa in KensaList)
                {
                    Label tmpLabel = new Label();
                    tmpLabel.Text = (KensaList.Count - counter) + ". " + kensa.KensaDate.PadRight(8, '0').Substring(2, 6).Insert(2, "/").Insert(5, "/");
                    tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                    tmpLabel.Size = new Size(86, 18);
                    tmpLabel.Location = new Point(x, y);

                    // デフォルトは直近２０回までを表示
                    if (counter < 20)
                    {
                        tmpLabel.BackColor = Color.LightPink;
                    }
                    else
                    {
                        tmpLabel.BackColor = Color.Wheat;
                    }

                    tmpLabel.Click += new EventHandler(DateLabel_Click);
                    tmpLabel.Tag = kensa.KensaDate;

                    this.Controls.Add(tmpLabel);

                    y += 20;
                    counter++;
                }

                this.HistoryContentShow();
            }
            else
            {
                DescLabel.Text = "過去の履歴はありません。";
            }
        }

        /// <summary>
        /// 日付がクリックされたときの動作。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DateLabel_Click(object sender, EventArgs e)
        {
            Label tmpLabel = (Label)sender;

            if (tmpLabel.BackColor == Color.Wheat)
            {
                tmpLabel.BackColor = Color.LightPink;
            }
            else
            {
                tmpLabel.BackColor = Color.Wheat;
            }

            this.HistoryContentShow();
        }

        /// <summary>
        /// 選択された日付ボタンの検査結果を表示する。
        /// </summary>
        void HistoryContentShow()
        {
            List<string> dateList = new List<string>();

            foreach (Control c in this.Controls)
            {
                if (c.GetType().Name.Equals("Label"))
                {
                    if (c.BackColor == Color.LightPink)
                    {
                        dateList.Add(c.Tag.ToString());
                    }
                }
            }

            this.ContentPanel.Controls.Clear();

            int x = 2;
            int y = 2;

            panel_width = int.Parse(KensaHistoryRow["PanelWidth"].ToString());
            panel_height = int.Parse(KensaHistoryRow["PanelHeight"].ToString());

            int item_move_x = 0;
            int item_move_y = 0;

            if (KensaHistoryRow["ItemMoveX"].ToString().Length > 0)
            {
                item_move_x = int.Parse(KensaHistoryRow["ItemMoveX"].ToString());
            }

            if (KensaHistoryRow["ItemMoveY"].ToString().Length > 0)
            {
                item_move_y = int.Parse(KensaHistoryRow["ItemMoveY"].ToString());
            }

            Color bgColor = Color.White;

            // 視力・眼圧の場合はグラフを描画する
            // 検査日と検査結果の組み合わせ（視力・眼圧）
            Dictionary<string, float> dateDict1 = new Dictionary<string, float>();
            Dictionary<string, float> dateDict2 = new Dictionary<string, float>();

            foreach (EyeKensa kensa in KensaList)
            {
                if (dateList.Contains(kensa.KensaDate))
                {
                    if (bgColor == Color.White)
                    {
                        bgColor = Color.LightYellow;
                    }
                    else
                    {
                        bgColor = Color.White;
                    }

                    // 検査パネルを作成する
                    Panel tmpPanel = new Panel();
                    tmpPanel.Size = new Size(panel_width, panel_height);
                    tmpPanel.Location = new Point(x, y + 20);
                    tmpPanel.AutoScroll = true;
                    tmpPanel.BackColor = bgColor;
                    tmpPanel.BorderStyle = BorderStyle.Fixed3D;

                    // 検査データのコントロールを作成する
                    DataRow[] tmpRows = EyeDict.EyeSet.Tables["KensaItem"].Select("KensaPage_ID = " + KensaPageRow["KensaPage_ID"].ToString());

                    foreach (DataRow tmpRow in tmpRows)
                    {
                        if (tmpRow["Type"].ToString().Equals("Label"))
                        {
                            Label tmpLabel = new Label();
                            tmpLabel.Name = tmpRow["Name"].ToString();
                            tmpLabel.Text = tmpRow["Text"].ToString();
                            tmpLabel.Location = new Point(int.Parse(tmpRow["X"].ToString()) + item_move_x, int.Parse(tmpRow["Y"].ToString()) + item_move_y);

                            tmpLabel.AutoSize = true;

                            tmpPanel.Controls.Add(tmpLabel);
                        }
                        else if (tmpRow["Type"].ToString().Equals("TextBox"))
                        {
                            TextBox tmpBox = new TextBox();
                            tmpBox.Name = tmpRow["Name"].ToString();
                            tmpBox.Text = tmpRow["Text"].ToString();
                            tmpBox.Location = new Point(int.Parse(tmpRow["X"].ToString()) + item_move_x, int.Parse(tmpRow["Y"].ToString()) + item_move_y);
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

                            tmpPanel.Controls.Add(tmpBox);
                        }
                        else if (tmpRow["Type"].ToString().Equals("ComboBox"))
                        {
                            ComboBox tmpBox = new ComboBox();
                            tmpBox.Name = tmpRow["Name"].ToString();
                            tmpBox.Text = tmpRow["Text"].ToString();
                            tmpBox.Location = new Point(int.Parse(tmpRow["X"].ToString()) + item_move_x, int.Parse(tmpRow["Y"].ToString()) + item_move_y);
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

                            tmpPanel.Controls.Add(tmpBox);
                        }
                        else if (tmpRow["Type"].ToString().Equals("CheckBox"))
                        {
                            CheckBox tmpBox = new CheckBox();
                            tmpBox.Name = tmpRow["Name"].ToString();
                            tmpBox.Text = tmpRow["Text"].ToString();
                            tmpBox.Location = new Point(int.Parse(tmpRow["X"].ToString()) + item_move_x, int.Parse(tmpRow["Y"].ToString()) + item_move_y);
                            tmpBox.Tag = tmpRow["Code"].ToString();
                            tmpBox.AutoSize = true;

                            tmpPanel.Controls.Add(tmpBox);
                        }
                    }

                    // パネル外にデータがあることを示すラベル
                    Label overLabel = new Label();
                    overLabel.Name = "OverLabel";
                    overLabel.AutoSize = false;
                    overLabel.Size = new Size(25, 12);
                    overLabel.Location = new Point(panel_width - 48, 3);
                    overLabel.BackColor = Color.Red;
                    overLabel.Visible = false;
                    tmpPanel.Controls.Add(overLabel);

                    // Doボタンを表示する
                    if (KensaDoRow["Visible"].ToString().Equals("1"))
                    {
                        Button doButton = new Button();
                        doButton.Name = "Do";
                        doButton.Text = "Do";
                        doButton.BackColor = Color.FromArgb(255, 192, 192);
                        doButton.Location = new Point(int.Parse(KensaDoRow["X"].ToString()), int.Parse(KensaDoRow["Y"].ToString()));
                        doButton.Size = new Size(int.Parse(KensaDoRow["Width"].ToString()), int.Parse(KensaDoRow["Height"].ToString()));
                        doButton.Click += new EventHandler(DoButton_Click);
                        tmpPanel.Controls.Add(doButton);
                    }

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

                    foreach (Control c in tmpPanel.Controls)
                    {
                        if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                        {
                            if (kensaDict.ContainsKey(c.Tag.ToString()))
                            {
                                c.Text = kensaDict[c.Tag.ToString()];

                                if (c.Location.X >= this.panel_width - 10 || c.Location.Y >= this.panel_height - 10)
                                {
                                    over_flg = true;
                                }
                            }
                        }
                        else if (c.GetType().Name.Equals("CheckBox"))
                        {
                            if (kensaDict.ContainsKey(c.Tag.ToString()))
                            {
                                ((CheckBox)c).Checked = true;

                                if (c.Location.X >= this.panel_width - 10 || c.Location.Y >= this.panel_height - 10)
                                {
                                    over_flg = true;
                                }
                            }
                        }
                    }

                    if (over_flg)
                    {
                        ((Label)(tmpPanel.Controls["OverLabel"])).Visible = true;
                    }
                    else
                    {
                        ((Label)(tmpPanel.Controls["OverLabel"])).Visible = false;
                    }

                    if (KensaPageRow["Name"].ToString().Equals("コントラスト感度"))
                    {
                        tmpPanel.Controls.Add(new ContrastPanel());
                        ((ContrastPanel)(tmpPanel.Controls["ContrastPanel"])).KensaShow();
                    }
                    else if (KensaPageRow["Name"].ToString().Equals("視力"))
                    {
                        if (kensaDict.ContainsKey("102R"))
                        {
                            float k = 0.0F;
                            float.TryParse(kensaDict["102R"], out k);

                            // n.c. の場合は 101R
                            if (kensaDict["102R"].StartsWith("n.c"))
                            {
                                if (kensaDict.ContainsKey("101R"))
                                {
                                    float.TryParse(kensaDict["101R"], out k);
                                }
                            }

                            dateDict1.Add(kensa.KensaDate, k);
                        }
                        else if (kensaDict.ContainsKey("109R"))
                        {
                            // 矯正不能裸眼視力
                            float k = 0.0F;
                            float.TryParse(kensaDict["109R"], out k);

                            dateDict1.Add(kensa.KensaDate, k);
                        }

                        if (kensaDict.ContainsKey("102L"))
                        {
                            float k = 0.0F;
                            float.TryParse(kensaDict["102L"], out k);

                            // n.c. の場合は 101L
                            if (kensaDict["102L"].StartsWith("n.c"))
                            {
                                if (kensaDict.ContainsKey("101L"))
                                {
                                    float.TryParse(kensaDict["101L"], out k);
                                }
                            }

                            dateDict2.Add(kensa.KensaDate, k);
                        }
                        else if (kensaDict.ContainsKey("109L"))
                        {
                            // 矯正不能裸眼視力
                            float k = 0.0F;
                            float.TryParse(kensaDict["109L"], out k);

                            dateDict2.Add(kensa.KensaDate, k);
                        }
                    }
                    else if (KensaPageRow["Name"].ToString().Equals("眼圧"))
                    {
                        if (kensaDict.ContainsKey("304R"))
                        {
                            float k = 0.0F;
                            float.TryParse(kensaDict["304R"], out k);

                            dateDict1.Add(kensa.KensaDate, k);
                        }

                        if (kensaDict.ContainsKey("304L"))
                        {
                            float k = 0.0F;
                            float.TryParse(kensaDict["304L"], out k);

                            dateDict2.Add(kensa.KensaDate, k);
                        }
                    }

                    // 検査日・作成者ラベルの表示
                    Label staffLabel = new Label();
                    staffLabel.Size = new Size(140, 18);
                    staffLabel.TextAlign = ContentAlignment.MiddleCenter;
                    staffLabel.BackColor = bgColor;
                    staffLabel.BorderStyle = BorderStyle.Fixed3D;
                    staffLabel.Location = new Point(x, y);

                    if (kensa.KensaDate.Length == 8)
                    {
                        staffLabel.Text = kensa.KensaDate.Substring(2, 6).Insert(2, "/").Insert(5, "/");
                    }

                    if (Dict.StaffDict.ContainsKey(kensa.Staff))
                    {
                        staffLabel.Text += "　" + Dict.StaffDict[kensa.Staff].Name;
                    }

                    this.ContentPanel.Controls.Add(staffLabel);

                    this.ContentPanel.Controls.Add(tmpPanel);

                    if (KensaHistoryRow["PanelDirection"].ToString().Equals("Horizontal"))
                    {
                        x += panel_width + 10;
                    }
                    else
                    {
                        y += panel_height + 30;
                    }
                }
            }

            if (KensaHistoryRow["PanelDirection"].ToString().Equals("Horizontal"))
            {
                this.ContentPanel.Size = new Size(x + 10, panel_height + 25);
            }
            else
            {
                this.ContentPanel.Size = new Size(panel_width + 10, y + 25);
            }

            int date_interval = 80;

            if (dateList.Count > 8 && dateList.Count <= 15)
            {
                date_interval = 70;
            }
            else if (dateList.Count > 15)
            {
                date_interval = 60;
            }

            if (KensaPageRow["Name"].ToString().Equals("視力"))
            {
                // 元のグラフがあれば削除する
                if (this.Controls.ContainsKey("Graph1"))
                {
                    this.Controls.RemoveByKey("Graph1");
                }

                if (this.Controls.ContainsKey("Graph2"))
                {
                    this.Controls.RemoveByKey("Graph2");
                }

                int form_width = int.Parse(KensaHistoryRow["FormWidth"].ToString());
                int form_height = int.Parse(KensaHistoryRow["FormHeight"].ToString());
                int p_height = (form_height - 100) / 2;

                this.Width = form_width + 50 + dateList.Count * date_interval;

                if (Screen.PrimaryScreen.Bounds.Width < this.Width + this.Location.X)
                {
                    this.Width = Screen.PrimaryScreen.Bounds.Width - this.Location.X;
                }

                PictureBox p1 = new PictureBox();
                p1.Name = "Graph1";
                p1.BackColor = Color.White;
                p1.Location = new Point(this.ContentPanel.Location.X + this.ContentPanel.Width + 10, 30);
                p1.Size = new Size(dateList.Count * date_interval + 50, p_height);
                p1.Image = new Bitmap(p1.Width, p1.Height);

                this.GraphDraw1("右", p1.Image, dateList, dateDict1, date_interval);

                PictureBox p2 = new PictureBox();
                p2.Name = "Graph2";
                p2.BackColor = Color.White;
                p2.Location = new Point(this.ContentPanel.Location.X + this.ContentPanel.Width + 10, 30 + p_height + 10);
                p2.Size = new Size(dateList.Count * date_interval + 50, p_height);
                p2.Image = new Bitmap(p2.Width, p2.Height);

                this.GraphDraw1("左", p2.Image, dateList, dateDict2, date_interval);

                this.Controls.Add(p1);
                this.Controls.Add(p2);
            }
            else if (KensaPageRow["Name"].ToString().Equals("眼圧"))
            {
                // 元のグラフがあれば削除する
                if (this.Controls.ContainsKey("Graph1"))
                {
                    this.Controls.RemoveByKey("Graph1");
                }

                if (this.Controls.ContainsKey("Graph2"))
                {
                    this.Controls.RemoveByKey("Graph2");
                }

                int form_width = int.Parse(KensaHistoryRow["FormWidth"].ToString());
                int form_height = int.Parse(KensaHistoryRow["FormHeight"].ToString());
                int p_height = (form_height - 100) / 2;

                this.Width = form_width + 50 + dateList.Count * date_interval;

                if (Screen.PrimaryScreen.Bounds.Width < this.Width + this.Location.X)
                {
                    this.Width = Screen.PrimaryScreen.Bounds.Width - this.Location.X;
                }

                PictureBox p1 = new PictureBox();
                p1.Name = "Graph1";
                p1.BackColor = Color.White;
                p1.Location = new Point(this.ContentPanel.Location.X + this.ContentPanel.Width + 10, 30);
                p1.Size = new Size(dateList.Count * date_interval + 50, p_height);
                p1.Image = new Bitmap(p1.Width, p1.Height);

                this.GraphDraw2("右", p1.Image, dateList, dateDict1, date_interval);

                PictureBox p2 = new PictureBox();
                p2.Name = "Graph2";
                p2.BackColor = Color.White;
                p2.Location = new Point(this.ContentPanel.Location.X + this.ContentPanel.Width + 10, 30 + p_height + 10);
                p2.Size = new Size(dateList.Count * date_interval + 50, p_height);
                p2.Image = new Bitmap(p2.Width, p2.Height);

                this.GraphDraw2("左", p2.Image, dateList, dateDict2, date_interval);

                this.Controls.Add(p1);
                this.Controls.Add(p2);
            }
        }

        /// <summary>
        /// 視力グラフ
        /// </summary>
        /// <param name="side_name"></param>
        /// <param name="im"></param>
        /// <param name="date_list"></param>
        /// <param name="date_dict"></param>
        void GraphDraw1(string side_name, Image im, List<string> date_list, Dictionary<string, float> date_dict, int date_interval)
        {
            Graphics g = Graphics.FromImage(im);
            
            Pen pen1 = new Pen(Brushes.Black, 1);
            Pen pen2 = new Pen(Brushes.LightGray, 1);
            Pen pen3 = new Pen(Brushes.Red, 2);

            // 縦軸の高さ
            int h = im.Height - 50;

            // 縦軸の目盛ごとの高さ
//            int iv_h = h / 4;
            int iv_h = h / 3;

            g.DrawLine(pen2, 40, h + 20, 40 + date_list.Count * date_interval, h + 20);
            g.DrawLine(pen2, 40, h + 20, 40, 20);

            Font f1 = new System.Drawing.Font("ＭＳ ゴシック", 9.0F, FontStyle.Regular);

            g.DrawString(side_name, f1, Brushes.Black, 40, 5);

            // 縦軸
            g.DrawLine(pen2, 40, 20, 40 + date_list.Count * date_interval, 20);
            g.DrawLine(pen2, 40, 1 * iv_h + 20, 40 + date_list.Count * date_interval, 1 * iv_h + 20);
            g.DrawLine(pen2, 40, 2 * iv_h + 20, 40 + date_list.Count * date_interval, 2 * iv_h + 20);

            g.DrawString("10.0", f1, Brushes.Black, 10, 15);
            g.DrawString("1.0", f1, Brushes.Black, 15, 1 * iv_h + 15);
            g.DrawString("0.1", f1, Brushes.Black, 15, 2 * iv_h + 15);
            g.DrawString("0.01", f1, Brushes.Black, 10, 3 * iv_h + 15);

            float px0 = 0.0F;
            float py0 = 0.0F;

            // 横軸と検査結果
            for (int i = 0; i < date_list.Count; i++)
            {
                g.DrawString(DateTimeAgent.DateFormat(date_list[i], DateTimeAgent.DateFormatKind.SHORT), f1, Brushes.Black, 40 + (date_list.Count - i - 1) * date_interval - 20, h + 25);
                g.DrawLine(pen2, 40 + (date_list.Count - i - 1) * date_interval, h + 20, 40 + (date_list.Count - i - 1) * date_interval, 20);

                if (date_dict.ContainsKey(date_list[i]))
                {
                    float k = date_dict[date_list[i]];
                    float px = 40 + (date_list.Count - i - 1) * date_interval;
                    float pyy = 0.0F;

                    // 表示する最小値を 0.01 とする
                    if (k >= 0.01F)
                    {
                        pyy = (float)Math.Round(Math.Log10(k), 3);
                    }
                    else
                    {
                        pyy = (float)Math.Round(Math.Log10(0.01F), 3);
                    }

                    // 1.0 が基準（ゼロ）となる
                    float py = 1 * iv_h + 20 - pyy * iv_h;

                    if (k >= 0.1F && k != 0.15F)
                    {
                        g.DrawString(k.ToString("F1"), f1, Brushes.Red, px, py - 20);
                    }
                    else if (k >= 0.01F)
                    {
                        g.DrawString(k.ToString("F2"), f1, Brushes.Red, px, py - 20);
                    }
                    else
                    {
                        g.DrawString(k.ToString("F3"), f1, Brushes.Red, px, py - 20);
                    }

                    g.FillRectangle(Brushes.Red, px - 5, py - 5, 10, 10);

                    if (px0 > 0)
                    {
                        g.DrawLine(pen3, px0, py0, px, py);
                    }

                    px0 = px;
                    py0 = py;
                }
            }
        }

        /// <summary>
        /// 眼圧グラフ
        /// </summary>
        /// <param name="side_name"></param>
        /// <param name="im"></param>
        /// <param name="date_list"></param>
        /// <param name="date_dict"></param>
        void GraphDraw2(string side_name, Image im, List<string> date_list, Dictionary<string, float> date_dict, int date_interval)
        {
            Graphics g = Graphics.FromImage(im);

            Pen pen1 = new Pen(Brushes.Black, 1);
            Pen pen2 = new Pen(Brushes.LightGray, 1);
            Pen pen3 = new Pen(Brushes.Red, 2);

            // 縦軸の高さ
            int h = im.Height - 50;

            // 縦軸の目盛ごとの値
            float iv_val = 0.0F;

            // 縦軸の目盛ごとの高さ
            int iv_h = 0;

            g.DrawLine(pen2, 40, h + 20, 40 + date_list.Count * date_interval, h + 20);
            g.DrawLine(pen2, 40, h + 20, 40, 20);

            Font f1 = new System.Drawing.Font("ＭＳ ゴシック", 9.0F, FontStyle.Regular);

            g.DrawString(side_name, f1, Brushes.Black, 40, 5);

            float max = 0.0F;

            // 最大値の取得
            foreach (float k in date_dict.Values)
            {
                if (k > max)
                {
                    max = k;
                }
            }

            // 最大値の調整
            if (max >= 40.0)
            {
                max = 80.0F;
                iv_val = 10.0F;
            }
            else if (max >= 20.0)
            {
                max = 40.0F;
                iv_val = 5.0F;
            }
            else
            {
                max = 20.0F;
                iv_val = 2.0F;
            }

            iv_h = h / (int)(max / iv_val);

            // 縦軸
            for (int i = 0; i < h / iv_h; i++)
            {
                float k = max - i * iv_val;

                g.DrawString((max - i * iv_val).ToString("F0").PadLeft(2, ' '), f1, Brushes.Black, 15, 20 + i * iv_h - 5);
                g.DrawLine(pen2, 40, 20 + i * iv_h, 40 + date_list.Count * date_interval, 20 + i * iv_h);
            }

            float px0 = 0.0F;
            float py0 = 0.0F;

            // 横軸と検査結果
            for (int i = 0; i < date_list.Count; i++)
            {
                g.DrawString(DateTimeAgent.DateFormat(date_list[i], DateTimeAgent.DateFormatKind.SHORT), f1, Brushes.Black, 40 + (date_list.Count - i - 1) * date_interval - 20, h + 25);
                g.DrawLine(pen2, 40 + (date_list.Count - i - 1) * date_interval, h + 20, 40 + (date_list.Count - i - 1) * date_interval, 20);

                if (date_dict.ContainsKey(date_list[i]))
                {
                    float k = date_dict[date_list[i]];
                    float px = 40 + (date_list.Count - i - 1) * date_interval;
                    float py = 20 + (max - k) / iv_val * iv_h;

                    g.DrawString(k.ToString("F1"), f1, Brushes.Red, px, py - 20);
                    g.FillRectangle(Brushes.Red, px - 5, py - 5, 10, 10);

                    if (px0 > 0)
                    {
                        g.DrawLine(pen3, px0, py0, px, py);
                    }

                    px0 = px;
                    py0 = py;
                }
            }
        }

        void DoButton_Click(object sender, EventArgs e)
        {
            Panel tmpPanel = (Panel)(((Button)sender).Parent);
            Dictionary<string, string> kensaDict = new Dictionary<string, string>();

            foreach (Control c in tmpPanel.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    if (c.Text.Length > 0)
                    {
                        kensaDict.Add(c.Tag.ToString(), c.Text);
                    }
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    if (((CheckBox)c).Checked)
                    {
                        kensaDict.Add(c.Tag.ToString(), "1");
                    }
                }
            }

            // 呼び出し元の FormPat
            if (this.Owner is FormPat)
            {
                ((FormPat)(this.Owner)).KensaDo(KensaId, kensaDict);
            }
        }

        private void ExcelButton_Click(object sender, EventArgs e)
        {
            Excel.Application exApp;
            Excel._Workbook exWorkbook;
            Excel._Worksheet exWorksheet;

            exApp = new Excel.Application();
            exApp.Visible = true;

            exWorkbook = (Excel._Workbook)(exApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet));
            exWorksheet = (Excel._Worksheet)(exWorkbook.Sheets[exWorkbook.Sheets.Count]);

            try
            {
                Dictionary<string, string> code_dict = new Dictionary<string, string>();

                exWorksheet.Cells[1, 1] = "患者ID";
                exWorksheet.Cells[1, 2] = "カナ";
                exWorksheet.Cells[1, 3] = "氏名";
                exWorksheet.Cells[1, 4] = "性別";
                exWorksheet.Cells[1, 5] = "生年月日";
                exWorksheet.Cells[1, 6] = "年齢";
                exWorksheet.Cells[1, 7] = "検査日";

                // 検査データのコントロールを作成する
                DataRow[] tmpRows = EyeDict.EyeSet.Tables["KensaItem"].Select("KensaPage_ID = " + EyeDict.EyeSet.Tables["KensaPage"].Select("ID = '" + KensaId + "'")[0]["KensaPage_ID"].ToString());

                int j = 8;

                foreach (DataRow r in tmpRows)
                {
                    if (r["Type"].ToString().Equals("TextBox") || r["Type"].ToString().Equals("ComboBox") || r["Type"].ToString().Equals("CheckBox"))
                    {
                        code_dict.Add(r["Code"].ToString(), r["Name"].ToString());
                        exWorksheet.Cells[1, j] = r["Name"].ToString();
                        j++;
                    }
                }

                int i = 2;

                Dictionary<string, string> cont_dict = new Dictionary<string, string>();
                string key = "";
                string value = "";

                List<EyeKensa> kensa_list = EyeKensa.LoadByPatKensa(PtId, KensaId, true);

                foreach (EyeKensa tmpKensa in kensa_list)
                {
                    if (tmpKensa.Cont.Length > 0)
                    {
                        cont_dict.Clear();

                        foreach (string s in tmpKensa.Cont.Split('\r', '\n'))
                        {
                            key = s.Split(',')[0];
                            value = "";

                            for (int k = 1; k < s.Split(',').Length; k++)
                            {
                                if (value.Length > 0)
                                {
                                    value += ",";
                                }

                                value += s.Split(',')[k].Replace("<CR+LF>", "\r\n");
                            }

                            if (!cont_dict.ContainsKey(key))
                            {
                                cont_dict.Add(key, value);
                            }
                        }

                        exWorksheet.Cells[i, 1] = tmpKensa.Pat.Id;
                        exWorksheet.Cells[i, 2] = tmpKensa.Pat.Kana;
                        exWorksheet.Cells[i, 3] = tmpKensa.Pat.Name;
                        exWorksheet.Cells[i, 4] = tmpKensa.Pat.SexNameEng;
                        exWorksheet.Cells[i, 5] = tmpKensa.Pat.BirthString;
                        exWorksheet.Cells[i, 6] = tmpKensa.Pat.AgeCalc(tmpKensa.KensaDate);
                        exWorksheet.Cells[i, 7] = DateTimeAgent.DateFormat(tmpKensa.KensaDate, DateTimeAgent.DateFormatKind.LONG);

                        j = 8;

                        foreach (string code in code_dict.Keys)
                        {
                            if (cont_dict.ContainsKey(code))
                            {
                                exWorksheet.Cells[i, j] = cont_dict[code];
                            }

                            j++;
                        }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                LibUtility.Except(ex, true);
            }
            finally
            {
                Marshal.ReleaseComObject(exWorksheet);
                Marshal.ReleaseComObject(exWorkbook);
                Marshal.ReleaseComObject(exApp);

                GC.Collect();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}