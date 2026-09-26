using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;
using MedicalLibrary.Entity;

namespace EyeCenter
{
    class FormControl
    {
        static FormOpeRsv F_OpeRsv;
        static FormOpeRsvList F_OpeRsvList;
        static FormFindOpeRecord F_FindOpeRecord;
        static FormFindKensa F_FindKensa;
        static FormFindSummary F_FindSummary;
        static FormPrint F_Print;
        static FormInput F_Input;
        static NidekARK1ListForm F_NidekARK1;
        static CanonRKF1Form F_CanonRKF1;

        static List<FormPat> FormPat_List = new List<FormPat>();

        /// <summary>
        /// 有効な患者画面の数
        /// </summary>
        public static int FormPat_Count
        {
            get { return FormPat_List.Count(fp => !fp.IsDisposed); }
        }

        public static void FormPat_Remove(FormPat fp)
        {
            FormPat_List.Remove(fp);
        }

        public static void Init()
        {
            F_Input = new FormInput();
            F_Input.Hide();

            F_Print = new FormPrint();
            F_Print.Hide();
        }

        /// <summary>
        /// 表示に使う FormPat を取得する。無ければ作る。
        /// </summary>
        static FormPat GetFormPat()
        {
            FormPat f = null;

            if (FormPat_List.Count > 0)
            {
                f = FormPat_List[0];
            }

            if (f == null || !f.Created)
            {
                f = new FormPat();
                FormPat_List.Add(f);
            }

            return f;
        }

