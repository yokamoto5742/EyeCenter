using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// 検索結果のCSV出力に含める列を選択するダイアログ。
    /// </summary>
    public class FormCsvColumnSelect : Form
    {
        CheckedListBox ItemListBox = new CheckedListBox();

        List<string> SelectedNames = new List<string>();

        FormCsvColumnSelect(List<string> names, List<string> excluded_names)
        {
            this.Text = "CSV出力列の選択";
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

            foreach (string name in names)
            {
                ItemListBox.Items.Add(name, !excluded_names.Contains(name));
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
            if (ItemListBox.CheckedItems.Count == 0)
            {
                MessageBox.Show("出力する列を選択してください");
                return;
            }

            SelectedNames.Clear();

            foreach (object o in ItemListBox.CheckedItems)
            {
                SelectedNames.Add((string)o);
            }

            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// 列の選択ダイアログを表示し、選択されなかった列を data から取り除く。
        /// 選択状態は key ごとに保存され、初回や新規の列はすべて選択済みとして扱う。
        /// キャンセル時は false を返す。
        /// </summary>
        /// <param name="data">出力データ</param>
        /// <param name="key">選択保存用のキー（検索フォームごとに一意）</param>
        /// <returns></returns>
        public static bool FilterColumns(TableData data, string key)
        {
            // 同名の列（日付・値のペアなど）は1項目にまとめて表示する
            List<string> names = new List<string>();

            foreach (string name in ColumnNames(data))
            {
                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }

            FormCsvColumnSelect form = new FormCsvColumnSelect(names, LoadExclusion(key));

            if (form.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            List<string> excluded = new List<string>();

            foreach (string name in names)
            {
                if (!form.SelectedNames.Contains(name))
                {
                    excluded.Add(name);
                }
            }

            SaveExclusion(key, excluded);

            if (excluded.Count == 0)
            {
                return true;
            }

            List<string> all_names = ColumnNames(data);

            for (int i = all_names.Count - 1; i >= 0; i--)
            {
                if (excluded.Contains(all_names[i]))
                {
                    data.Title.RemoveAt(i);

                    if (i < data.Title2.Count)
                    {
                        data.Title2.RemoveAt(i);
                    }

                    foreach (TableDataRecord record in data.RecordList)
                    {
                        if (i < record.DataList.Count)
                        {
                            record.DataList.RemoveAt(i);
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 列の表示名のリストを作成する。Title2 がある列は「Title Title2」の形式にする。
        /// </summary>
        static List<string> ColumnNames(TableData data)
        {
            List<string> names = new List<string>();

            for (int i = 0; i < data.Title.Count; i++)
            {
                if (i < data.Title2.Count && data.Title2[i].Length > 0)
                {
                    names.Add(data.Title[i] + " " + data.Title2[i]);
                }
                else
                {
                    names.Add(data.Title[i]);
                }
            }

            return names;
        }

        static string ExclusionFile(string key)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"EyeCenter\CsvColumns_" + key + ".txt");
        }

        static List<string> LoadExclusion(string key)
        {
            List<string> list = new List<string>();

            try
            {
                if (File.Exists(ExclusionFile(key)))
                {
                    foreach (string s in File.ReadAllLines(ExclusionFile(key)))
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

        static void SaveExclusion(string key, List<string> excluded)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(ExclusionFile(key)));

                File.WriteAllLines(ExclusionFile(key), excluded.ToArray());
            }
            catch (Exception)
            {
            }
        }
    }
}
