using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// 手術検索のCSV出力に結合するサマリー項目を選択するダイアログ。
    /// </summary>
    public class FormSumColumnSelect : Form
    {
        /// <summary>
        /// 結合対象のサマリー項目
        /// </summary>
        class SumColumn
        {
            public string Key;      // 選択保存用のキー
            public string Name;     // CSVの列名
            public string Group;    // 表示グループ名
            public int Cont;        // 0:固定項目, 1〜4:CONT1〜4
            public string Code;     // SumItemのコード

            public override string ToString()
            {
                return "【" + Group + "】" + Name;
            }
        }

        CheckedListBox ItemListBox = new CheckedListBox();

        List<SumColumn> SelectedColumns = new List<SumColumn>();

        FormSumColumnSelect(List<SumColumn> columns, List<string> checked_keys)
        {
            this.Text = "サマリー結合列の選択";
            this.ClientSize = new Size(380, 520);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;

            ItemListBox.Location = new Point(10, 10);
            ItemListBox.Size = new Size(360, 465);
            ItemListBox.CheckOnClick = true;
            ItemListBox.IntegralHeight = false;
            this.Controls.Add(ItemListBox);

            foreach (SumColumn col in columns)
            {
                ItemListBox.Items.Add(col, checked_keys.Contains(col.Key));
            }

            Button allButton = new Button();
            allButton.Text = "全選択";
            allButton.Location = new Point(10, 485);
            allButton.Size = new Size(70, 25);
            allButton.Click += new EventHandler(AllButton_Click);
            this.Controls.Add(allButton);

            Button clearButton = new Button();
            clearButton.Text = "全解除";
            clearButton.Location = new Point(85, 485);
            clearButton.Size = new Size(70, 25);
            clearButton.Click += new EventHandler(ClearButton_Click);
            this.Controls.Add(clearButton);

            Button okButton = new Button();
            okButton.Text = "OK";
            okButton.Location = new Point(215, 485);
            okButton.Size = new Size(75, 25);
            okButton.Click += new EventHandler(OkButton_Click);
            this.Controls.Add(okButton);

            Button cancelButton = new Button();
            cancelButton.Text = "キャンセル";
            cancelButton.Location = new Point(295, 485);
            cancelButton.Size = new Size(75, 25);
            cancelButton.DialogResult = DialogResult.Cancel;
            this.Controls.Add(cancelButton);

            this.AcceptButton = okButton;
            this.CancelButton = cancelButton;
        }

        void AllButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ItemListBox.Items.Count; i++)
            {
                ItemListBox.SetItemChecked(i, true);
            }
        }

        void ClearButton_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < ItemListBox.Items.Count; i++)
            {
                ItemListBox.SetItemChecked(i, false);
            }
        }

        void OkButton_Click(object sender, EventArgs e)
        {
            SelectedColumns.Clear();

            foreach (object o in ItemListBox.CheckedItems)
            {
                SelectedColumns.Add((SumColumn)o);
            }

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// サマリー項目の選択ダイアログを表示し、選択された項目を data の各行の右側に結合する。
        /// キャンセル時は false を返す。
        /// </summary>
        /// <param name="data">結合先の出力データ（RecordList は pt_id_list と同順であること）</param>
        /// <param name="pt_id_list">各行の患者IDのリスト</param>
        /// <returns></returns>
        public static bool AppendSummaryColumns(TableData data, List<string> pt_id_list)
        {
            FormSumColumnSelect form = new FormSumColumnSelect(GetAllColumns(), LoadSelection());

            if (form.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            List<SumColumn> selected = form.SelectedColumns;

            SaveSelection(selected);

            if (selected.Count == 0)
            {
                return true;
            }

            foreach (SumColumn col in selected)
            {
                if (col.Cont == 2)
                {
                    data.Title.Add(col.Name + "_日付");
                    data.Title.Add(col.Name);
                }
                else
                {
                    data.Title.Add(col.Name);
                }
            }

            List<string> pt_list = new List<string>();

            foreach (string id in pt_id_list)
            {
                if (!pt_list.Contains(id))
                {
                    pt_list.Add(id);
                }
            }

            Dictionary<string, EyeSummary> sumDict = new Dictionary<string, EyeSummary>();

            foreach (EyeSummary sum in EyeSummary.GetListByPats(pt_list))
            {
                sumDict[sum.PtId] = sum;
            }

            for (int i = 0; i < data.RecordList.Count && i < pt_id_list.Count; i++)
            {
                EyeSummary sum = null;

                if (sumDict.ContainsKey(pt_id_list[i]))
                {
                    sum = sumDict[pt_id_list[i]];
                }

                Dictionary<string, string>[] contDicts = new Dictionary<string, string>[5];

                for (int k = 1; k <= 4; k++)
                {
                    contDicts[k] = new Dictionary<string, string>();
                }

                if (sum != null)
                {
                    contDicts[1] = ParseCont(sum.Cont1);
                    contDicts[2] = ParseCont(sum.Cont2);
                    contDicts[3] = ParseCont(sum.Cont3);
                    contDicts[4] = ParseCont(sum.Cont4);
                }

                foreach (SumColumn col in selected)
                {
                    if (col.Cont == 0)
                    {
                        data.RecordList[i].DataList.Add(sum != null ? FixedValue(sum, col.Code) : "");
                    }
                    else if (col.Cont == 2)
                    {
                        // 「日付 値」形式を2列に分けて出力する
                        if (contDicts[2].ContainsKey(col.Code) && contDicts[2][col.Code].Contains(" "))
                        {
                            data.RecordList[i].DataList.Add(contDicts[2][col.Code].Split(' ')[0]);
                            data.RecordList[i].DataList.Add(contDicts[2][col.Code].Split(' ')[1]);
                        }
                        else
                        {
                            data.RecordList[i].DataList.Add("");
                            data.RecordList[i].DataList.Add("");
                        }
                    }
                    else
                    {
                        if (contDicts[col.Cont].ContainsKey(col.Code))
                        {
                            data.RecordList[i].DataList.Add(contDicts[col.Cont][col.Code]);
                        }
                        else
                        {
                            data.RecordList[i].DataList.Add("");
                        }
                    }
                }
            }

            return true;
        }

        static List<SumColumn> GetAllColumns()
        {
            List<SumColumn> list = new List<SumColumn>();

            string[,] fixedItems = {
                { "DIAG", "主病名" },
                { "KIND1", "分類1" },
                { "KIND2", "分類2" },
                { "KIND3", "分類3" },
                { "PLAN", "方針" },
                { "PASS", "経過" },
                { "HIST", "既往" }
            };

            for (int i = 0; i < fixedItems.GetLength(0); i++)
            {
                SumColumn col = new SumColumn();
                col.Key = fixedItems[i, 0];
                col.Name = fixedItems[i, 1];
                col.Group = "基本";
                col.Cont = 0;
                col.Code = fixedItems[i, 0];
                list.Add(col);
            }

            AddItems(list, "SumItem1", "Label", "基本項目", 1);
            AddItems(list, "SumItem2", "Label", "登録・測定", 2);
            AddItems(list, "SumItem3", "Label", "入院", 3);
            AddItems(list, "SumItem4", "Name", "検査結果", 4);

            return list;
        }

        static void AddItems(List<SumColumn> list, string table, string name_column, string group, int cont)
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables[table].Rows)
            {
                if (r["Code"].ToString().Length > 0 && r[name_column].ToString().Length > 0)
                {
                    SumColumn col = new SumColumn();
                    col.Key = cont + "_" + r["Code"].ToString();
                    col.Name = r[name_column].ToString();
                    col.Group = group;
                    col.Cont = cont;
                    col.Code = r["Code"].ToString();
                    list.Add(col);
                }
            }
        }

        static string FixedValue(EyeSummary sum, string code)
        {
            switch (code)
            {
                case "DIAG": return sum.Diag;
                case "KIND1": return sum.Kind1;
                case "KIND2": return sum.Kind2;
                case "KIND3": return sum.Kind3;
                case "PLAN": return sum.Plan;
                case "PASS": return sum.Pass;
                case "HIST": return sum.Hist;
            }

            return "";
        }

        static Dictionary<string, string> ParseCont(string cont)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            foreach (string s in cont.Split('\r', '\n'))
            {
                if (s.Split(',').Length > 1 && !dict.ContainsKey(s.Split(',')[0]))
                {
                    dict.Add(s.Split(',')[0], s.Substring(s.IndexOf(',') + 1).Replace("<CR+LF>", "\r\n"));
                }
            }

            return dict;
        }

        static string SelectionFile()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"EyeCenter\SumJoinColumns.txt");
        }

        static List<string> LoadSelection()
        {
            List<string> list = new List<string>();

            try
            {
                if (File.Exists(SelectionFile()))
                {
                    foreach (string s in File.ReadAllLines(SelectionFile()))
                    {
                        if (s.Length > 0)
                        {
                            list.Add(s);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return list;
        }

        static void SaveSelection(List<SumColumn> selected)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SelectionFile()));

                List<string> keys = new List<string>();

                foreach (SumColumn col in selected)
                {
                    keys.Add(col.Key);
                }

                File.WriteAllLines(SelectionFile(), keys.ToArray());
            }
            catch (Exception)
            {
            }
        }
    }
}
