using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Entity;

namespace EyeCenter
{
    class ControlIVPage
    {
        FormPat FP;
        DataSet DSet;

        string IVTemplate = "";

        /// <summary>
        /// 問診を初期化する。
        /// </summary>
        public void Init(FormPat fp)
        {
            FP = fp;
            DSet = new DataSet();
            IVTemplate = EyeDict.EyeSet.Tables["IVTemplate"].Rows[0]["Text"].ToString();

            DataTable tmpTable = DSet.Tables.Add("IV");
            tmpTable.Columns.Add("ID");
            tmpTable.Columns.Add("IV_DATE");
            tmpTable.Columns.Add("日付");
            tmpTable.Columns.Add("時刻");
            tmpTable.Columns.Add("内容");
            tmpTable.Columns.Add("STAFF");
            tmpTable.Columns.Add("入力者");
            tmpTable.Columns.Add("SAVE_DATE");
            tmpTable.Columns.Add("SAVE_TIME");
            tmpTable.Columns.Add("STATUS");
            tmpTable.Columns.Add("PDF_SAVE");
        }

        /// <summary>
        /// 問診をクリアする。
        /// </summary>
        public void Clear()
        {
            FP.IVIdBox.Clear();
            FP.IVDate.Value = DateTime.Now;
            FP.IVContBox.Text = IVTemplate;
            FP.IVStaffLabel.Text = "";
        }

        /// <summary>
        /// 問診をクリアする。
        /// </summary>
        public void AllClear()
        {
            Clear();

            DataTable tmpTable = DSet.Tables["IV"];
            tmpTable.Clear();

            FP.IVHistoryView.DataSource = new DataView(tmpTable);
        }

        /// <summary>
        /// 問診歴を表示する。
        /// </summary>
        /// <param name="pt_id"></param>
        public void HistoryShow(string pt_id)
        {
            AllClear();

            DataTable tmpTable = DSet.Tables["IV"];

            List<EyeIV> ivList = EyeIV.Load(pt_id);

            foreach (EyeIV tmpIV in ivList)
            {
                DataRow r = tmpTable.NewRow();

                r["ID"] = tmpIV.Id;
                r["IV_DATE"] = tmpIV.IVDate;
                r["日付"] = tmpIV.IVDate.Substring(2, 6).Insert(2, "/").Insert(5, "/");
                r["時刻"] = tmpIV.SaveTime.PadLeft(6, '0').Substring(0, 4).Insert(2, ":");
                r["内容"] = tmpIV.Cont;
                r["STAFF"] = tmpIV.Staff;

                if (Dict.StaffDict.ContainsKey(tmpIV.Staff))
                {
                    r["入力者"] = Dict.StaffDict[tmpIV.Staff].Name;
                }

                r["SAVE_DATE"] = tmpIV.SaveDate;
                r["SAVE_TIME"] = tmpIV.SaveTime;
                r["STATUS"] = tmpIV.Status;
                r["PDF_SAVE"] = tmpIV.PDFSave;

                tmpTable.Rows.Add(r);
            }

            HistoryFormat();
        }

        /// <summary>
        /// 問診歴をフォーマットする。
        /// </summary>
        public void HistoryFormat()
        {
            if (DSet != null && DSet.Tables.Contains("IV"))
            {
                DataView tmpView = new DataView(DSet.Tables["IV"]);
                tmpView.RowFilter = "STATUS = '1'";

                FP.IVHistoryView.DataSource = tmpView;

                FP.IVHistoryView.Columns["ID"].Visible = false;
                FP.IVHistoryView.Columns["IV_DATE"].Visible = false;
                FP.IVHistoryView.Columns["日付"].Width = 70;
                FP.IVHistoryView.Columns["日付"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                FP.IVHistoryView.Columns["時刻"].Width = 45;
                FP.IVHistoryView.Columns["時刻"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                FP.IVHistoryView.Columns["内容"].Width = FP.IVHistoryView.Width - 230;
                FP.IVHistoryView.Columns["STAFF"].Visible = false;
                FP.IVHistoryView.Columns["入力者"].Width = 70;
                FP.IVHistoryView.Columns["入力者"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                FP.IVHistoryView.Columns["SAVE_DATE"].Visible = false;
                FP.IVHistoryView.Columns["SAVE_TIME"].Visible = false;
                FP.IVHistoryView.Columns["STATUS"].Visible = false;
                FP.IVHistoryView.Columns["PDF_SAVE"].Visible = false;
            }
        }

        /// <summary>
        /// 問診を登録する。
        /// </summary>
        public bool Save(string pt_id)
        {
            int p = 0;

            if (pt_id.Length == 0 || !int.TryParse(pt_id, out p) || FP.IVContBox.Text.Length == 0)
            {
                return false;
            }

            EyeIV iv = new EyeIV();

            iv.Id = FP.IVIdBox.Text;
            iv.PtId = pt_id;
            iv.IVDate = FP.IVDate.Value.ToString("yyyyMMdd");
            iv.Cont = FP.IVContBox.Text;
            iv.Staff = LoginUser.Id;
            iv.Status = "1";

            iv.Save();

            return true;
        }

        /// <summary>
        /// 問診を削除する。
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(string id)
        {
            int i = 0;

            if (id.Length == 0 || !int.TryParse(id, out i))
            {
                return false;
            }

            EyeIV.Delete(id);

            return true;
        }
    }
}
