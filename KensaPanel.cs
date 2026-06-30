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
    class KensaPanel : Panel
    {
        public bool Edited
        {
            get
            {
                bool b = false;

                foreach (Control c in this.Controls)
                {
                    // true があれば終了
                    if (b) break;

                    if (c is TabControl)
                    {
                        TabControl tc = (TabControl)c;

                        foreach (TabPage tp in tc.TabPages)
                        {
                            // true があれば終了
                            if (b) break;

                            if (tp is KensaTabPage)
                            {
                                b = ((KensaTabPage)tp).Edited;
                            }
                            else if (tp is KensaTabPage2)
                            {
                                b = ((KensaTabPage2)tp).Edited;
                            }
                        }
                    }
                }

                return b;
            }
            set
            {
                foreach (Control c in this.Controls)
                {
                    if (c is TabControl)
                    {
                        TabControl tc = (TabControl)c;

                        foreach (TabPage tp in tc.TabPages)
                        {
                            if (tp is KensaTabPage)
                            {
                                ((KensaTabPage)tp).Edited = value;
                            }
                            else if (tp is KensaTabPage2)
                            {
                                ((KensaTabPage2)tp).Edited = value;
                            }
                        }
                    }
                }
            }
        }

        public KensaPanel()
        {
            this.Location = new Point(2, 25);
            this.Size = new Size(998, 516);
            this.Name = "KensaPanel";
            this.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;

            DataTable tmpTable = EyeDict.EyeSet.Tables["KensaTabControl"];
            TabControl tmpControl;

            foreach (DataRow r in tmpTable.Rows)
            {
                tmpControl = new TabControl();
                tmpControl.Name = r["ID"].ToString();
                tmpControl.Location = new Point(int.Parse(r["X"].ToString()), int.Parse(r["Y"].ToString()));
                tmpControl.Size = new Size(int.Parse(r["Width"].ToString()), int.Parse(r["Height"].ToString()));

                // タブの Anchor 設定
                if (r["ID"].ToString().Equals("1"))
                {
                    tmpControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                }
                else if (r["ID"].ToString().Equals("2"))
                {
                    tmpControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Right;
                }
                else if (r["ID"].ToString().Equals("3"))
                {
                    tmpControl.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                }

                this.Controls.Add(tmpControl);
            }

            tmpTable = EyeDict.EyeSet.Tables["KensaPage"];

            foreach (DataRow r in tmpTable.Rows)
            {
                if (!r["PageVisible"].ToString().Equals("1"))
                {
                    continue;
                }

                if (r["PageType"].ToString().Equals("1"))
                {
                    KensaTabPage tmpPage = new KensaTabPage(r["ID"].ToString());
                    tmpPage.Name = r["Name"].ToString();
                    tmpPage.Text = r["Text"].ToString();
                    tmpPage.AutoScroll = true;
                    tmpPage.Tag = r["ID"].ToString();

                    tmpControl = (TabControl)(this.Controls[r["Tab"].ToString()]);
                    tmpControl.TabPages.Add(tmpPage);
                }
                else if (r["PageType"].ToString().Equals("2"))
                {
                    KensaTabPage2 tmpPage = new KensaTabPage2(r["ID"].ToString());
                    tmpPage.Name = r["Name"].ToString();
                    tmpPage.Text = r["Text"].ToString();
                    tmpPage.AutoScroll = true;
                    tmpPage.Tag = r["ID"].ToString();

                    tmpControl = (TabControl)(this.Controls[r["Tab"].ToString()]);
                    tmpControl.TabPages.Add(tmpPage);
                }
            }
        }

        /// <summary>
        /// 検査結果をクリアする。
        /// </summary>
        public bool KensaClear()
        {
            foreach (Control c1 in this.Controls)
            {
                if (c1.GetType().Name.Equals("TabControl"))
                {
                    foreach (TabPage tp in ((TabControl)c1).TabPages)
                    {
                        ((KensaTabPageBase)tp).KensaClear();

                        if (tp.Name.Equals("コントラスト感度"))
                        {
                            ((ContrastPanel)(tp.Controls["KensaPanel"].Controls["ContrastPanel"])).KensaClear();
                        }
                    }
                }
            }

            this.Edited = false;

            return true;
        }

        /// <summary>
        /// 検査結果を表示する。
        /// </summary>
        public bool KensaShow(string pt_id, string kensa_date)
        {
            int i = 0;
            DateTime d = DateTime.Now;

            if (pt_id.Length == 0 || !int.TryParse(pt_id, out i) || kensa_date.Length != 8 || !DateTime.TryParse(kensa_date.Insert(4, "/").Insert(7, "/"), out d))
            {
                return false;
            }

            if (!this.KensaClear())
            {
                return false;
            }

            List<EyeKensa> tmpList = EyeKensa.LoadByPatDate(pt_id, kensa_date);
            List<EyeKensa2> tmpList2 = EyeKensa2.LoadByPatDate(pt_id, kensa_date);

            foreach (Control c in this.Controls)
            {
                if (c.GetType().Name.Equals("TabControl"))
                {
                    foreach (TabPage tp in ((TabControl)c).TabPages)
                    {
                        bool kensa1 = false;

                        KensaTabPageBase tmpPage = (KensaTabPageBase)tp;
                        tmpPage.KensaClear();

                        foreach (EyeKensa k in tmpList)
                        {
                            if (tmpPage.Tag.ToString().Equals(k.KensaId))
                            {
                                kensa1 = true;
                                tmpPage.KensaShow(k);
                                break;
                            }
                        }

                        if (!kensa1)
                        {
                            foreach (EyeKensa2 k in tmpList2)
                            {
                                if (tmpPage.Tag.ToString().Equals(k.KensaId))
                                {
                                    tmpPage.KensaShow(k, tmpList2.Count);
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            this.Edited = false;

            return true;
        }
    }
}
