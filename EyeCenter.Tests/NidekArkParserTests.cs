using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// NidekArkParser（NIDEK ARK の CONT 解析）の動作確認。
    /// 入力は NIDEKARK-1data.csv の CONT（EYE_KENSA2 には改行 CR+LF で保存されている）。
    /// </summary>
    [TestClass]
    public class NidekArkParserTests
    {
        static string Cont(params string[] lines)
        {
            return string.Join("\r\n", lines);
        }

        // 両眼あり・左眼に L.DATA あり
        static readonly string BothEyes = Cont(
            "NIDEK ARK-1s", "2026/09/19 09:30:26", "VD = 12.00 mm", "",
            "<R>", "S      C      A",
            "+0.75  -0.50  100 8", "+0.75  -0.50  99  8",
            "Avg", "+0.75  -0.50  101 ", "L.DATA", "+0.50  -0.50  105 ", "",
            "PS    5.2", "",
            "      mm    D      deg", "R1    8.10  41.75  112", "R2    8.00  42.25   22", "Avg   8.05  42.00", "CYL         -0.50  112", "",
            "CS   12.0", "",
            "<L>", "S      C      A",
            "+0.25  -0.50  76  8",
            "Avg", "+0.25  -0.75  82  ", "L.DATA", "+0.00  -0.75  75  ", "",
            "PS    5.2", "",
            "      mm    D      deg", "R1    8.11  41.50   78", "R2    7.91  42.75  168", "Avg   8.01  42.25", "CYL         -1.25   78", "",
            "CS   11.8", "",
            "PD     67");

        [TestMethod]
        public void Parse_両眼の代表値を取り出す()
        {
            NidekArkParser.Result r = NidekArkParser.Parse(BothEyes);

            Assert.AreEqual("NIDEK ARK-1s", r.Model);
            Assert.AreEqual("2026/09/19 09:30:26", r.MeasuredAt);

            CollectionAssert.AreEqual(
                new[] { "+0.75", "-0.50", "101", "41.75", "112", "42.25", "22", "42.00", "-0.50", "112" },
                r.R.ToArray());
            CollectionAssert.AreEqual(
                new[] { "+0.25", "-0.75", "82", "41.50", "78", "42.75", "168", "42.25", "-1.25", "78" },
                r.L.ToArray());
        }

        [TestMethod]
        public void Parse_空の眼は空欄()
        {
            string cont = Cont(
                "NIDEK ARK-1a", "2026/09/18 08:30:19", "VD = 12.00 mm", "",
                "<R>", "", "", "",
                "<L>", "S      C      A",
                "+0.50  -0.50  163 7",
                "Avg", "+0.75  -1.00  3   ", "L.DATA", "+0.50  -0.50  168 ", "",
                "PS    4.9", "",
                "      mm    D      deg", "R1    7.53  44.75   68", "R2    7.50  45.00  158", "Avg   7.52  45.00", "CYL         -0.25   68", "",
                "CS   11.8");

            NidekArkParser.Result r = NidekArkParser.Parse(cont);

            CollectionAssert.AreEqual(new string[10] { "", "", "", "", "", "", "", "", "", "" }, r.R.ToArray());
            Assert.AreEqual("+0.75", r.L.S);
            Assert.AreEqual("3", r.L.A);
            Assert.AreEqual("44.75", r.L.K1);
            Assert.AreEqual("45.00", r.L.K2);
        }

        [TestMethod]
        public void Parse_R1が強主経線ならK1とK2を入れ替える()
        {
            string cont = Cont(
                "NIDEK ARK-1s", "2026/09/19 09:00:00", "",
                "<R>",
                "      mm    D      deg", "R1    7.51  45.00   82", "R2    7.75  43.50  172", "Avg   7.63  44.25", "CYL         -1.50  172");

            NidekArkParser.Eye e = NidekArkParser.Parse(cont).R;

            Assert.AreEqual("43.50", e.K1);
            Assert.AreEqual("172", e.K1Axis);
            Assert.AreEqual("45.00", e.K2);
            Assert.AreEqual("82", e.K2Axis);
        }

        [TestMethod]
        public void Parse_K1とK2が同値ならR1をK1とする()
        {
            string cont = Cont(
                "NIDEK ARK-1s", "", "<R>",
                "R1    7.90  42.75  125", "R2    7.89  42.75   35");

            NidekArkParser.Eye e = NidekArkParser.Parse(cont).R;

            Assert.AreEqual("125", e.K1Axis);
            Assert.AreEqual("35", e.K2Axis);
        }

        [TestMethod]
        public void Parse_CRLFトークン形式も読める()
        {
            NidekArkParser.Result r = NidekArkParser.Parse(BothEyes.Replace("\r\n", "<CR+LF>"));

            Assert.AreEqual("+0.75", r.R.S);
            Assert.AreEqual("-1.25", r.L.Cyl);
        }

        [TestMethod]
        public void Parse_NIDEK以外はnull()
        {
            Assert.IsNull(NidekArkParser.Parse(Cont("CANON RK-F1", "<R>", "R1  7.75  43.50  172")));
            Assert.IsNull(NidekArkParser.Parse("<R>    S      C    A"));
            Assert.IsNull(NidekArkParser.Parse(""));
            Assert.IsNull(NidekArkParser.Parse(null));
        }
    }
}
