using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// 手術記録タブ・経過記録パネルの動的コントロール生成と自動計算。
    /// </summary>
    public partial class FormPat
    {
        /// <summary>
        /// 手術記録タブコントロールの初期化
        /// </summary>
        private void RecordTabControlInit()
        {
            RecordTabControl.TabPages.Clear();

            DataTable tmpTable = EyeDict.EyeSet.Tables["OpeTab"];

            foreach (DataRow r in tmpTable.Rows)
            {
                TabPage tmpPage = new TabPage(r["Name"].ToString());
                tmpPage.Name = r["Name"].ToString();

                DataRow[] tmpRows = EyeDict.EyeSet.Tables["OpeTabItem"].Select("OpeTab_id = " + r["OpeTab_id"].ToString());

                foreach (DataRow r2 in tmpRows)
                {
                    if (r2["Type"].ToString().Equals("Label"))
                    {
                        Label tmpLabel = new Label();
                        tmpLabel.Name = r2["Name"].ToString();
                        tmpLabel.Text = r2["Text"].ToString();

                        if (DynamicControl.ApplyBounds(tmpLabel, r2))
                        {
                            tmpLabel.AutoSize = true;
                        }

                        tmpLabel.TextAlign = DynamicControl.GetContentAlign(r2["Align"].ToString(), tmpLabel.TextAlign);

                        tmpPage.Controls.Add(tmpLabel);
                    }
                    else if (r2["Type"].ToString().Equals("TextBox"))
                    {
                        TextBox tmpBox = new TextBox();
                        tmpBox.Tag = r2["Code"].ToString();
                        tmpBox.Name = r2["Name"].ToString();
                        tmpBox.Text = r2["Text"].ToString();

                        DynamicControl.ApplyBounds(tmpBox, r2);

                        tmpBox.ImeMode = DynamicControl.GetImeMode(r2["Ime"].ToString(), ImeMode.NoControl);
                        tmpBox.TextAlign = DynamicControl.GetTextAlign(r2["Align"].ToString(), tmpBox.TextAlign);


                        // データの自動計算。（IOL誤差）
                        if (r2["Name"].ToString().Equals("IOL_予想屈折TextBox"))
                        {
                            tmpBox.Leave += new EventHandler(PassDataBox_Leave);
                        }

                        tmpPage.Controls.Add(tmpBox);
                    }
                    else if (r2["Type"].ToString().Equals("ComboBox"))
                    {
                        ComboBox tmpBox = new ComboBox();
                        tmpBox.Tag = r2["Code"].ToString();
                        tmpBox.Name = r2["Name"].ToString();
                        tmpBox.Text = r2["Text"].ToString();

                        DynamicControl.ApplyBounds(tmpBox, r2);

                        tmpBox.ImeMode = DynamicControl.GetImeMode(r2["Ime"].ToString(), ImeMode.NoControl);

                        foreach (string s in r2["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(s);
                        }

                        tmpPage.Controls.Add(tmpBox);
                    }
                    else if (r2["Type"].ToString().Equals("CheckBox"))
                    {
                        CheckBox tmpBox = new CheckBox();
                        tmpBox.Tag = r2["Code"].ToString();
                        tmpBox.Name = r2["Name"].ToString();
                        tmpBox.Text = r2["Text"].ToString();
                        tmpBox.Location = new Point(int.Parse(r2["X"].ToString()), int.Parse(r2["Y"].ToString()));
                        tmpBox.AutoSize = true;

                        tmpPage.Controls.Add(tmpBox);
                    }
                }

                RecordTabControl.TabPages.Add(tmpPage);
            }
        }

        /// <summary>
        /// 経過記録パネルの初期化
        /// </summary>
        private void PassPanelControlInit()
        {
            PassPanel.Controls.Clear();
            passDict.Clear();

            int i = 0;

            for (i = KensaHistoryMenuStrip.Items.Count - 1; i >= 9;  i--)
            {
                KensaHistoryMenuStrip.Items.RemoveAt(i);
            }

            DataTable timeTable = EyeDict.EyeSet.Tables["OpePassTime"];
            DataTable itemTable = EyeDict.EyeSet.Tables["OpePassItem"];

            const int TIME_X_START = 115;
            const int TIME_Y_POS = 3;
            const int TIME_X_INTERVAL = 60;

            const int TIME_WIDTH = TIME_X_INTERVAL - 2;
            const int TIME_HEIGHT = 32;

            const int ITEM_X_POS = 2;
            const int ITEM_Y_START = TIME_Y_POS + TIME_HEIGHT + 3;
            const int ITEM_Y_INTERVAL = 25;

            const int ITEM_WIDTH = TIME_X_START - ITEM_X_POS - 2;
            const int ITEM_HEIGHT = ITEM_Y_INTERVAL - 5;

            Size time_size = new Size(TIME_WIDTH, TIME_HEIGHT);
            Size item_size = new Size(ITEM_WIDTH, ITEM_HEIGHT);

            Dictionary<string, int> pos_x_dict = new Dictionary<string, int>();
            string kensa_date = "";
            string kensa_add_kind = "";
            string kensa_add_value = "";
            int j = 0;

            int x_count = 0;
            int y_count = 0;
            const int X_COUNT_INTERVAL = 6;
            const int Y_COUNT_INTERVAL = 8;

            int pos_x = 0;
            int pos_y = 0;

            // 経過日時のラベルを生成する。
            for (i = 0; i < itemTable.Rows.Count;)
            {
                pos_x = TIME_X_START;
                x_count = 0;

                foreach (DataRow r in timeTable.Rows)
                {
                    if (x_count > X_COUNT_INTERVAL)
                    {
                        pos_x += TIME_X_START;
                        x_count = 0;
                    }

                    Label tmpLabel = new Label();
                    tmpLabel.Name = r["Name"].ToString() + "_" + i;

                    kensa_date = "";
                    kensa_add_kind = "";
                    kensa_add_value = "";

                    if (r["Date"].ToString().Length > 0 && r["Date"].ToString().Split(',').Length >= 2)
                    {
                        kensa_add_kind = r["Date"].ToString().Split(',')[0];
                        kensa_add_value = r["Date"].ToString().Split(',')[1];

                        if (int.TryParse(kensa_add_value, out j))
                        {
                            if (kensa_add_kind.Equals("AddDays", StringComparison.CurrentCultureIgnoreCase))
                            {
                                kensa_date = OpeDateTimePicker.Value.AddDays(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                            }
                            else if (kensa_add_kind.Equals("AddMonths", StringComparison.CurrentCultureIgnoreCase))
                            {
                                kensa_date = OpeDateTimePicker.Value.AddMonths(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                            }
                            else if (kensa_add_kind.Equals("AddYears", StringComparison.CurrentCultureIgnoreCase))
                            {
                                kensa_date = OpeDateTimePicker.Value.AddYears(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                            }
                        }
                    }

                    tmpLabel.Text = r["Text"].ToString() + "\r\n" + kensa_date;
                    tmpLabel.AutoSize = false;
                    tmpLabel.BackColor = Color.LightCyan;
                    tmpLabel.TextAlign = ContentAlignment.MiddleCenter;
                    tmpLabel.Size = time_size;
                    tmpLabel.Location = new Point(pos_x, pos_y);

                    PassPanel.Controls.Add(tmpLabel);

                    if (i == 0)
                    {
                        KensaHistoryMenuStrip.Items.Add(new ToolStripSeparator());

                        ToolStripMenuItem tmpItem1 = new ToolStripMenuItem();
                        tmpItem1.Text = "右を " + r["Text"].ToString() + " にコピー";
                        tmpItem1.Tag = "R";
                        tmpItem1.Click += new EventHandler(KensaCopy);
                        KensaHistoryMenuStrip.Items.Add(tmpItem1);

                        ToolStripMenuItem tmpItem2 = new ToolStripMenuItem();
                        tmpItem2.Text = "左を " + r["Text"].ToString() + " にコピー";
                        tmpItem2.Tag = "L";
                        tmpItem2.Click += new EventHandler(KensaCopy);
                        KensaHistoryMenuStrip.Items.Add(tmpItem2);

                        passDict.Add(r["Text"].ToString(), r["Name"].ToString());
                        pos_x_dict.Add(r["Name"].ToString(), pos_x);
                    }

                    pos_x += TIME_X_INTERVAL;
                    x_count++;
                }

                pos_y += ITEM_Y_START;

                while (y_count <= Y_COUNT_INTERVAL)
                {
                    i++;
                    y_count++;
                    pos_y += ITEM_Y_INTERVAL;
                }

                y_count = 0;
            }

            x_count = 0;
            y_count = 0;
            pos_x = 0;
            pos_y = 0;

            // 検査項目のラベルを作成する
            for (i = 0; i < timeTable.Rows.Count;)
            {
                pos_y = ITEM_Y_START;
                y_count = 0;

                foreach (DataRow r in itemTable.Rows)
                {
                    if (y_count > Y_COUNT_INTERVAL)
                    {
                        pos_y += ITEM_Y_START;
                        y_count = 0;
                    }

                    Label itemLabel = new Label();
                    itemLabel.Name = r["Name"].ToString() + "_" + i;
                    itemLabel.Text = r["Text"].ToString();

                    if (r["Unit"].ToString().Length > 0)
                    {
                        itemLabel.Text += "(" + r["Unit"].ToString() + ")";
                    }

                    itemLabel.AutoSize = false;
                    itemLabel.BackColor = Color.LightYellow;
                    itemLabel.TextAlign = ContentAlignment.MiddleCenter;
                    itemLabel.Size = item_size;
                    itemLabel.Location = new Point(pos_x, pos_y);

                    PassPanel.Controls.Add(itemLabel);

                    pos_y += ITEM_Y_INTERVAL;
                    y_count++;
                }

                pos_x += TIME_X_START;

                while (x_count <= X_COUNT_INTERVAL)
                {
                    i++;
                    x_count++;
                    pos_x += TIME_X_INTERVAL;
                }

                x_count = 0;
            }

            x_count = 0;
            y_count = 0;
            pos_x = 0;
            pos_y = ITEM_Y_START;

            // ツールチップをいったんすべてクリアする。
            passTip.RemoveAll();

            foreach (DataRow r in itemTable.Rows)
            {
                if (y_count > Y_COUNT_INTERVAL)
                {
                    pos_y += ITEM_Y_START;
                    y_count = 0;
                }

                foreach (string s in r["Data"].ToString().Split(','))
                {
                    if (r["Type"].ToString().Equals("TextBox"))
                    {
                        TextBox tmpBox = new TextBox();
                        tmpBox.Tag = r["Code"].ToString() + "_" + s;
                        tmpBox.Name = r["Name"].ToString() + "_" + s;
                        tmpBox.Width = TIME_WIDTH;
                        tmpBox.Location = new Point(pos_x_dict[s], pos_y);
                        tmpBox.MaxLength = 12;

                        tmpBox.ImeMode = DynamicControl.GetImeMode(r["Ime"].ToString(), ImeMode.Off);

                        tmpBox.TextAlign = HorizontalAlignment.Center;

                        // データの自動計算。（等価球面度数、IOL誤差等）
                        if (r["Name"].ToString().StartsWith("KyumenDosu") || r["Name"].ToString().StartsWith("EnchuDosu"))
                        {
                            tmpBox.Leave += new EventHandler(PassDataBox_Leave);
                        }
                        
                        passTip.SetToolTip(tmpBox, r["Text"].ToString() + " " + s);

                        PassPanel.Controls.Add(tmpBox);
                    }
                    else if (r["Type"].ToString().Equals("ComboBox"))
                    {
                        ComboBox tmpBox = new ComboBox();
                        tmpBox.Tag = r["Code"].ToString() + "_" + s;
                        tmpBox.Name = r["Name"].ToString() + "_" + s;
                        tmpBox.Width = TIME_WIDTH;
                        tmpBox.Location = new Point(pos_x_dict[s], pos_y);
                        tmpBox.MaxLength = 12;

                        tmpBox.ImeMode = DynamicControl.GetImeMode(r["Ime"].ToString(), ImeMode.Off);

                        foreach (string ss in r["Item"].ToString().Split(','))
                        {
                            tmpBox.Items.Add(ss);
                        }

                        // データの自動計算。（等価球面度数、IOL誤差等）
                        if (r["Name"].ToString().StartsWith("KyumenDosu") || r["Name"].ToString().StartsWith("EnchuDosu"))
                        {
                            tmpBox.Leave += new EventHandler(PassDataBox_Leave);
                        }

                        passTip.SetToolTip(tmpBox, r["Text"].ToString() + " " + s);

                        PassPanel.Controls.Add(tmpBox);
                    }
                }

                pos_y += ITEM_Y_INTERVAL;
                y_count++;
            }
        }

        /// <summary>
        /// 経過記録パネルの日付ラベル変更
        /// </summary>
        void PassPanelControlDateChange()
        {
            DataTable tmpTable = EyeDict.EyeSet.Tables["OpePassTime"];

            string kensa_date = "";
            string kensa_add_kind = "";
            string kensa_add_value = "";
            int j = 0;

            foreach (DataRow r in tmpTable.Rows)
            {
                foreach (Control c in PassPanel.Controls)
                {
                    if (c is Label && c.Name.StartsWith(r["Name"].ToString() + "_"))
                    {
                        Label tmpLabel = (Label)c;

                        kensa_date = "";
                        kensa_add_kind = "";
                        kensa_add_value = "";

                        if (r["Date"].ToString().Length > 0 && r["Date"].ToString().Split(',').Length >= 2)
                        {
                            kensa_add_kind = r["Date"].ToString().Split(',')[0];
                            kensa_add_value = r["Date"].ToString().Split(',')[1];

                            if (int.TryParse(kensa_add_value, out j))
                            {
                                if (kensa_add_kind.Equals("AddDays", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    kensa_date = OpeDateTimePicker.Value.AddDays(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                                }
                                else if (kensa_add_kind.Equals("AddMonths", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    kensa_date = OpeDateTimePicker.Value.AddMonths(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                                }
                                else if (kensa_add_kind.Equals("AddYears", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    kensa_date = OpeDateTimePicker.Value.AddYears(int.Parse(kensa_add_value)).ToString("yy/MM/dd");
                                }
                            }
                        }

                        tmpLabel.Text = r["Text"].ToString() + "\r\n" + kensa_date;

                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// データの自動計算。（等価球面度数、IOL誤差等）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PassDataBox_Leave(object sender, EventArgs e)
        {
            this.CalcPassData();
        }

        /// <summary>
        /// データの自動計算。（等価球面度数、IOL誤差等）
        /// </summary>
        private void CalcPassData()
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables["OpePassTime"].Rows)
            {
                // 等価球面度数の計算
                if (PassPanel.Controls.ContainsKey("ToukaKyumenDosu_" + r["Name"].ToString()) && PassPanel.Controls.ContainsKey("KyumenDosu_" + r["Name"].ToString()) && PassPanel.Controls.ContainsKey("EnchuDosu_" + r["Name"].ToString()))
                {
                    Control c1 = PassPanel.Controls["KyumenDosu_" + r["Name"].ToString()];
                    Control c2 = PassPanel.Controls["EnchuDosu_" + r["Name"].ToString()];

                    double d1 = 0;
                    double d2 = 0;

                    if (c1.Text.Length > 0 && double.TryParse(c1.Text, out d1) && c2.Text.Length > 0 && double.TryParse(c2.Text, out d2))
                    {
                        PassPanel.Controls["ToukaKyumenDosu_" + r["Name"].ToString()].Text = EyeDict.CalcSE(d1, d2).ToString();

                        // IOL誤差の計算
                        if (PassPanel.Controls.ContainsKey("IOLGosa_" + r["Name"].ToString()) && RecordTabControl.TabPages.ContainsKey("白内障") && RecordTabControl.TabPages["白内障"].Controls.ContainsKey("IOL_予想屈折TextBox"))
                        {
                            Control c3 = RecordTabControl.TabPages["白内障"].Controls["IOL_予想屈折TextBox"];
                            double d3 = 0;

                            if (c3.Text.Length > 0 && double.TryParse(c3.Text, out d3))
                            {
                                PassPanel.Controls["IOLGosa_" + r["Name"].ToString()].Text = EyeDict.CalcIOLGosa(EyeDict.CalcSE(d1, d2), d3).ToString();
                            }
                        }
                    }
                }
            }
        }

        void KensaCopy(object sender, EventArgs e)
        {
            if (KensaHistoryView.SelectedRows.Count > 0)
            {
                ToolStripMenuItem tmpItem = (ToolStripMenuItem)(sender);
                string side = tmpItem.Tag.ToString();
                string time = passDict[tmpItem.Text.Split(' ')[1]];

                for (int i = 13; i < KensaHistoryView.Columns.Count; i++)
                {
                    DataGridViewCell cell = KensaHistoryView.SelectedRows[0].Cells[i];
                    string col_name = KensaHistoryView.Columns[cell.ColumnIndex].Name;

                    if (col_name.Contains("_" + side))
                    {
                        if (PassPanel.Controls.ContainsKey(col_name.Split('_')[0] + "_" + time))
                        {
                            PassPanel.Controls[col_name.Split('_')[0] + "_" + time].Text = cell.Value.ToString();
                        }
                    }
                }
            }
        }
    }
}
