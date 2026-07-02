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
    public partial class FormPrint : Form
    {
        enum MODE : int
        {
            PAT_LIST = 1,
            KENSA_LIST = 2,
            WORK_SHEET = 3
        }

        MODE mode = MODE.PAT_LIST;

        Font f14 = new Font("", 14);
        Font f12 = new Font("", 12);
        Font f11 = new Font("", 11);
        Font f10 = new Font("", 10);
        Font f9 = new Font("", 9);
        Font f8 = new Font("", 8);

        Pen p2 = new Pen(Brushes.Black, 2);
        Pen p1 = new Pen(Brushes.Black, 1);

        int PageNumber = 1;

        string[] Base1 = new string[] { "部屋", "ID", "氏名", "治療方針" };
        int[] Width1 = new int[] { 30, 50, 45, 110 };
        int Height1 = 70;
        int StartX1 = 20;
        int StartY1 = 70;
        int counter1 = 0;

        string[] Base2 = new string[] { "ID", "氏名", "治療方針", "今日の検査１", "今日の検査２" };
        int[] Width2 = new int[] { 50, 45, 300, 360, 360 };
        int Height2 = 50;
        int StartX2 = 20;
        int StartY2 = 60;
        int counter2 = 0;

        string[] Base3 = new string[] { "部屋", "ID", "氏名", "治療方針" };
        int StartX3 = 30;
        int Height3 = 20;
        int StartX31 = 30;
        int StartY31 = 200;
        int StartX32 = 300;
        int StartY32 = 200;
        int StartX33 = 30;
        int StartY33 = 700;
        int counter3 = 0;

        List<InPrint1> List1;
        List<InPrint2> List2;
        List<InPrint3> List3;

        public FormPrint()
        {
            InitializeComponent();
        }

        private void PatListButton_Click(object sender, EventArgs e)
        {
            mode = MODE.PAT_LIST;

            List1 = new List<InPrint1>();
            Dictionary<string, InPrint1> dict1 = InPrint1.GetDict(PrintDate.Value.ToString("yyyyMMdd"));

            foreach (string k in dict1.Keys)
            {
                List1.Add(dict1[k]);
            }

            counter1 = 0;
            Print();
        }

        private void KensaListButton_Click(object sender, EventArgs e)
        {
            mode = MODE.KENSA_LIST;
            List2 = InPrint2.GetList(PrintDate.Value.ToString("yyyyMMdd"));
            counter2 = 0;
            Print();
        }

        private void WorksheetButton_Click(object sender, EventArgs e)
        {
            mode = MODE.WORK_SHEET;

            List3 = new List<InPrint3>();
            Dictionary<string, InPrint3> dict3 = InPrint3.GetDict(PrintDate.Value.ToString("yyyyMMdd"));

            foreach (string k in dict3.Keys)
            {
                List3.Add(dict3[k]);
            }

            counter3 = 0;
            Print();
        }

        /// <summary>
        /// 印刷を実行する。
        /// </summary>
        void Print()
        {
            printDialog1.PrinterSettings = new System.Drawing.Printing.PrinterSettings();

            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                printDocument1.PrinterSettings = printDialog1.PrinterSettings;
                PageNumber = 1;

                if (MessageBox.Show("印刷しますか？プレビューしますか？\r\nYes … 印刷する\r\nNo … プレビューする", "確認", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    printDocument1.Print();
                }
                else
                {
                    printPreviewDialog1.Document = printDocument1;
                    printPreviewDialog1.ShowDialog();
                }
            }
        }

        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            if (mode == MODE.PAT_LIST)
            {
                e.Graphics.DrawString(PrintDate.Value.ToString("M月d日") + "　入院患者一覧", f10, Brushes.Black, StartX1, 18); ;
                e.Graphics.DrawString("印刷日時　：　" + DateTime.Now.ToString("yy/MM/dd HH:mm") + "　　　　　　　　Page " + PageNumber, f8, Brushes.Black, 800, 22); ;

                int tmp_x1 = StartX1;

                for (int i = 0; i < Base1.Length; i++)
                {
                    e.Graphics.DrawLine(p1, tmp_x1, 40, tmp_x1, 70);
                    e.Graphics.DrawString(Base1[i], f8, Brushes.Black, new RectangleF(tmp_x1 + 1, 42, Width1[i] - 1, 28));
                    tmp_x1 += Width1[i];
                }

                foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrint1"].Rows)
                {
                    e.Graphics.DrawLine(p1, tmp_x1, 40, tmp_x1, 70);
                    e.Graphics.DrawString(r["Text"].ToString(), f8, Brushes.Black, new RectangleF(tmp_x1 + 1, 42, int.Parse(r["Width"].ToString()) - 1, 28));
                    tmp_x1 += int.Parse(r["Width"].ToString());
                }

                e.Graphics.DrawRectangle(p2, new Rectangle(StartX1, 40, tmp_x1 - StartX1, 30));

                int tmp_y1 = StartY1;
                int list_counter = 0;

                for (int c = counter1; c < List1.Count && tmp_y1 < 760; c++)
                {
                    InPrint1 tmpPrint = List1[c];
                    tmp_x1 = StartX1;

                    for (int i = 0; i < Base1.Length; i++)
                    {
                        e.Graphics.DrawLine(p1, tmp_x1, tmp_y1, tmp_x1, tmp_y1 + Height1);
                        e.Graphics.DrawString(tmpPrint.BaseList[i], f8, Brushes.Black, new RectangleF(tmp_x1 + 1, tmp_y1 + 2, Width1[i] - 1, Height1 - 2));
                        tmp_x1 += Width1[i];
                    }

                    list_counter = 0;

                    foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrint1"].Rows)
                    {
                        e.Graphics.DrawLine(p1, tmp_x1, tmp_y1, tmp_x1, tmp_y1 + Height1);

                        if (tmpPrint.ValueList.Count > list_counter)
                        {
                            e.Graphics.DrawString(tmpPrint.ValueList[list_counter], f8, Brushes.Black, new RectangleF(tmp_x1 + 1, tmp_y1 + 2, int.Parse(r["Width"].ToString()) - 1, Height1 - 2));
                        }

                        tmp_x1 += int.Parse(r["Width"].ToString());
                        list_counter++;
                    }

                    e.Graphics.DrawLine(p1, tmp_x1, tmp_y1, tmp_x1, tmp_y1 + Height1);
                    e.Graphics.DrawLine(p1, StartX1, tmp_y1 + Height1, tmp_x1, tmp_y1 + Height1);

                    tmp_y1 += Height1;
                    counter1++;
                }

                if (counter1 < List1.Count)
                {
                    e.HasMorePages = true;
                    PageNumber++;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
            else if (mode == MODE.KENSA_LIST)
            {
                e.Graphics.DrawString(PrintDate.Value.ToString("M月d日") + "　検査予定一覧", f10, Brushes.Black, StartX2, 18); ;
                e.Graphics.DrawString("印刷日時　：　" + DateTime.Now.ToString("yy/MM/dd HH:mm") + "　　　　　　　　Page " + PageNumber, f8, Brushes.Black, 800, 22); ;

                int tmp_x2 = StartX2;

                for (int i = 0; i < Base2.Length; i++)
                {
                    e.Graphics.DrawLine(p1, tmp_x2, 40, tmp_x2, 60);
                    e.Graphics.DrawString(Base2[i], f8, Brushes.Black, new RectangleF(tmp_x2 + 1, 42, Width2[i] - 1, 18));
                    tmp_x2 += Width2[i];
                }

                e.Graphics.DrawRectangle(p2, new Rectangle(StartX2, 40, tmp_x2 - StartX2, 20));

                int tmp_y2 = StartY2;

                for (int c = counter2; c < List2.Count && tmp_y2 < 750; c++)
                {
                    InPrint2 tmpPrint = List2[c];
                    tmp_x2 = StartX2;

                    for (int i = 0; i < Base2.Length; i++)
                    {
                        e.Graphics.DrawLine(p1, tmp_x2, tmp_y2, tmp_x2, tmp_y2 + Height2);
                        e.Graphics.DrawString(tmpPrint.BaseList[i], f8, Brushes.Black, new RectangleF(tmp_x2 + 1, tmp_y2 + 2, Width2[i] - 1, Height2 - 2));
                        tmp_x2 += Width2[i];
                    }

                    e.Graphics.DrawLine(p1, tmp_x2, tmp_y2, tmp_x2, tmp_y2 + Height2);
                    e.Graphics.DrawLine(p1, StartX2, tmp_y2 + Height2, tmp_x2, tmp_y2 + Height2);

                    tmp_y2 += Height2;
                    counter2++;
                }

                if (counter2 < List2.Count)
                {
                    e.HasMorePages = true;
                    PageNumber++;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
            else if (mode == MODE.WORK_SHEET)
            {
                if (counter3 >= List3.Count)
                {
                    return;
                }

                e.Graphics.DrawString("印刷日時　：　" + DateTime.Now.ToString("yy/MM/dd HH:mm"), f8, Brushes.Black, 500, 12); ;

                InPrint3 tmpPrint = List3[counter3];

                // 診療支援課・岡本様からの依頼により、患者カナを取得・印刷 2014/07/22
//                e.Graphics.DrawString(tmpPrint.BaseList[0] + "号室　　　" + tmpPrint.BaseList[1] + "　　" + tmpPrint.BaseList[2] + "(" + PatIn.GetPatIn(tmpPrint.BaseList[1]).Kana + ")様　　　　" + PrintDate.Value.ToString("M月d日"), f12, Brushes.Black, StartX3 + 50, 30);
                e.Graphics.DrawString(tmpPrint.BaseList[0] + "号室　　　" + tmpPrint.BaseList[1] + "　　" + tmpPrint.BaseList[2] + "(" + tmpPrint.BaseList[3] + ")様　　　　" + PrintDate.Value.ToString("M月d日"), f12, Brushes.Black, StartX3 + 50, 30);

                // 診療支援課・岡本様からの依頼により修正 2013/04/02, sakane
                e.Graphics.DrawString("退院診察（ＡＭ）　　　　術後翌日診察（ＡＭ）　　　　手術当日（ＡＭ）　　　　診察（中待合・病棟）", f12, Brushes.Black, StartX3, 70);

                e.Graphics.DrawString("【治療方針】", f9, Brushes.Black, StartX3, 110);
                e.Graphics.DrawString(tmpPrint.BaseList[4], f9, Brushes.Black, new RectangleF(StartX3 + 100, 110, 650, 90));

                int tmp_y31 = StartY31;
                int tmp_y32 = StartY32;
                int tmp_cols = 1;
                List<int> tmp_h31 = new List<int>();
                List<int> tmp_h32 = new List<int>();

                foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrint3"].Rows)
                {
                    tmp_cols = 1;

                    if (r["Area"].ToString().Equals("1"))
                    {
                        e.Graphics.DrawString("【" + r["Text"].ToString() + "】", f9, Brushes.Black, StartX31, tmp_y31);
                        int.TryParse(r["Cols"].ToString(), out tmp_cols);
                        tmp_y31 += Height3 * tmp_cols;
                        tmp_h31.Add(tmp_cols);
                    }
                    else if (r["Area"].ToString().Equals("2"))
                    {
                        e.Graphics.DrawString("【" + r["Text"].ToString() + "】", f9, Brushes.Black, StartX32, tmp_y32);
                        int.TryParse(r["Cols"].ToString(), out tmp_cols);
                        tmp_y32 += Height3 * tmp_cols;
                        tmp_h32.Add(tmp_cols);
                    }
                }

                tmp_y31 = StartY31;
                tmp_y32 = StartY32;

                for (int i = 0; i < tmpPrint.ValueList1.Count; i++)
                {
                    if (i < tmp_h31.Count)
                    {
                        tmp_cols = tmp_h31[i];
                    }
                    else
                    {
                        tmp_cols = 1;
                    }

                    e.Graphics.DrawString(tmpPrint.ValueList1[i], f9, Brushes.Black, new RectangleF(StartX31 + 100, tmp_y31, 150, Height3 * tmp_cols));
                    tmp_y31 += Height3 * tmp_cols;
                }

                for (int i = 0; i < tmpPrint.ValueList2.Count; i++)
                {
                    if (i < tmp_h32.Count)
                    {
                        tmp_cols = tmp_h32[i];
                    }
                    else
                    {
                        tmp_cols = 1;
                    }

                    e.Graphics.DrawString(tmpPrint.ValueList2[i], f9, Brushes.Black, new RectangleF(StartX32 + 100, tmp_y32, 400, Height3 * tmp_cols));
                    tmp_y32 += Height3 * tmp_cols;
                }

                int tmp_x3 = StartX33;
                int tmp_y3 = StartY33;
                tmp_y31 = StartY33;
                tmp_y32 = StartY33;

                foreach (DataRow r in EyeDict.EyeSet.Tables["SumPrintEtc3"].Rows)
                {
                    if (r["Line"].ToString().Equals("1"))
                    {
                        tmp_x3 = StartX33;
                        tmp_y3 = tmp_y31;
                        tmp_y31 += Height3;
                    }
                    else if (r["Line"].ToString().Equals("2"))
                    {
                        tmp_x3 = StartX33 + 250;
                        tmp_y3 = tmp_y32;
                        tmp_y32 += Height3;
                    }

                    e.Graphics.DrawRectangle(p1, tmp_x3, tmp_y3, 250, Height3);
                    e.Graphics.DrawLine(p1, tmp_x3 + 100, tmp_y3, tmp_x3 + 100, tmp_y3 + Height3);
                    e.Graphics.DrawString(r["Text"].ToString(), f10, Brushes.Black, tmp_x3 + 2, tmp_y3 + 2);
                }

                tmp_y3 = 900;

                // 診療支援課・岡本様からの依頼により変更 2014/05/09 by sakane
//                e.Graphics.DrawString("【散瞳】　　　　　　　右　・　左　　　　　ネオシネ　　　　　ミドリン　　　　　無散瞳", f10, Brushes.Black, StartX3, tmp_y3);
                e.Graphics.DrawString("【散瞳】　　　　　　　右　・　左　　　　　ネオシネ　　　　　オフミック　　　　　無散瞳", f10, Brushes.Black, StartX3, tmp_y3);
                e.Graphics.DrawString("【散瞳開始時間】　（　　　：　　　）", f10, Brushes.Black, StartX3, tmp_y3 + 25);

                e.Graphics.DrawString("RV=", f10, Brushes.Black, StartX3, tmp_y3 + 60);
                e.Graphics.DrawString("(               ×               D=cyl               D  Ax               °)", f10, Brushes.Black, StartX3 + 100, tmp_y3 + 60);
                e.Graphics.DrawString("LV=", f10, Brushes.Black, StartX3, tmp_y3 + 85);
                e.Graphics.DrawString("(               ×               D=cyl               D  Ax               °)", f10, Brushes.Black, StartX3 + 100, tmp_y3 + 85);

                e.Graphics.DrawString("眼圧<R>            ,            ,                <L>            ,            ,", f10, Brushes.Black, StartX3, tmp_y3 + 120);

                if (counter3 < List3.Count - 1)
                {
                    e.HasMorePages = true;
                    counter3++;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}