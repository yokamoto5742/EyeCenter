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
    /// 手術記録の Excel 帳票（同意書・申し送り書・オペ録）出力。
    /// </summary>
    public partial class FormPat
    {
        /// <summary>
        /// 眼科同意書を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgreePrintButton_Click(object sender, EventArgs e)
        {
            this.ExcelOpen(EyeDict.EyeSet.Tables["OpeExcel"].Rows[0]["Agree"].ToString());
        }

        /// <summary>
        /// 眼科申し送り書を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NsPrintButton_Click(object sender, EventArgs e)
        {
            this.ExcelOpen(EyeDict.EyeSet.Tables["OpeExcel"].Rows[0]["Ns"].ToString());
        }

        /// <summary>
        /// オペ録を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecordPrintButton_Click(object sender, EventArgs e)
        {
            this.ExcelOpen(EyeDict.EyeSet.Tables["OpeExcel"].Rows[0]["Record"].ToString());
        }

        /// <summary>
        /// EyeDoc.Item を 1 件作成してリストに追加する。
        /// </summary>
        static void AddItem(List<EyeDoc.Item> list, string kind, string name, string value)
        {
            EyeDoc.Item item = new EyeDoc.Item();
            item.Kind = kind;
            item.Name = name;
            item.Value = value;
            list.Add(item);
        }

        /// <summary>
        /// パネル内の TextBox / ComboBox / CheckBox を EyeDoc.Item にしてリストに追加する。
        /// CheckBox は "1"/"0" に変換する。name_from_tag が true なら項目名に Tag を使う（サマリ系パネル）。
        /// </summary>
        static void AddPanelItems(Control.ControlCollection controls, string kind, bool name_from_tag, List<EyeDoc.Item> list)
        {
            foreach (Control c in controls)
            {
                if (c is TextBox || c is ComboBox)
                {
                    AddItem(list, kind, name_from_tag ? c.Tag.ToString() : c.Name, c.Text);
                }
                else if (c is CheckBox)
                {
                    AddItem(list, kind, name_from_tag ? c.Tag.ToString() : c.Name, ((CheckBox)c).Checked ? "1" : "0");
                }
            }
        }

        private void ExcelOpen(string file_name)
        {
            // exe と同じフォルダに同名のテンプレートがあれば優先する（拡張子違いも許容）
            string baseName = Path.GetFileNameWithoutExtension(file_name);
            if (baseName.Length > 0)
            {
                string[] localFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, baseName + ".xls*");
                if (localFiles.Length > 0)
                {
                    file_name = localFiles[0];
                }
            }

            EyeDoc tmpDoc = new EyeDoc(this.Pat.Id);

            tmpDoc.FileName = file_name;

            string eye = "";

            if (EyeBoxR.Checked && EyeBoxL.Checked)
            {
                eye = "両";
            }
            else if (EyeBoxR.Checked)
            {
                eye = "右";
            }
            else if (EyeBoxL.Checked)
            {
                eye = "左";
            }

            AddItem(tmpDoc.ItemList, "手術基本情報", "眼球", eye);
            AddItem(tmpDoc.ItemList, "手術基本情報", "手術日", this.OpeDateTimePicker.Value.ToString("yyyy/MM/dd"));
            AddItem(tmpDoc.ItemList, "手術基本情報", "種別", this.OpeKindBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "時刻", this.OpeTimeBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "手術室", this.OpeRoomBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "術者", this.DoctorBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "予定時間", this.PlanTimeBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "麻酔", this.AnesBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "病名", this.DiagBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "術式", this.OpeNameBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "入外", this.InOutBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "病室", this.InRoomBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "入院日", this.InDateTimePicker.Enabled ? this.InDateTimePicker.Value.ToString("yyyy/MM/dd") : "");
            AddItem(tmpDoc.ItemList, "手術基本情報", "入院時刻", this.InTimeBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "入院期間", this.InTermBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "身長", this.HeightBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "体重", this.WeightBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "体表", this.SurfaceBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "ビスダイン", this.VisdineBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "ブドウ糖", this.GrapeBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "DM単位", this.DmBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "感染症", this.InfectionBox.Text);
            AddItem(tmpDoc.ItemList, "手術基本情報", "術前チェック完了", this.PreCheckBox.Checked ? "1" : "0");
            AddItem(tmpDoc.ItemList, "手術基本情報", "備考", this.CommentBox.Text);

            AddPanelItems(this.RecordTabControl.TabPages[0].Controls, "術前検査", false, tmpDoc.ItemList);
            AddPanelItems(this.RecordTabControl.TabPages[1].Controls, "術前アナムネ", false, tmpDoc.ItemList);
            AddPanelItems(this.RecordTabControl.TabPages[2].Controls, "共通記録", false, tmpDoc.ItemList);

            AddItem(tmpDoc.SumList, "サマリメイン", "主病名", SumDiagBox.Text);
            AddItem(tmpDoc.SumList, "サマリメイン", "分類1", SumKindBox1.Text);
            AddItem(tmpDoc.SumList, "サマリメイン", "分類2", SumKindBox2.Text);
            AddItem(tmpDoc.SumList, "サマリメイン", "分類3", SumKindBox3.Text);

            AddPanelItems(this.SumPanel1.Controls, "サマリメイン", true, tmpDoc.SumList);
            AddPanelItems(this.SumPanel3.Controls, "サマリ入院管理", true, tmpDoc.SumList);
            AddPanelItems(this.SumPanel4.Controls, "サマリ経過", true, tmpDoc.SumList);

            AddItem(tmpDoc.SumList, "サマリ経過", "経過", SumPassBox.Text);
            AddItem(tmpDoc.SumList, "サマリ経過", "履歴", SumHistBox.Text);
            AddItem(tmpDoc.SumList, "サマリ治療方針", "方針", SumPlanBox.Text);

            foreach (Control c in this.SumPanel2.Controls)
            {
                if (c is Label && (c.Name.EndsWith("_D") || c.Name.EndsWith("_S")))
                {
                    AddItem(tmpDoc.SumList, "サマリ治療方針", c.Name, c.Text);
                }
            }

            // オペ録・眼科申し送り書はアプリ側でバーコードを生成・挿入して別名保存する
            ExcelControl excelControl = new ExcelControl();

            try
            {
                excelControl.MakeDocument(tmpDoc);
            }
            catch (Exception ex)
            {
                LibUtility.Except(ex);
            }
            finally
            {
                excelControl.ReleaseExcel();
            }
        }
    }
}
