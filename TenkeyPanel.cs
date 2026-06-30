using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MedicalLibrary.Agent;

namespace EyeCenter
{
    class TenkeyPanel : Panel
    {
        public TenkeyPanel(string tenkey_id)
        {
            this.BackColor = Color.Wheat;

            DataRow tmpRow = EyeDict.EyeSet.Tables["Tenkey"].Select("ID = " + tenkey_id)[0];
            this.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));
            this.Size = new Size(int.Parse(tmpRow["Width"].ToString()), int.Parse(tmpRow["Height"].ToString()));

            DataRow[] tmpRows = EyeDict.EyeSet.Tables["TenkeyButton"].Select("Tenkey_ID = " + tmpRow["Tenkey_ID"].ToString());

            foreach (DataRow r in tmpRows)
            {
                Button tmpButton = new Button();
                tmpButton.Name = r["Name"].ToString();
                tmpButton.Text = r["Text"].ToString();
                tmpButton.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));
                tmpButton.Size = new Size(int.Parse(r["Width"].ToString()), int.Parse(r["Height"].ToString()));
                tmpButton.BackColor = Color.FromName(r["Color"].ToString());

                if (tmpButton.Name.Equals("MovePrev"))
                {
                    tmpButton.Click += new EventHandler(MovePrev);
                }
                else if (tmpButton.Name.Equals("MoveNext"))
                {
                    tmpButton.Click += new EventHandler(MoveNext);
                }
                else if (tmpButton.Name.Equals("InputClear"))
                {
                    tmpButton.Click += new EventHandler(InputClear);
                }
                else
                {
                    tmpButton.Click += new EventHandler(InputWord);
                }

                this.Controls.Add(tmpButton);
            }
        }

        private void InputWord(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);
            ((KensaPanelDetail)(this.Parent.Controls["KensaPanel"])).InputWord(tmpButton.Text);
        }

        private void InputClear(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);
            ((KensaPanelDetail)(this.Parent.Controls["KensaPanel"])).InputClear();
        }

        private void MovePrev(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);
            ((KensaPanelDetail)(this.Parent.Controls["KensaPanel"])).MovePrev();
        }

        private void MoveNext(object sender, EventArgs e)
        {
            Button tmpButton = (Button)(sender);
            ((KensaPanelDetail)(this.Parent.Controls["KensaPanel"])).MoveNext();
        }
    }
}
