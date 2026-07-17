using System.Data;
using MedicalLibrary.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// InPrintCommon.GetSummaryValue（サマリー内容のKind分岐・Code一致行の値復元）の動作確認。
    /// </summary>
    [TestClass]
    public class InPrintCommonTests
    {
        static DataRow MakeRow(string kind, string code)
        {
            DataTable table = new DataTable();
            table.Columns.Add("Kind");
            table.Columns.Add("Code");

            DataRow row = table.NewRow();
            row["Kind"] = kind;
            row["Code"] = code;
            table.Rows.Add(row);

            return row;
        }

        [TestMethod]
        public void GetSummaryValue_Kind1はCont1からCode一致行の値を取得する()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont1 = "A001,値1\rA002,値2";

            string value = InPrintCommon.GetSummaryValue(sum, MakeRow("1", "A002"));

            Assert.AreEqual("値2", value);
        }

        [TestMethod]
        public void GetSummaryValue_Kind3はCont3からKind4はCont4から取得する()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont3 = "B001,見え方";
            sum.Cont4 = "C001,視力";

            Assert.AreEqual("見え方", InPrintCommon.GetSummaryValue(sum, MakeRow("3", "B001")));
            Assert.AreEqual("視力", InPrintCommon.GetSummaryValue(sum, MakeRow("4", "C001")));
        }

        [TestMethod]
        public void GetSummaryValue_Kindが2や空なら空文字を返す()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont1 = "A001,値1";
            sum.Cont2 = "A001,値2";

            Assert.AreEqual("", InPrintCommon.GetSummaryValue(sum, MakeRow("2", "A001")));
            Assert.AreEqual("", InPrintCommon.GetSummaryValue(sum, MakeRow("", "A001")));
        }

        [TestMethod]
        public void GetSummaryValue_カンマを含まない行やCode不一致は無視される()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont1 = "ノーカンマ行\rA002,値2";

            Assert.AreEqual("", InPrintCommon.GetSummaryValue(sum, MakeRow("1", "A999")), "Code不一致");
            Assert.AreEqual("値2", InPrintCommon.GetSummaryValue(sum, MakeRow("1", "A002")), "カンマなし行は読み飛ばされる");
        }

        [TestMethod]
        public void GetSummaryValue_重複Codeは最初の行が優先される()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont1 = "A001,先勝ち\rA001,後勝ちしない";

            Assert.AreEqual("先勝ち", InPrintCommon.GetSummaryValue(sum, MakeRow("1", "A001")));
        }

        [TestMethod]
        public void GetSummaryValue_値が空のCodeCommaのみは空文字になる()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont1 = "A001,";

            Assert.AreEqual("", InPrintCommon.GetSummaryValue(sum, MakeRow("1", "A001")));
        }

        [TestMethod]
        public void GetSummaryValue_CRLFトークンは改行に復元され複数値はカンマ連結される()
        {
            EyeSummary sum = new EyeSummary();
            sum.Cont1 = "A001,1行目<CR+LF>2行目,3列目";

            Assert.AreEqual("1行目\r\n2行目,3列目", InPrintCommon.GetSummaryValue(sum, MakeRow("1", "A001")));
        }

        [TestMethod]
        public void GetSummaryValue_行区切りがCRのみLFのみでも解析できる()
        {
            EyeSummary sumCr = new EyeSummary();
            sumCr.Cont1 = "A001,値1\rA002,値2";

            EyeSummary sumLf = new EyeSummary();
            sumLf.Cont1 = "A001,値1\nA002,値2";

            Assert.AreEqual("値2", InPrintCommon.GetSummaryValue(sumCr, MakeRow("1", "A002")));
            Assert.AreEqual("値2", InPrintCommon.GetSummaryValue(sumLf, MakeRow("1", "A002")));
        }
    }
}
