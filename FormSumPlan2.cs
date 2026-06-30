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
    public partial class FormSumPlan2 : Form
    {
        Label DateLabel;
        Label StaffLabel;
        Label CodeLabel;

        public FormSumPlan2(Label date_label, Label staff_label, Label code_label)
        {
            InitializeComponent();

            DateLabel = date_label;
            StaffLabel = staff_label;
            CodeLabel = code_label;
        }

        private void FormSumPlan2_Load(object sender, EventArgs e)
        {
            if (DateLabel.Text.Length > 0)
            {
                KensaDate.Value = DateTime.Parse(DateLabel.Text);
            }

            string sum_staff = EyeDict.EyeSet.Tables["SumStaff"].Rows[0]["Code"].ToString();
            StaffBox.Items.Add("");

            foreach (string s in sum_staff.Split(','))
            {
                if (Dict.StaffDict.ContainsKey(s.Trim()))
                {
                    StaffBox.Items.Add(s.Trim() + " " + Dict.StaffDict[s.Trim()].Name);
                }
            }

            if (Dict.StaffDict.ContainsKey(CodeLabel.Text))
            {
                StaffBox.Text = CodeLabel.Text + " " + Dict.StaffDict[CodeLabel.Text].Name;
            }
        }

        private void InputButton_Click(object sender, EventArgs e)
        {
            if (StaffBox.Text.Length == 0)
            {
                MessageBox.Show("’S“–ŽÒ‚ð‘I‘ð‚µ‚Ä‚­‚¾‚³‚¢");
                return;
            }

            DateLabel.Text = KensaDate.Value.ToString("yyyy/MM/dd");
            CodeLabel.Text = StaffBox.Text.Split(' ')[0];
            StaffLabel.Text = StaffBox.Text.Split(' ')[1];

            this.Dispose();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            DateLabel.Text = "";
            StaffLabel.Text = "";
            CodeLabel.Text = "";

            this.Dispose();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}