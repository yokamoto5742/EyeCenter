using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// ContData（Cont 形式 "code,value" 行の相互変換）の動作確認。
    /// </summary>
    [TestClass]
    public class ContDataTests
    {
        // ---------- Parse ----------

        [TestMethod]
        public void Parse_基本形をcodeとvalueに分解する()
        {
            Dictionary<string, string> dict = ContData.Parse("101R,1.2\r\n101L,0.8");

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("1.2", dict["101R"]);
            Assert.AreEqual("0.8", dict["101L"]);
        }

        [TestMethod]
        public void Parse_value内のカンマは保持する()
        {
            Dictionary<string, string> dict = ContData.Parse("301R,10,12,14");

            Assert.AreEqual("10,12,14", dict["301R"]);
        }

        [TestMethod]
        public void Parse_CRLFトークンは改行に戻す()
        {
            Dictionary<string, string> dict = ContData.Parse("901R,1行目<CR+LF>2行目");

            Assert.AreEqual("1行目\r\n2行目", dict["901R"]);
        }

        [TestMethod]
        public void Parse_値の無い行と空行は無視する()
        {
            Dictionary<string, string> dict = ContData.Parse("101R\r\n\r\n102R,x\r\n");

            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("x", dict["102R"]);
        }

        [TestMethod]
        public void Parse_同一codeは先勝ちとする()
        {
            Dictionary<string, string> dict = ContData.Parse("101R,先\r\n101R,後");

            Assert.AreEqual("先", dict["101R"]);
        }

        [TestMethod]
        public void Parse_空文字とnullは空のDictionaryを返す()
        {
            Assert.AreEqual(0, ContData.Parse("").Count);
            Assert.AreEqual(0, ContData.Parse(null).Count);
        }

        [TestMethod]
        public void Parse_値が空文字の行も取り込む()
        {
            Dictionary<string, string> dict = ContData.Parse("101R,");

            Assert.AreEqual("", dict["101R"]);
        }

        // ---------- ParseInto ----------

        [TestMethod]
        public void ParseInto_既存キーだけを上書きしキーは追加しない()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("101R", "");
            dict.Add("101L", "");

            ContData.ParseInto("101R,1.2\r\n999X,無関係", dict);

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual("1.2", dict["101R"]);
            Assert.AreEqual("", dict["101L"]);
            Assert.IsFalse(dict.ContainsKey("999X"));
        }

        [TestMethod]
        public void ParseInto_空文字なら何も変更しない()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            dict.Add("101R", "初期値");

            ContData.ParseInto("", dict);

            Assert.AreEqual("初期値", dict["101R"]);
        }

        // ---------- Build ----------

        [TestMethod]
        public void Build_TextBoxとComboBoxはTagとTextを1行にする()
        {
            using (Panel panel = new Panel())
            {
                TextBox textBox = new TextBox();
                textBox.Tag = "101R";
                textBox.Text = "1.2";
                panel.Controls.Add(textBox);

                ComboBox comboBox = new ComboBox();
                comboBox.Tag = "102R";
                comboBox.Items.Add("n.c.");
                comboBox.Text = "n.c.";
                panel.Controls.Add(comboBox);

                Assert.AreEqual("101R,1.2\r\n102R,n.c.", ContData.Build(panel.Controls));
            }
        }

        [TestMethod]
        public void Build_CheckBoxはチェック時のみTagと1を出力する()
        {
            using (Panel panel = new Panel())
            {
                CheckBox checkedBox = new CheckBox();
                checkedBox.Tag = "201";
                checkedBox.Checked = true;
                panel.Controls.Add(checkedBox);

                CheckBox uncheckedBox = new CheckBox();
                uncheckedBox.Tag = "202";
                panel.Controls.Add(uncheckedBox);

                Assert.AreEqual("201,1", ContData.Build(panel.Controls));
            }
        }

        [TestMethod]
        public void Build_Textが空かTagが空のコントロールは出力しない()
        {
            using (Panel panel = new Panel())
            {
                TextBox emptyText = new TextBox();
                emptyText.Tag = "101R";
                panel.Controls.Add(emptyText);

                TextBox emptyTag = new TextBox();
                emptyTag.Tag = "";
                emptyTag.Text = "値";
                panel.Controls.Add(emptyTag);

                TextBox nullTag = new TextBox();
                nullTag.Text = "値";
                panel.Controls.Add(nullTag);

                Label label = new Label();
                label.Tag = "301R";
                label.Text = "ラベルは対象外";
                panel.Controls.Add(label);

                Assert.AreEqual("", ContData.Build(panel.Controls));
            }
        }

        [TestMethod]
        public void Build_Text内の改行はトークン化される()
        {
            using (Panel panel = new Panel())
            {
                TextBox textBox = new TextBox();
                textBox.Multiline = true;
                textBox.Tag = "901R";
                textBox.Text = "1行目\r\n2行目";
                panel.Controls.Add(textBox);

                Assert.AreEqual("901R,1行目<CR+LF>2行目", ContData.Build(panel.Controls));
            }
        }

        [TestMethod]
        public void Build_タブページ全体をまとめて組み立てる()
        {
            using (TabControl tab = new TabControl())
            {
                TabPage page1 = new TabPage();
                TextBox box1 = new TextBox();
                box1.Tag = "A1";
                box1.Text = "v1";
                page1.Controls.Add(box1);
                tab.TabPages.Add(page1);

                TabPage page2 = new TabPage();
                TextBox box2 = new TextBox();
                box2.Tag = "B1";
                box2.Text = "v2";
                page2.Controls.Add(box2);
                tab.TabPages.Add(page2);

                Assert.AreEqual("A1,v1\r\nB1,v2", ContData.Build(tab.TabPages));
            }
        }

        // ---------- 相互変換 ----------

        [TestMethod]
        public void BuildとParseで値が往復できる()
        {
            using (Panel panel = new Panel())
            {
                TextBox textBox = new TextBox();
                textBox.Multiline = true;
                textBox.Tag = "901R";
                textBox.Text = "複数,カンマ\r\n改行あり";
                panel.Controls.Add(textBox);

                Dictionary<string, string> dict = ContData.Parse(ContData.Build(panel.Controls));

                Assert.AreEqual("複数,カンマ\r\n改行あり", dict["901R"]);
            }
        }
    }
}
