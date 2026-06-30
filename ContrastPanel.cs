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
    class ContrastPanel : Panel
    {
        /// <summary>
        /// グループ名（右A, 左B など）ごとのラベルのリストの辞書。
        /// グループ名をキーとして、それに所属するラベルのリストが取得できる。
        /// </summary>
        Dictionary<string, List<Label>> LabelBroupDictR = new Dictionary<string, List<Label>>();
        Dictionary<string, List<Label>> LabelBroupDictL = new Dictionary<string, List<Label>>();

        public ContrastPanel()
        {
            DataRow tmpRow = EyeDict.EyeSet.Tables["ContrastPanel"].Rows[0];
            this.Location = new Point(int.Parse(tmpRow["X"].ToString()), int.Parse(tmpRow["Y"].ToString()));
            this.Size = new Size(int.Parse(tmpRow["Width"].ToString()), int.Parse(tmpRow["Height"].ToString()));
            this.BackColor = Color.LightGoldenrodYellow;
            this.Name = "ContrastPanel";

            DataTable tmpTable = EyeDict.EyeSet.Tables["ContrastBox"];

            foreach (DataRow r in tmpTable.Rows)
            {
                Label tmpLabel = new Label();
                tmpLabel.Name = r["Name"].ToString();
                tmpLabel.Text = r["Text"].ToString();
                tmpLabel.Size = new Size(14, 14);
                tmpLabel.BackColor = Color.LightYellow;
                tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                tmpLabel.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));
                tmpLabel.Tag = r["KensaItemName"].ToString() + "," + r["Value"].ToString();
                tmpLabel.Click += new EventHandler(Label_Click);

                // ラベルをグループ別リストに登録する。
                if (r["Side"].ToString().Equals("R"))
                {
                    if (LabelBroupDictR.ContainsKey(r["KensaItemName"].ToString()))
                    {
                        LabelBroupDictR[r["KensaItemName"].ToString()].Add(tmpLabel);
                    }
                    else
                    {
                        List<Label> tmpList = new List<Label>();
                        tmpList.Add(tmpLabel);
                        LabelBroupDictR.Add(r["KensaItemName"].ToString(), tmpList);
                    }
                }
                else if (r["Side"].ToString().Equals("L"))
                {
                    if (LabelBroupDictL.ContainsKey(r["KensaItemName"].ToString()))
                    {
                        LabelBroupDictL[r["KensaItemName"].ToString()].Add(tmpLabel);
                    }
                    else
                    {
                        List<Label> tmpList = new List<Label>();
                        tmpList.Add(tmpLabel);
                        LabelBroupDictL.Add(r["KensaItemName"].ToString(), tmpList);
                    }
                }

                this.Controls.Add(tmpLabel);
            }

            PictureBox picBox = new PictureBox();
            picBox.Name = "PicBox";

            if (AppFile.FilePath(@"EyeCenter\Contrast.jpg").Length > 0)
            {
                picBox.BackgroundImage = Image.FromFile(AppFile.FilePath(@"EyeCenter\Contrast.jpg"));
            }
            else if (AppFile.FilePath(@"EyeImage\Contrast.jpg").Length > 0)
            {
                picBox.BackgroundImage = Image.FromFile(AppFile.FilePath(@"EyeImage\Contrast.jpg"));
            }

            picBox.BackgroundImageLayout = ImageLayout.Zoom;
            picBox.Location = new Point(1, 1);
            picBox.Size = new Size(this.Size.Width - 2, this.Size.Height - 2);
            picBox.Image = new Bitmap(picBox.Size.Width, picBox.Size.Height);

            this.Controls.Add(picBox);
        }

        void Label_Click(object sender, EventArgs e)
        {
            Label tmpLabel = (Label)(sender);
            string group = tmpLabel.Tag.ToString().Split(',')[0];
            string value = tmpLabel.Tag.ToString().Split(',')[1];

            if (this.Parent.Controls.ContainsKey(group))
            {
                this.Parent.Controls[group].Text = value;
            }

            if (this.LabelBroupDictR.ContainsKey(group))
            {
                foreach (Label r in LabelBroupDictR[group])
                {
                    if (r.Name.Equals(tmpLabel.Name))
                    {
                        r.BackColor = Color.Red;
                    }
                    else
                    {
                        r.BackColor = Color.LightYellow;
                    }
                }
            }
            else if (this.LabelBroupDictL.ContainsKey(group))
            {
                foreach (Label r in LabelBroupDictL[group])
                {
                    if (r.Name.Equals(tmpLabel.Name))
                    {
                        r.BackColor = Color.Red;
                    }
                    else
                    {
                        r.BackColor = Color.LightYellow;
                    }
                }
            }

            this.DrawLine();

            // 値が変更されたら親パネルの Edited = true をセットする
            Control c = this.Parent;

            while (c != null)
            {
                if (c is KensaPanelDetail)
                {
                    ((KensaPanelDetail)c).Edited = true;
                    break;
                }

                c = c.Parent;
            }
        }

        void DrawLine()
        {
            List<Point> pointListR = new List<Point>();
            List<Point> pointListL = new List<Point>();

            foreach (string k in LabelBroupDictR.Keys)
            {
                foreach (Label c in LabelBroupDictR[k])
                {
                    if (c.BackColor == Color.Red)
                    {
                        pointListR.Add(new Point(c.Location.X + 7, c.Location.Y + 7));
                        break;
                    }
                }
            }

            foreach (string k in LabelBroupDictL.Keys)
            {
                foreach (Label c in LabelBroupDictL[k])
                {
                    if (c.BackColor == Color.Red)
                    {
                        pointListL.Add(new Point(c.Location.X + 7, c.Location.Y + 7));
                        break;
                    }
                }
            }

            PictureBox tmpBox = (PictureBox)(this.Controls["PicBox"]);
            tmpBox.Image = new Bitmap(tmpBox.Size.Width, tmpBox.Size.Height);

            Graphics g = Graphics.FromImage(tmpBox.Image);
            Pen p = new Pen(Color.Red, 2);

            for (int i = 0; i < pointListR.Count - 1; i++)
            {
                g.DrawLine(p, pointListR[i], pointListR[i + 1]);
            }

            for (int i = 0; i < pointListL.Count - 1; i++)
            {
                g.DrawLine(p, pointListL[i], pointListL[i + 1]);
            }
        }

        public void KensaShow()
        {
            foreach (Control c in this.Parent.Controls)
            {
                if (c.GetType().Name.Equals("TextBox") && c.Text.Length > 0)
                {
                    if (LabelBroupDictR.ContainsKey(c.Name))
                    {
                        foreach (Label tmpLabel in LabelBroupDictR[c.Name])
                        {
                            if (tmpLabel.Tag.ToString().Split(',')[1].Equals(c.Text))
                            {
                                tmpLabel.BackColor = Color.Red;
                            }
                            else
                            {
                                tmpLabel.BackColor = Color.LightYellow;
                            }
                        }
                    }
                    else if (LabelBroupDictL.ContainsKey(c.Name))
                    {
                        foreach (Label tmpLabel in LabelBroupDictL[c.Name])
                        {
                            if (tmpLabel.Tag.ToString().Split(',')[1].Equals(c.Text))
                            {
                                tmpLabel.BackColor = Color.Red;
                            }
                            else
                            {
                                tmpLabel.BackColor = Color.LightYellow;
                            }
                        }
                    }
                }
            }

            this.DrawLine();
        }

        public void KensaClear()
        {
            foreach (Control c in this.Controls)
            {
                if (c.GetType().Name.Equals("Label"))
                {
                    c.BackColor = Color.LightYellow;
                }
            }

            PictureBox tmpBox = (PictureBox)(this.Controls["PicBox"]);
            tmpBox.Image = new Bitmap(tmpBox.Size.Width, tmpBox.Size.Height);
        }
    }
}
