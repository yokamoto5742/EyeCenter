using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    public partial class FormSumPlan1 : Form
    {
        FormPat FP;

        public FormSumPlan1(FormPat fp)
        {
            this.FP = fp;
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void FormSumPlan1_Load(object sender, EventArgs e)
        {
            string sum_staff = EyeDict.EyeSet.Tables["SumStaff"].Rows[0]["Code"].ToString();

            StaffBox.Items.Add("");

            foreach (string s in sum_staff.Split(','))
            {
                if (Dict.StaffDict.ContainsKey(s.Trim()))
                {
                    StaffBox.Items.Add(s.Trim() + " " + Dict.StaffDict[s.Trim()].Name);
                }
            }

            StaffBox.Text = LoginUser.Id + " " + LoginUser.Name;

            int pos_x1 = 5;
            int pos_x2 = 105;

            int pos_y1 = 5;
            int pos_y2 = 5;

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumItem2"].Rows)
            {
                if (r["Label"].ToString().Length > 0)
                {
                    if (r["Line"].ToString().Equals("1"))
                    {
                        CheckBox tmpBox = new CheckBox();
                        tmpBox.Name = r["Code"].ToString();
                        tmpBox.Text = r["Label"].ToString();
                        tmpBox.AutoSize = true;
                        tmpBox.Location = new Point(pos_x1, pos_y1);

                        KensaPanel.Controls.Add(tmpBox);

                        pos_y1 += 22;
                    }
                    else if (r["Line"].ToString().Equals("2"))
                    {
                        CheckBox tmpBox = new CheckBox();
                        tmpBox.Name = r["Code"].ToString();
                        tmpBox.Text = r["Label"].ToString();
                        tmpBox.AutoSize = true;
                        tmpBox.Location = new Point(pos_x2, pos_y2);

                        KensaPanel.Controls.Add(tmpBox);

                        pos_y2 += 22;
                    }
                }
            }

            Size tmpSize = new Size(60, 20);

            foreach (DataRow r in EyeDict.EyeSet.Tables["SumSet2"].Rows)
            {
                if (r["Name"].ToString().Length > 0)
                {
                    Button tmpButton = new Button();
                    tmpButton.Size = tmpSize;
                    tmpButton.Text = r["Name"].ToString();
                    tmpButton.Tag = r["Code"].ToString();
                    tmpButton.Click += new EventHandler(SetButton_Click);

                    SetPanel.Controls.Add(tmpButton);
                }
            }
        }

        void SetButton_Click(object sender, EventArgs e)
        {
            Button tmpButton = (Button)sender;

            foreach (string s in tmpButton.Tag.ToString().Split(','))
            {
                if (KensaPanel.Controls.ContainsKey(s.Trim()))
                {
                    ((CheckBox)(KensaPanel.Controls[s.Trim()])).Checked = true;
                }
            }
        }

        private void InputButton_Click(object sender, EventArgs e)
        {
            if (!StaffBox.Text.Contains(" "))
            {
                MessageBox.Show("íSìñé“ÇëIëÇµÇƒÇ≠ÇæÇ≥Ç¢");
                return;
            }

            foreach (Control c in KensaPanel.Controls)
            {
                if (c.GetType().Name.Equals("CheckBox") && ((CheckBox)c).Checked)
                {
                    if (FP.SumPanel2.Controls.ContainsKey(c.Name + "_L"))
                    {
                        FP.SumPanel2.Controls[c.Name + "_D"].Text = KensaDate.Value.ToString("yyyy/MM/dd");
                        FP.SumPanel2.Controls[c.Name + "_S"].Text = StaffBox.Text.Split(' ')[1].Replace('Å@', ' ').Split(' ')[0];
                        FP.SumPanel2.Controls[c.Name + "_C"].Text = StaffBox.Text.Split(' ')[0];
                    }
                }
            }

            this.Dispose();
        }
    }
}