        /// <summary>
        /// FormPat を表示する。
        /// </summary>
        public static void FormPat_Show()
        {
            FormPat f = GetFormPat();

            f.Show();
            f.Activate();
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// バーコード読み取りで FormPat を表示する。
        /// 別の患者を表示中の場合は、登録していない入力内容を失わないよう切り替えてよいか確認する。
        /// </summary>
        public static void FormPat_ShowByBarcode(string pt_id)
        {
            PatBase p = PatBase.Load(pt_id);

            if (p.Id.Length == 0)
            {
                MessageBox.Show("患者ID " + pt_id + " の患者が見つかりません", "バーコード読み取り");
                return;
            }

            FormPat f = FormPat_List.Count > 0 ? FormPat_List[0] : null;

            if (f != null && f.Created && f.Visible && f.Pat.Id.Length > 0 && !f.Pat.Id.Equals(p.Id))
            {
                f.Activate();
                f.WindowState = FormWindowState.Normal;

                DialogResult result = MessageBox.Show(f,
                    "患者台帳を切り替えます。\r\n\r\n" +
                    "表示中　: " + f.Pat.Id + " " + f.Pat.Name + "\r\n" +
                    "読み取り: " + p.Id + " " + p.Name + "\r\n\r\n" +
                    "登録していない入力内容は破棄されます。よろしいですか？",
                    "バーコード読み取り", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (result != DialogResult.Yes)
                {
                    return;
                }
            }

            FormPat_Show(p.Id, FormPat.Mode.SHOW);
        }

        /// <summary>
        /// FormPat を表示する。
        /// </summary>
        public static void FormPat_Show(string pt_id, FormPat.Mode mode)
        {
            FormPat f = GetFormPat();

            f.ShowByPat(pt_id, mode);

            f.Activate();
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// FormPat で既存記録を開く場合。
        /// </summary>
        /// <param name="record_id"></param>
        public static void FormPat_Show_ByRecord(string record_id)
        {
            FormPat f = GetFormPat();

            f.ShowByRecord(record_id);
            f.Activate();
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// FormPat で予約を新規入力（手術記録を新規作成）する場合。
        /// </summary>
        /// <param name="ope_kind"></param>
        /// <param name="ope_date"></param>
        /// <param name="ope_time"></param>
        public static void FormPat_Show_ByNewRecord(string pt_id, string ope_kind, string ope_date, string ope_time)
        {
            FormPat f = GetFormPat();

            f.ShowByNewRecord(pt_id, ope_kind, ope_date, ope_time);
            f.Activate();
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// FormOpeRsv を表示する。当該患者の予約が存在すれば色を変える。
        /// </summary>
        /// <param name="pt_id"></param>
        public static void FormOpeRsv_Show(string pt_id = "")
        {
            if (F_OpeRsv == null || !F_OpeRsv.Created)
            {
                F_OpeRsv = new FormOpeRsv();
            }

            F_OpeRsv.Show();
            F_OpeRsv.Activate();

            if (F_OpeRsv.WindowState != FormWindowState.Maximized)
            {
                F_OpeRsv.WindowState = FormWindowState.Maximized;
            }

            // 当該患者の予約が存在すれば色を変える。
            if (int.TryParse(pt_id, out _))
            {
                F_OpeRsv.PtTwinkle(pt_id);
            }
        }

        /// <summary>
        /// FormOpeRsvList を表示する。
        /// </summary>
        public static void FormOpeRsvList_Show(string ope_date)
        {
            if (F_OpeRsvList == null || !F_OpeRsvList.Created)
            {
                F_OpeRsvList = new FormOpeRsvList();
            }

            F_OpeRsvList.Show(ope_date);
            F_OpeRsvList.Activate();

            if (F_OpeRsvList.WindowState == FormWindowState.Minimized)
            {
                F_OpeRsvList.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// フォームを 1 つだけ表示する。閉じられていれば作り直し、最小化されていれば元に戻す。
        /// </summary>
        static T ShowSingle<T>(T form) where T : Form, new()
        {
            if (form == null || !form.Created)
            {
                form = new T();
            }

            form.Show();
            form.Activate();

            if (form.WindowState == FormWindowState.Minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }

            return form;
        }

        /// <summary>
        /// FormFindOpeRecord を表示する。
        /// </summary>
        public static void FormFindOpeRecord_Show()
        {
            F_FindOpeRecord = ShowSingle(F_FindOpeRecord);
        }

        /// <summary>
        /// FormFindKensa を表示する。
        /// </summary>
        public static void FormFindKensa_Show()
        {
            F_FindKensa = ShowSingle(F_FindKensa);
        }

        /// <summary>
        /// FormFindSummary を表示する。
        /// </summary>
        public static void FormFindSummary_Show()
        {
            F_FindSummary = ShowSingle(F_FindSummary);
        }

        /// <summary>
        /// FormPrint を表示する。
        /// </summary>
        public static void FormPrint_Show()
        {
            F_Print = ShowSingle(F_Print);
        }

        /// <summary>
        /// FormInput のモードを変更する。
        /// </summary>
        /// <param name="mode"></param>
        public static void FormInput_ModeChange(FormInput.Mode mode)
        {
            F_Input.ModeChange(mode);
        }

        /// <summary>
        /// FormInput を表示する。
        /// </summary>
        public static DialogResult FormInput_ShowDialog()
        {
            return F_Input.ShowDialog();
        }

        /// <summary>
        /// FormInput に入力されたコメントを取得する。
        /// </summary>
        /// <returns></returns>
        public static string FormInput_CommentGet()
        {
            return F_Input.CommentBox.Text;
        }

        /// <summary>
        /// FormInput に入力されたコメントをクリアする。
        /// </summary>
        /// <returns></returns>
        public static void FormInput_CommentClear()
        {
            F_Input.CommentBox.Clear();
        }

        /// <summary>
        /// NidekARK1ListForm を表示する。
        /// </summary>
        public static void FormNidekARK1_Show()
        {
            F_NidekARK1 = ShowSingle(F_NidekARK1);
        }

        /// <summary>
        /// CanonRKF1Form を表示する。
        /// </summary>
        public static void FormCanonRKF1_Show()
        {
            F_CanonRKF1 = ShowSingle(F_CanonRKF1);
        }

        /// <summary>
        /// メイン画面以外の画面をすべて閉じる（ユーザー変更時に前のユーザーの画面を残さないため）。
        /// FormInput は作り直さずに使い回しているため閉じない。
        /// </summary>
        public static void CloseAll(Form main)
        {
            foreach (Form f in Application.OpenForms.Cast<Form>().ToList())
            {
                if (f != main && f != F_Input && !f.IsDisposed)
                {
                    f.Close();
                }
            }
        }
    }
}
