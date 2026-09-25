using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using MedicalLibrary.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// リポジトリの EyeData.xml（レイアウト・マスタ定義）の記述ミスを検出する。
    /// EyeDict.Init は実行フォルダの EyeData.xml を読むため、テスト出力フォルダへコピーしたものを使う。
    /// </summary>
    [TestClass]
    public class EyeDataXmlTests
    {
        static readonly string[] ComboTables =
        {
            "OpeRoom", "OpeName", "Doctor", "PlanTime", "Anes", "Diag", "InOut", "InRoom", "InTime", "InTerm", "PostDeal"
        };

        static string XmlPath
        {
            get { return Path.Combine(Path.GetDirectoryName(typeof(EyeDataXmlTests).Assembly.Location), "EyeData.xml"); }
        }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            EyeDict.Init(true);
        }

        [TestMethod]
        public void XML宣言でShiftJISを明示している()
        {
            string head = File.ReadLines(XmlPath, Encoding.GetEncoding("shift-jis")).First();

            Assert.AreEqual("<?xml version=\"1.0\" encoding=\"shift_jis\"?>", head);
        }

        [TestMethod]
        public void OpeTime_曜日ごとの人数が枠と同数の数値()
        {
            foreach (DataRow r in EyeDict.EyeSet.Tables["OpeTime"].Rows)
            {
                int wakuCount = r["Waku"].ToString().Split(',').Length;

                // EyeDict.GetWakuNums と同じく '\r' だけを除去する（行末のタブが残ると "0" と判定されない）
                foreach (string line in r["Time"].ToString().Split('\n').Select(s => s.Trim('\r')).Where(s => s.Trim().Length > 0))
                {
                    string[] nums = line.Split('=')[1].Split(',');
                    string where = "OpeKind=" + r["OpeKind"] + " " + line;

                    Assert.AreEqual(wakuCount, nums.Length, where);
                    Assert.IsTrue(nums.All(n => Regex.IsMatch(n, @"^\d+$")), where);
                }
            }
        }

        [TestMethod]
        public void 同名要素が入れ子になっていない()
        {
            XDocument doc;

            using (var reader = new StreamReader(XmlPath, Encoding.GetEncoding("shift-jis")))
            {
                doc = XDocument.Load(reader, LoadOptions.SetLineInfo);
            }

            var nested = doc.Descendants()
                .Where(e => e.Parent != null && e.Parent.Name == e.Name)
                .Select(e => e.Name.LocalName + " (line " + ((System.Xml.IXmlLineInfo)e).LineNumber + ")")
                .ToList();

            Assert.AreEqual(0, nested.Count, string.Join(", ", nested));
        }

        [TestMethod]
        public void コンボの選択肢に重複や前後の空白がない()
        {
            foreach (string table in ComboTables)
            {
                string[] values = EyeDict.EyeSet.Tables[table].Rows.Cast<DataRow>().Select(r => r["Value"].ToString()).ToArray();

                foreach (string v in values)
                {
                    Assert.AreEqual(v.Trim(), v, table + " [" + v + "]");
                }

                // 空の値は区切り行として使うため重複チェックの対象外
                var dups = values.Where(v => v.Length > 0).GroupBy(v => v).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

                Assert.AreEqual(0, dups.Count, table + ": " + string.Join(", ", dups));
            }
        }
    }
}
