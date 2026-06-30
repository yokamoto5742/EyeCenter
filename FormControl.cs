using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using MedicalLibrary.Boundary;

namespace EyeCenter
{
    class FormControl
    {
        /// <summary>
        /// 複数の患者画面を開けるかどうか
        /// </summary>
        public static bool MultiPat = false;

        static FormList F_List;
        static FormOpeRsv F_OpeRsv;
        static FormOpeRsvList F_OpeRsvList;
        static FormFindOpeRecord F_FindOpeRecord;
        static FormFindKensa F_FindKensa;
        static FormFindSummary F_FindSummary;
        static FormPrint F_Print;
        static FormInput F_Input;
        static FormRsvPatList F_RsvPatList;

        static List<FormPat> FormPat_List = new List<FormPat>();

        /// <summary>
        /// 有効な患者画面の数
        /// </summary>
        public static int FormPat_Count
        {
            get
            {
                int c = 0;

                foreach (FormPat fp in FormPat_List)
                {
                    if (!fp.IsDisposed)
                    {
                        c++;
                    }
                }

                return c;
            }
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
        /// FormList を表示する。
        /// </summary>
        public static void FormList_Show()
        {
            if (F_List == null || !F_List.Created)
            {
                F_List = new FormList();
            }

            F_List.Show();
            F_List.Activate();
            F_List.BringToFront();
            F_List.WindowState = FormWindowState.Normal;

            F_List.EyeListViewShow1();
        }

        /// <summary>
        /// FormPat を表示する。
        /// </summary>
        public static void FormPat_Show()
        {
            FormPat f = null;

            if (MultiPat)
            {
                f = new FormPat();
                FormPat_List.Add(f);
            }
            else
            {
                if (FormPat_List.Count > 0)
                {
                    f = FormPat_List[0];
                }
                else
                {
                    f = new FormPat();
                    FormPat_List.Add(f);
                }
            }

            f.Show();
            f.Activate();
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
        }

        /// <summary>
        /// FormPat を表示する。
        /// </summary>
        public static void FormPat_Show(string pt_id, FormPat.Mode mode)
        {
            FormPat f = null;

            if (MultiPat)
            {
                foreach (FormPat fp in FormPat_List)
                {
                    if (fp.Pat.Id.Equals(pt_id))
                    {
                        f = fp;
                        break;
                    }
                }

                if (f == null || !f.Created)
                {
                    f = new FormPat();
                    f.ShowByPat(pt_id, FormPat.Mode.SHOW);
                    FormPat_List.Add(f);
                }
            }
            else
            {
                if (FormPat_List.Count > 0)
                {
                    f = FormPat_List[0];
                }

                if (f == null || !f.Created)
                {
                    f = new FormPat();
                    FormPat_List.Add(f);
                }

                f.ShowByPat(pt_id, FormPat.Mode.SHOW);
            }

            f.Activate();
            f.BringToFront();
            f.WindowState = FormWindowState.Normal;
            //            F_Pat.OrgSize();
        }

        /// <summary>
        /// FormPat で既存記録を開く場合。
        /// </summary>
        /// <param name="record_id"></param>
        public static void FormPat_Show_ByRecord(string record_id)
        {
            string pt_id = EyeOpe.Load(record_id).PtId;

            FormPat f = null;

            if (MultiPat)
            {
                foreach (FormPat fp in FormPat_List)
                {
                    if (fp.Pat.Id.Equals(pt_id))
                    {
                        f = fp;
                        break;
                    }
                }

                if (f == null || !f.Created)
                {
                    f = new FormPat();
                    FormPat_List.Add(f);
                }
            }
            else
            {
                if (FormPat_List.Count > 0)
                {
                    f = FormPat_List[0];
                }

                if (f == null || !f.Created)
                {
                    f = new FormPat();
                    FormPat_List.Add(f);
                }
            }

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
            FormPat f = null;

            if (MultiPat)
            {
                foreach (FormPat fp in FormPat_List)
                {
                    if (fp.Pat.Id.Equals(pt_id))
                    {
                        f = fp;
                        break;
                    }
                }

                if (f == null || !f.Created)
                {
                    f = new FormPat();
                    FormPat_List.Add(f);
                }
            }
            else
            {
                if (FormPat_List.Count > 0)
                {
                    f = FormPat_List[0];
                }

                if (f == null || !f.Created)
                {
                    f = new FormPat();
                    FormPat_List.Add(f);
                }
            }

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
            int i = 0;

            if (pt_id.Length > 0 && int.TryParse(pt_id, out i))
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
        /// FormFindOpeRecord を表示する。
        /// </summary>
        public static void FormFindOpeRecord_Show()
        {
            if (F_FindOpeRecord == null || !F_FindOpeRecord.Created)
            {
                F_FindOpeRecord = new FormFindOpeRecord();
            }

            F_FindOpeRecord.Show();
            F_FindOpeRecord.Activate();

            if (F_FindOpeRecord.WindowState == FormWindowState.Minimized)
            {
                F_FindOpeRecord.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// FormFindKensa を表示する。
        /// </summary>
        public static void FormFindKensa_Show()
        {
            if (F_FindKensa == null || !F_FindKensa.Created)
            {
                F_FindKensa = new FormFindKensa();
            }

            F_FindKensa.Show();
            F_FindKensa.Activate();

            if (F_FindKensa.WindowState == FormWindowState.Minimized)
            {
                F_FindKensa.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// FormFindSummary を表示する。
        /// </summary>
        public static void FormFindSummary_Show()
        {
            if (F_FindSummary == null || !F_FindSummary.Created)
            {
                F_FindSummary = new FormFindSummary();
            }

            F_FindSummary.Show();
            F_FindSummary.Activate();

            if (F_FindSummary.WindowState == FormWindowState.Minimized)
            {
                F_FindSummary.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// FormPrint を表示する。
        /// </summary>
        public static void FormPrint_Show()
        {
            if (F_Print == null || !F_Print.Created)
            {
                F_Print = new FormPrint();
            }

            F_Print.Show();
            F_Print.Activate();

            if (F_Print.WindowState == FormWindowState.Minimized)
            {
                F_Print.WindowState = FormWindowState.Normal;
            }
        }

        /// <summary>
        /// ワークシートを印刷する。
        /// </summary>
        /// <param name="room"></param>
        /// <param name="pt_id"></param>
        /// <param name="pt_name"></param>
        public static void FormPrint_WorksheetPrint(string room, string pt_id, string pt_name, string pt_kana)
        {
            F_Print.WorksheetPrint(room, pt_id, pt_name, pt_kana);
        }

        /// <summary>
        /// FormRsvPatList を表示する。
        /// </summary>
        public static void FormRsvPatList_Show()
        {
            if (F_RsvPatList == null || !F_RsvPatList.Created)
            {
                F_RsvPatList = new FormRsvPatList();
            }

            F_RsvPatList.Show();
            F_RsvPatList.Activate();

            if (F_RsvPatList.WindowState == FormWindowState.Minimized)
            {
                F_RsvPatList.WindowState = FormWindowState.Normal;
            }
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
    }
}
