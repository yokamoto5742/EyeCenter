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
    class ControlSumPage
    {
        FormPat FP;
        ControlIVPage IVPage;

        DataSet DSet;

        /// <summary>
        /// サマリを初期化する。
        /// </summary>
        public void Init(FormPat fp, ControlIVPage ip)
        {
            FP = fp;
            IVPage = ip;

            DSet = new DataSet();

            DataRow tmpRow = EyeDict.EyeSet.Tables["Summary"].Rows[0];

            FP.SumDiagBox.Items.Add("");

            foreach (string s in tmpRow["Diag"].ToString().Split(','))
            {
                FP.SumDiagBox.Items.Add(s);
            }

            FP.SumKindBox1.Items.Add("");

            foreach (string s in tmpRow["Kind1"].ToString().Split(','))
            {
                FP.SumKindBox1.Items.Add(s);
            }

            FP.SumKindBox2.Items.Add("");

            foreach (string s in tmpRow["Kind2"].ToString().Split(','))
            {
                FP.SumKindBox2.Items.Add(s);
            }

            FP.SumKindBox3.Items.Add("");

            foreach (string s in tmpRow["Kind3"].ToString().Split(','))
            {
                FP.SumKindBox3.Items.Add(s);
            }

            int pos_x = 5;
            int pos_y = 5;

            int c_width = 100;
            int c_height = 20;

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem1"].Rows)
            {
                if (r["Label"].ToString().Length > 0)
                {
                    Label tmpLabel = new Label();
                    tmpLabel.Name = r["Code"].ToString() + "_L";
                    tmpLabel.Location = new Point(pos_x, pos_y + 2);
                    tmpLabel.AutoSize = true;
                    tmpLabel.Text = r["Label"].ToString();

                    FP.SumPanel1.Controls.Add(tmpLabel);
                }

                c_width = 100;
                c_height = 20;

                if (r["Type"].ToString().Equals("TextBox"))
                {
                    TextBox tmpBox = new TextBox();
                    tmpBox.Name = r["Code"].ToString() + "_C";
                    tmpBox.Location = new Point(pos_x + 80, pos_y);

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
//                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    if (r["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (r["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else if (r["Ime"].ToString().Equals("Disable"))
                    {
                        tmpBox.ImeMode = ImeMode.Disable;
                    }

                    if (r["Multiline"].ToString().Equals("1"))
                    {
                        tmpBox.Multiline = true;
                    }

                    FP.SumPanel1.Controls.Add(tmpBox);
                }
                else if (r["Type"].ToString().Equals("ComboBox"))
                {
                    ComboBox tmpBox = new ComboBox();
                    tmpBox.Name = r["Code"].ToString() + "_C";
                    tmpBox.Location = new Point(pos_x + 80, pos_y);

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
//                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    if (r["Item"].ToString().Length > 0)
                    {
                        foreach (string s in r["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(s);
                        }
                    }

                    if (r["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (r["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else if (r["Ime"].ToString().Equals("Disable"))
                    {
                        tmpBox.ImeMode = ImeMode.Disable;
                    }

                    FP.SumPanel1.Controls.Add(tmpBox);
                }
                else if (r["Type"].ToString().Equals("CheckBox"))
                {
                    CheckBox tmpBox = new CheckBox();
                    tmpBox.Name = r["Code"].ToString() + "_C";
                    tmpBox.Location = new Point(pos_x + 80, pos_y);

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    FP.SumPanel1.Controls.Add(tmpBox);
                }

                pos_y += c_height + 2;
            }

            int pos_x1 = 5;
            int pos_x2 = 215;

            int pos_y1 = 5;
            int pos_y2 = 5;

            Size size_d = new Size(75, 20);
            Size size_s = new Size(45, 20);
            Size size_c = new Size(10, 20);

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
            {
                if (r["Label"].ToString().Length > 0)
                {
                    if (r["Line"].ToString().Equals("1"))
                    {
                        Label tmpLabel1 = new Label();
                        tmpLabel1.Name = r["Code"].ToString() + "_L";
                        tmpLabel1.AutoSize = true;
                        tmpLabel1.Text = r["Label"].ToString();
                        tmpLabel1.Location = new Point(pos_x1, pos_y1 + 2);

                        FP.SumPanel2.Controls.Add(tmpLabel1);

                        Label tmpLabel2 = new Label();
                        tmpLabel2.Name = r["Code"].ToString() + "_D";
                        tmpLabel2.BackColor = Color.LightYellow;
                        tmpLabel2.Location = new Point(pos_x1 + 75, pos_y1);
                        tmpLabel2.Size = size_d;
                        tmpLabel2.TextAlign = ContentAlignment.MiddleCenter;
                        tmpLabel2.DoubleClick += new EventHandler(Label2_DoubleClick);

                        FP.SumPanel2.Controls.Add(tmpLabel2);

                        Label tmpLabel3 = new Label();
                        tmpLabel3.Name = r["Code"].ToString() + "_S";
                        tmpLabel3.BackColor = Color.LightYellow;
                        tmpLabel3.Location = new Point(pos_x1 + 153, pos_y1);
                        tmpLabel3.Size = size_s;
                        tmpLabel3.TextAlign = ContentAlignment.MiddleCenter;
                        tmpLabel3.DoubleClick += new EventHandler(Label2_DoubleClick);

                        FP.SumPanel2.Controls.Add(tmpLabel3);

                        Label tmpLabel4 = new Label();
                        tmpLabel4.Name = r["Code"].ToString() + "_C";
                        tmpLabel4.Location = new Point(pos_x1 + 153, pos_y1);
                        tmpLabel4.Size = size_c;
                        tmpLabel4.Visible = false;

                        FP.SumPanel2.Controls.Add(tmpLabel4);

                        pos_y1 += 22;
                    }
                    else if (r["Line"].ToString().Equals("2"))
                    {
                        Label tmpLabel1 = new Label();
                        tmpLabel1.Name = r["Code"].ToString() + "_L";
                        tmpLabel1.AutoSize = true;
                        tmpLabel1.Text = r["Label"].ToString();
                        tmpLabel1.Location = new Point(pos_x2, pos_y2 + 2);

                        FP.SumPanel2.Controls.Add(tmpLabel1);

                        Label tmpLabel2 = new Label();
                        tmpLabel2.Name = r["Code"].ToString() + "_D";
                        tmpLabel2.BackColor = Color.LightYellow;
                        tmpLabel2.Location = new Point(pos_x2 + 75, pos_y2);
                        tmpLabel2.Size = size_d;
                        tmpLabel2.TextAlign = ContentAlignment.MiddleCenter;
                        tmpLabel2.DoubleClick += new EventHandler(Label2_DoubleClick);

                        FP.SumPanel2.Controls.Add(tmpLabel2);

                        Label tmpLabel3 = new Label();
                        tmpLabel3.Name = r["Code"].ToString() + "_S";
                        tmpLabel3.BackColor = Color.LightYellow;
                        tmpLabel3.Location = new Point(pos_x2 + 153, pos_y2);
                        tmpLabel3.Size = size_s;
                        tmpLabel3.TextAlign = ContentAlignment.MiddleCenter;
                        tmpLabel3.DoubleClick += new EventHandler(Label2_DoubleClick);

                        FP.SumPanel2.Controls.Add(tmpLabel3);

                        Label tmpLabel4 = new Label();
                        tmpLabel4.Name = r["Code"].ToString() + "_C";
                        tmpLabel4.Location = new Point(pos_x2 + 153, pos_y2);
                        tmpLabel4.Size = size_c;
                        tmpLabel4.Visible = false;

                        FP.SumPanel2.Controls.Add(tmpLabel4);

                        pos_y2 += 22;
                    }
                }
            }

            pos_x = 5;
            pos_y = 5;

            c_width = 100;
            c_height = 20;

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem3"].Rows)
            {
                if (r["Label"].ToString().Length > 0)
                {
                    Label tmpLabel = new Label();
                    tmpLabel.Name = r["Code"].ToString() + "_L";
                    tmpLabel.Location = new Point(pos_x, pos_y + 2);
                    tmpLabel.AutoSize = true;
                    tmpLabel.Text = r["Label"].ToString();

                    FP.SumPanel3.Controls.Add(tmpLabel);
                }

                c_width = 100;
                c_height = 20;

                if (r["Type"].ToString().Equals("TextBox"))
                {
                    TextBox tmpBox = new TextBox();
                    tmpBox.Name = r["Code"].ToString() + "_C";
                    tmpBox.Location = new Point(pos_x + 80, pos_y);

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
//                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    if (r["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (r["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else if (r["Ime"].ToString().Equals("Disable"))
                    {
                        tmpBox.ImeMode = ImeMode.Disable;
                    }

                    if (r["Multiline"].ToString().Equals("1"))
                    {
                        tmpBox.Multiline = true;
                    }

                    if (r["Code"].ToString().Equals("301") || r["Code"].ToString().Equals("302"))
                    {
                        tmpBox.TextChanged += new EventHandler(Box3_TextChanged);
                    }

                    FP.SumPanel3.Controls.Add(tmpBox);
                }
                else if (r["Type"].ToString().Equals("ComboBox"))
                {
                    ComboBox tmpBox = new ComboBox();
                    tmpBox.Name = r["Code"].ToString() + "_C";
                    tmpBox.Location = new Point(pos_x + 80, pos_y);

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
//                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    if (r["Item"].ToString().Length > 0)
                    {
                        foreach (string s in r["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(s);
                        }
                    }

                    if (r["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (r["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else if (r["Ime"].ToString().Equals("Disable"))
                    {
                        tmpBox.ImeMode = ImeMode.Disable;
                    }

                    FP.SumPanel3.Controls.Add(tmpBox);
                }
                else if (r["Type"].ToString().Equals("CheckBox"))
                {
                    CheckBox tmpBox = new CheckBox();
                    tmpBox.Name = r["Code"].ToString() + "_C";
                    tmpBox.Location = new Point(pos_x + 80, pos_y);

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    FP.SumPanel3.Controls.Add(tmpBox);
                }

                pos_y += c_height + 2;
            }

            c_width = 100;
            c_height = 20;

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem4"].Rows)
            {
                c_width = 100;
                c_height = 20;

                if (r["Type"].ToString().Equals("Label"))
                {
                    Label tmpLabel = new Label();
                    tmpLabel.Name = r["Name"].ToString();
                    tmpLabel.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));
                    tmpLabel.AutoSize = true;
                    tmpLabel.Text = r["Text"].ToString();

                    FP.SumPanel4.Controls.Add(tmpLabel);
                }
                else if (r["Type"].ToString().Equals("TextBox"))
                {
                    TextBox tmpBox = new TextBox();
                    tmpBox.Name = r["Name"].ToString();
                    tmpBox.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
//                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    if (r["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (r["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else if (r["Ime"].ToString().Equals("Disable"))
                    {
                        tmpBox.ImeMode = ImeMode.Disable;
                    }

                    if (r["Multiline"].ToString().Equals("1"))
                    {
                        tmpBox.Multiline = true;
                    }

                    FP.SumPanel4.Controls.Add(tmpBox);
                }
                else if (r["Type"].ToString().Equals("ComboBox"))
                {
                    ComboBox tmpBox = new ComboBox();
                    tmpBox.Name = r["Name"].ToString();
                    tmpBox.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
//                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    if (r["Item"].ToString().Length > 0)
                    {
                        foreach (string s in r["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(s);
                        }
                    }

                    if (r["Ime"].ToString().Equals("Hiragana"))
                    {
                        tmpBox.ImeMode = ImeMode.Hiragana;
                    }
                    else if (r["Ime"].ToString().Equals("Off"))
                    {
                        tmpBox.ImeMode = ImeMode.Off;
                    }
                    else if (r["Ime"].ToString().Equals("Disable"))
                    {
                        tmpBox.ImeMode = ImeMode.Disable;
                    }

                    FP.SumPanel4.Controls.Add(tmpBox);
                }
                else if (r["Type"].ToString().Equals("CheckBox"))
                {
                    CheckBox tmpBox = new CheckBox();
                    tmpBox.Name = r["Name"].ToString();
                    tmpBox.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));

                    if (r["Width"].ToString().Length > 0)
                    {
                        int.TryParse(r["Width"].ToString(), out c_width);
                    }

                    if (r["Height"].ToString().Length > 0)
                    {
                        int.TryParse(r["Height"].ToString(), out c_height);
                    }

                    tmpBox.Size = new Size(c_width, c_height);
                    tmpBox.Text = r["Text"].ToString();
                    tmpBox.Tag = r["Code"].ToString();

                    FP.SumPanel4.Controls.Add(tmpBox);
                }
            }
        }

        void Box3_TextChanged(object sender, EventArgs e)
        {
            if (FP.SumPanel3.Controls.ContainsKey("301_C") && FP.SumPanel3.Controls.ContainsKey("302_C") && FP.SumPanel3.Controls.ContainsKey("303_C"))
            {
                string s1 = FP.SumPanel3.Controls["301_C"].Text;
                string s2 = FP.SumPanel3.Controls["302_C"].Text;

                int days = 0;

                if (s1.Length > 0 && s2.Length > 0 && int.TryParse(s2, out days))
                {
                    string in_year = "";
                    string in_month = "";
                    string in_day = "";

                    if (s1.Split('/', '.').Length > 2)
                    {
                        in_year = s1.Split('/', '.')[0];

                        if (in_year.Length == 2)
                        {
                            in_year = "20" + in_year;
                        }

                        in_month = s1.Split('/', '.')[1].PadLeft(2, '0');
                        in_day = s1.Split('/', '.')[2].PadLeft(2, '0');
                    }
                    else if (s1.Split('/', '.').Length > 1)
                    {
                        in_year = DateTime.Now.ToString("yyyy");
                        in_month = s1.Split('/', '.')[0].PadLeft(2, '0');
                        in_day = s1.Split('/', '.')[1].PadLeft(2, '0');
                    }

                    DateTime d1 = new DateTime();
                    DateTime.TryParse(in_year + "/" + in_month + "/" + in_day, out d1);

                    FP.SumPanel3.Controls["303_C"].Text = d1.AddDays(days).ToString("M/d");
                }
            }
        }

        void Label2_DoubleClick(object sender, EventArgs e)
        {
            Label tmpLabel = (Label)sender;
            string code = tmpLabel.Name.Split('_')[0];
            Label date_label = (Label)(FP.SumPanel2.Controls[code + "_D"]);
            Label staff_label = (Label)(FP.SumPanel2.Controls[code + "_S"]);
            Label code_label = (Label)(FP.SumPanel2.Controls[code + "_C"]);

            FormSumPlan2 fp2 = new FormSumPlan2(date_label, staff_label, code_label);
            fp2.ShowDialog();
        }

        /// <summary>
        /// SumPanel2（検査予定）をクリアする。
        /// </summary>
        public void Panel2Clear()
        {
            foreach (Control c in FP.SumPanel2.Controls)
            {
                if (c.Name.EndsWith("_D") || c.Name.EndsWith("_S") || c.Name.EndsWith("_C"))
                {
                    c.Text = "";
                }
            }
        }

        /// <summary>
        /// サマリをクリアする。
        /// </summary>
        public void Clear()
        {
            FP.SumDiagBox.Text = "";
            FP.SumKindBox1.Text = "";
            FP.SumKindBox2.Text = "";
            FP.SumKindBox3.Text = "";
            FP.SumPlanBox.Clear();
            FP.SumPassBox.Clear();
            FP.SumHistBox.Clear();
            FP.SumSaveDateLabel.Text = "";
            FP.SumStaffLabel.Text = "";

            foreach (Control c in FP.SumPanel1.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    c.Text = "";
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    ((CheckBox)c).Checked = false;
                }
            }

            Panel2Clear();

            foreach (Control c in FP.SumPanel3.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    c.Text = "";
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    ((CheckBox)c).Checked = false;
                }
            }

            foreach (Control c in FP.SumPanel4.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    c.Text = "";
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    ((CheckBox)c).Checked = false;
                }
            }

            this.IVPage.Clear();
        }

        /// <summary>
        /// SumPanel1 に初期値をセットする。
        /// </summary>
        public void InitValue1()
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem1"].Rows)
            {
                if (r["Text"].ToString().Length > 0 && (r["Type"].ToString().Equals("TextBox") || r["Type"].ToString().Equals("ComboBox")))
                {
                    if (FP.SumPanel1.Controls.ContainsKey(r["Code"].ToString() + "_C"))
                    {
                        FP.SumPanel1.Controls[r["Code"].ToString() + "_C"].Text = r["Text"].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// SumPanel3 に初期値をセットする。
        /// </summary>
        public void InitValue3()
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem3"].Rows)
            {
                if (r["Text"].ToString().Length > 0 && (r["Type"].ToString().Equals("TextBox") || r["Type"].ToString().Equals("ComboBox")))
                {
                    if (FP.SumPanel3.Controls.ContainsKey(r["Code"].ToString() + "_C"))
                    {
                        FP.SumPanel3.Controls[r["Code"].ToString() + "_C"].Text = r["Text"].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// SumPanel4 に初期値をセットする。
        /// </summary>
        public void InitValue4()
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem4"].Rows)
            {
                if (r["Text"].ToString().Length > 0 && (r["Type"].ToString().Equals("TextBox") || r["Type"].ToString().Equals("ComboBox")))
                {
                    if (FP.SumPanel4.Controls.ContainsKey(r["Name"].ToString()))
                    {
                        FP.SumPanel4.Controls[r["Name"].ToString()].Text = r["Text"].ToString();
                    }
                }
            }
        }

        /// <summary>
        /// サマリを表示する。
        /// </summary>
        /// <param name="pt_id"></param>
        public bool Show(string pt_id)
        {
            Clear();

            if (pt_id.Length == 0)
            {
                return false;
            }

            this.IVPage.HistoryShow(pt_id);

            EyeSummary sum = EyeSummary.Load(pt_id);

            if (sum.PtId.Length == 0)
            {
                return false;
            }

            FP.SumDiagBox.Text = sum.Diag;
            FP.SumKindBox1.Text = sum.Kind1;
            FP.SumKindBox2.Text = sum.Kind2;
            FP.SumKindBox3.Text = sum.Kind3;
            FP.SumPlanBox.Text = sum.Plan;
            FP.SumPassBox.Text = sum.Pass;
            FP.SumHistBox.Text = sum.Hist;

            string value = "";
            string[] ss;
            
            // SumPanel1
            string[] cont = sum.Cont1.Split('\r', '\n');

            foreach (string s in cont)
            {
                if (s.Contains(","))
                {
                    ss = s.Split(',');
                    value = "";

                    for (int i = 1; i < ss.Length; i++)
                    {
                        if (value.Length > 0)
                        {
                            value += ",";
                        }

                        value += ss[i].Replace("<CR+LF>", "\r\n");
                    }

                    if (FP.SumPanel1.Controls.ContainsKey(ss[0] + "_C"))
                    {
                        Control c = FP.SumPanel1.Controls[ss[0] + "_C"];

                        if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                        {
                            c.Text = value;
                        }
                        else if (c.GetType().Name.Equals("CheckBox") && value.Equals("1"))
                        {
                            ((CheckBox)c).Checked = true;
                        }
                    }
                }
            }

            // SumPanel2
            cont = sum.Cont2.Split('\r', '\n');

            string kensa_date = "";
            string kensa_code = "";
            string kensa_staff = "";

            foreach (string s in cont)
            {
                if (s.Contains(","))
                {
                    ss = s.Split(',');

                    if (ss.Length > 1 && ss[1].Contains(" "))
                    {
                        kensa_date = ss[1].Split(' ')[0];
                        kensa_code = ss[1].Split(' ')[1];

                        if (Dict.StaffDict.ContainsKey(kensa_code))
                        {
                            kensa_staff = Dict.StaffDict[kensa_code].Name.Replace('　', ' ').Split(' ')[0];
                        }
                        else
                        {
                            kensa_staff = "";
                        }

                        if (FP.SumPanel2.Controls.ContainsKey(ss[0] + "_D"))
                        {
                            FP.SumPanel2.Controls[ss[0] + "_D"].Text = kensa_date.Insert(4, "/").Insert(7, "/");
                        }

                        if (FP.SumPanel2.Controls.ContainsKey(ss[0] + "_S"))
                        {
                            FP.SumPanel2.Controls[ss[0] + "_S"].Text = kensa_staff;
                        }

                        if (FP.SumPanel2.Controls.ContainsKey(ss[0] + "_C"))
                        {
                            FP.SumPanel2.Controls[ss[0] + "_C"].Text = kensa_code;
                        }
                    }
                }
            }

            // SumPanel3
            cont = sum.Cont3.Split('\r', '\n');

            foreach (string s in cont)
            {
                if (s.Contains(","))
                {
                    ss = s.Split(',');
                    value = "";

                    for (int i = 1; i < ss.Length; i++)
                    {
                        if (value.Length > 0)
                        {
                            value += ",";
                        }

                        value += ss[i].Replace("<CR+LF>", "\r\n");
                    }

                    if (FP.SumPanel3.Controls.ContainsKey(ss[0] + "_C"))
                    {
                        Control c = FP.SumPanel3.Controls[ss[0] + "_C"];

                        if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                        {
                            c.Text = value;
                        }
                        else if (c.GetType().Name.Equals("CheckBox") && value.Equals("1"))
                        {
                            ((CheckBox)c).Checked = true;
                        }
                    }
                }
            }

            // SumPanel4
            cont = sum.Cont4.Split('\r', '\n');

            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (string s in cont)
            {
                if (s.Contains(","))
                {
                    ss = s.Split(',');
                    value = "";

                    for (int i = 1; i < ss.Length; i++)
                    {
                        if (value.Length > 0)
                        {
                            value += ",";
                        }

                        value += ss[i].Replace("<CR+LF>", "\r\n");
                    }

                    if (!dict.ContainsKey(ss[0]))
                    {
                        dict.Add(ss[0], value);
                    }
                }
            }

            foreach (Control c in FP.SumPanel4.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    if (dict.ContainsKey(c.Tag.ToString()))
                    {
                        c.Text = dict[c.Tag.ToString()];
                    }
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    if (dict.ContainsKey(c.Tag.ToString()) && dict[c.Tag.ToString()].Equals("1"))
                    {
                        ((CheckBox)c).Checked = true;
                    }
                }
            }

            FP.SumSaveDateLabel.Text = DateTimeAgent.DateFormat(sum.SaveDate, DateTimeAgent.DateFormatKind.LONG) + " " + DateTimeAgent.TimeFormat6(sum.SaveTime, 4, true);

            if (Dict.StaffDict.ContainsKey(sum.Staff))
            {
                FP.SumStaffLabel.Text = Dict.StaffDict[sum.Staff].Name;
            }

            return true;
        }

        /// <summary>
        /// サマリーを登録する。
        /// </summary>
        public bool Save(string pt_id)
        {
            int p = 0;

            if (!int.TryParse(pt_id, out p))
            {
                return false;
            }

            EyeSummary sum = new EyeSummary();

            sum.PtId = pt_id;
            sum.Diag = FP.SumDiagBox.Text;
            sum.Kind1 = FP.SumKindBox1.Text;
            sum.Kind2 = FP.SumKindBox2.Text;
            sum.Kind3 = FP.SumKindBox3.Text;
            sum.Plan = FP.SumPlanBox.Text;
            sum.Pass = FP.SumPassBox.Text;
            sum.Hist = FP.SumHistBox.Text;
            sum.Staff = LoginUser.Id;

            string value = "";

            foreach (Control c in FP.SumPanel1.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    if (c.Text.Length > 0)
                    {
                        value += c.Tag.ToString() + "," + c.Text.Replace("\r\n", "<CR+LF>") + "\r\n";
                    }
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    if (((CheckBox)c).Checked)
                    {
                        value += c.Tag.ToString() + ",1\r\n";
                    }
                }
            }

            sum.Cont1 = value;

            value = "";
            string code = "";

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
            {
                code = r["Code"].ToString();

                if (FP.SumPanel2.Controls[code + "_D"].Text.Length > 0 && FP.SumPanel2.Controls[code + "_C"].Text.Length > 0)
                {
                    value += code + "," + FP.SumPanel2.Controls[code + "_D"].Text.Replace("/", "") + " " + FP.SumPanel2.Controls[code + "_C"].Text + "\r\n";
                }
            }

            sum.Cont2 = value;

            value = "";

            foreach (Control c in FP.SumPanel3.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    if (c.Text.Length > 0)
                    {
                        value += c.Tag.ToString() + "," + c.Text.Replace("\r\n", "<CR+LF>") + "\r\n";
                    }
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    if (((CheckBox)c).Checked)
                    {
                        value += c.Tag.ToString() + ",1\r\n";
                    }
                }
            }

            sum.Cont3 = value;

            value = "";

            foreach (Control c in FP.SumPanel4.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") || c.GetType().Name.Equals("ComboBox"))
                {
                    if (c.Text.Length > 0)
                    {
                        value += c.Tag.ToString() + "," + c.Text.Replace("\r\n", "<CR+LF>") + "\r\n";
                    }
                }
                else if (c.GetType().Name.Equals("CheckBox"))
                {
                    if (((CheckBox)c).Checked)
                    {
                        value += c.Tag.ToString() + ",1\r\n";
                    }
                }
            }

            sum.Cont4 = value;

            sum.Save();

            FP.SumSaveDateLabel.Text = DateTime.Now.ToString("yyyy/MM/dd HH:mm");
            FP.SumStaffLabel.Text = LoginUser.Name;

            return true;
        }
    }
}
