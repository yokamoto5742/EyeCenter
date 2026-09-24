using System.Collections.Generic;
using MedicalLibrary.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// MedicalLibrary の DateTimeAgent（日付の書式変換・和暦変換）の動作確認。
    /// 期待値はリファクタリング前の実装の出力（ja-JP 環境）で固定している。
    /// </summary>
    [TestClass]
    public class DateTimeAgentTests
    {
        static readonly object[,] DateFormatCases =
        {
            { "18680908", DateTimeAgent.DateFormatKind.LONG, "1868/09/08" },
            { "18680908", DateTimeAgent.DateFormatKind.SHORT, "68/09/08" },
            { "18680908", DateTimeAgent.DateFormatKind.WLONG, "1868/09/08(火)" },
            { "18680908", DateTimeAgent.DateFormatKind.WSHORT, "68/09/08(火)" },
            { "18680908", DateTimeAgent.DateFormatKind.J1, "明治元年9月8日" },
            { "18680908", DateTimeAgent.DateFormatKind.J2, "M01/9/8" },
            { "18680908", DateTimeAgent.DateFormatKind.JW1, "明治元年9月8日(火)" },
            { "18680908", DateTimeAgent.DateFormatKind.JW2, "M01/9/8(火)" },
            { "18680908", DateTimeAgent.DateFormatKind.MD, "9/8" },
            { "18680908", DateTimeAgent.DateFormatKind.MDD, "9/08" },
            { "18680908", DateTimeAgent.DateFormatKind.MDW, "9/8(火)" },
            { "19120729", DateTimeAgent.DateFormatKind.LONG, "1912/07/29" },
            { "19120729", DateTimeAgent.DateFormatKind.SHORT, "12/07/29" },
            { "19120729", DateTimeAgent.DateFormatKind.WLONG, "1912/07/29(月)" },
            { "19120729", DateTimeAgent.DateFormatKind.WSHORT, "12/07/29(月)" },
            { "19120729", DateTimeAgent.DateFormatKind.J1, "明治45年7月29日" },
            { "19120729", DateTimeAgent.DateFormatKind.J2, "M45/7/29" },
            { "19120729", DateTimeAgent.DateFormatKind.JW1, "明治45年7月29日(月)" },
            { "19120729", DateTimeAgent.DateFormatKind.JW2, "M45/7/29(月)" },
            { "19120729", DateTimeAgent.DateFormatKind.MD, "7/29" },
            { "19120729", DateTimeAgent.DateFormatKind.MDD, "7/29" },
            { "19120729", DateTimeAgent.DateFormatKind.MDW, "7/29(月)" },
            { "19120730", DateTimeAgent.DateFormatKind.LONG, "1912/07/30" },
            { "19120730", DateTimeAgent.DateFormatKind.SHORT, "12/07/30" },
            { "19120730", DateTimeAgent.DateFormatKind.WLONG, "1912/07/30(火)" },
            { "19120730", DateTimeAgent.DateFormatKind.WSHORT, "12/07/30(火)" },
            { "19120730", DateTimeAgent.DateFormatKind.J1, "大正元年7月30日" },
            { "19120730", DateTimeAgent.DateFormatKind.J2, "T01/7/30" },
            { "19120730", DateTimeAgent.DateFormatKind.JW1, "大正元年7月30日(火)" },
            { "19120730", DateTimeAgent.DateFormatKind.JW2, "T01/7/30(火)" },
            { "19120730", DateTimeAgent.DateFormatKind.MD, "7/30" },
            { "19120730", DateTimeAgent.DateFormatKind.MDD, "7/30" },
            { "19120730", DateTimeAgent.DateFormatKind.MDW, "7/30(火)" },
            { "19261224", DateTimeAgent.DateFormatKind.LONG, "1926/12/24" },
            { "19261224", DateTimeAgent.DateFormatKind.SHORT, "26/12/24" },
            { "19261224", DateTimeAgent.DateFormatKind.WLONG, "1926/12/24(金)" },
            { "19261224", DateTimeAgent.DateFormatKind.WSHORT, "26/12/24(金)" },
            { "19261224", DateTimeAgent.DateFormatKind.J1, "大正15年12月24日" },
            { "19261224", DateTimeAgent.DateFormatKind.J2, "T15/12/24" },
            { "19261224", DateTimeAgent.DateFormatKind.JW1, "大正15年12月24日(金)" },
            { "19261224", DateTimeAgent.DateFormatKind.JW2, "T15/12/24(金)" },
            { "19261224", DateTimeAgent.DateFormatKind.MD, "12/24" },
            { "19261224", DateTimeAgent.DateFormatKind.MDD, "12/24" },
            { "19261224", DateTimeAgent.DateFormatKind.MDW, "12/24(金)" },
            { "19261225", DateTimeAgent.DateFormatKind.LONG, "1926/12/25" },
            { "19261225", DateTimeAgent.DateFormatKind.SHORT, "26/12/25" },
            { "19261225", DateTimeAgent.DateFormatKind.WLONG, "1926/12/25(土)" },
            { "19261225", DateTimeAgent.DateFormatKind.WSHORT, "26/12/25(土)" },
            { "19261225", DateTimeAgent.DateFormatKind.J1, "昭和元年12月25日" },
            { "19261225", DateTimeAgent.DateFormatKind.J2, "S01/12/25" },
            { "19261225", DateTimeAgent.DateFormatKind.JW1, "昭和元年12月25日(土)" },
            { "19261225", DateTimeAgent.DateFormatKind.JW2, "S01/12/25(土)" },
            { "19261225", DateTimeAgent.DateFormatKind.MD, "12/25" },
            { "19261225", DateTimeAgent.DateFormatKind.MDD, "12/25" },
            { "19261225", DateTimeAgent.DateFormatKind.MDW, "12/25(土)" },
            { "19890107", DateTimeAgent.DateFormatKind.LONG, "1989/01/07" },
            { "19890107", DateTimeAgent.DateFormatKind.SHORT, "89/01/07" },
            { "19890107", DateTimeAgent.DateFormatKind.WLONG, "1989/01/07(土)" },
            { "19890107", DateTimeAgent.DateFormatKind.WSHORT, "89/01/07(土)" },
            { "19890107", DateTimeAgent.DateFormatKind.J1, "昭和64年1月7日" },
            { "19890107", DateTimeAgent.DateFormatKind.J2, "S64/1/7" },
            { "19890107", DateTimeAgent.DateFormatKind.JW1, "昭和64年1月7日(土)" },
            { "19890107", DateTimeAgent.DateFormatKind.JW2, "S64/1/7(土)" },
            { "19890107", DateTimeAgent.DateFormatKind.MD, "1/7" },
            { "19890107", DateTimeAgent.DateFormatKind.MDD, "1/07" },
            { "19890107", DateTimeAgent.DateFormatKind.MDW, "1/7(土)" },
            { "19890108", DateTimeAgent.DateFormatKind.LONG, "1989/01/08" },
            { "19890108", DateTimeAgent.DateFormatKind.SHORT, "89/01/08" },
            { "19890108", DateTimeAgent.DateFormatKind.WLONG, "1989/01/08(日)" },
            { "19890108", DateTimeAgent.DateFormatKind.WSHORT, "89/01/08(日)" },
            { "19890108", DateTimeAgent.DateFormatKind.J1, "平成元年1月8日" },
            { "19890108", DateTimeAgent.DateFormatKind.J2, "H01/1/8" },
            { "19890108", DateTimeAgent.DateFormatKind.JW1, "平成元年1月8日(日)" },
            { "19890108", DateTimeAgent.DateFormatKind.JW2, "H01/1/8(日)" },
            { "19890108", DateTimeAgent.DateFormatKind.MD, "1/8" },
            { "19890108", DateTimeAgent.DateFormatKind.MDD, "1/08" },
            { "19890108", DateTimeAgent.DateFormatKind.MDW, "1/8(日)" },
            { "20080501", DateTimeAgent.DateFormatKind.LONG, "2008/05/01" },
            { "20080501", DateTimeAgent.DateFormatKind.SHORT, "08/05/01" },
            { "20080501", DateTimeAgent.DateFormatKind.WLONG, "2008/05/01(木)" },
            { "20080501", DateTimeAgent.DateFormatKind.WSHORT, "08/05/01(木)" },
            { "20080501", DateTimeAgent.DateFormatKind.J1, "平成20年5月1日" },
            { "20080501", DateTimeAgent.DateFormatKind.J2, "H20/5/1" },
            { "20080501", DateTimeAgent.DateFormatKind.JW1, "平成20年5月1日(木)" },
            { "20080501", DateTimeAgent.DateFormatKind.JW2, "H20/5/1(木)" },
            { "20080501", DateTimeAgent.DateFormatKind.MD, "5/1" },
            { "20080501", DateTimeAgent.DateFormatKind.MDD, "5/01" },
            { "20080501", DateTimeAgent.DateFormatKind.MDW, "5/1(木)" },
            { "20190430", DateTimeAgent.DateFormatKind.LONG, "2019/04/30" },
            { "20190430", DateTimeAgent.DateFormatKind.SHORT, "19/04/30" },
            { "20190430", DateTimeAgent.DateFormatKind.WLONG, "2019/04/30(火)" },
            { "20190430", DateTimeAgent.DateFormatKind.WSHORT, "19/04/30(火)" },
            { "20190430", DateTimeAgent.DateFormatKind.J1, "平成31年4月30日" },
            { "20190430", DateTimeAgent.DateFormatKind.J2, "H31/4/30" },
            { "20190430", DateTimeAgent.DateFormatKind.JW1, "平成31年4月30日(火)" },
            { "20190430", DateTimeAgent.DateFormatKind.JW2, "H31/4/30(火)" },
            { "20190430", DateTimeAgent.DateFormatKind.MD, "4/30" },
            { "20190430", DateTimeAgent.DateFormatKind.MDD, "4/30" },
            { "20190430", DateTimeAgent.DateFormatKind.MDW, "4/30(火)" },
            { "20190501", DateTimeAgent.DateFormatKind.LONG, "2019/05/01" },
            { "20190501", DateTimeAgent.DateFormatKind.SHORT, "19/05/01" },
            { "20190501", DateTimeAgent.DateFormatKind.WLONG, "2019/05/01(水)" },
            { "20190501", DateTimeAgent.DateFormatKind.WSHORT, "19/05/01(水)" },
            { "20190501", DateTimeAgent.DateFormatKind.J1, "令和01年5月1日" },
            { "20190501", DateTimeAgent.DateFormatKind.J2, "R01/5/1" },
            { "20190501", DateTimeAgent.DateFormatKind.JW1, "令和01年5月1日(水)" },
            { "20190501", DateTimeAgent.DateFormatKind.JW2, "R01/5/1(水)" },
            { "20190501", DateTimeAgent.DateFormatKind.MD, "5/1" },
            { "20190501", DateTimeAgent.DateFormatKind.MDD, "5/01" },
            { "20190501", DateTimeAgent.DateFormatKind.MDW, "5/1(水)" },
            { "20200101", DateTimeAgent.DateFormatKind.LONG, "2020/01/01" },
            { "20200101", DateTimeAgent.DateFormatKind.SHORT, "20/01/01" },
            { "20200101", DateTimeAgent.DateFormatKind.WLONG, "2020/01/01(水)" },
            { "20200101", DateTimeAgent.DateFormatKind.WSHORT, "20/01/01(水)" },
            { "20200101", DateTimeAgent.DateFormatKind.J1, "令和02年1月1日" },
            { "20200101", DateTimeAgent.DateFormatKind.J2, "R02/1/1" },
            { "20200101", DateTimeAgent.DateFormatKind.JW1, "令和02年1月1日(水)" },
            { "20200101", DateTimeAgent.DateFormatKind.JW2, "R02/1/1(水)" },
            { "20200101", DateTimeAgent.DateFormatKind.MD, "1/1" },
            { "20200101", DateTimeAgent.DateFormatKind.MDD, "1/01" },
            { "20200101", DateTimeAgent.DateFormatKind.MDW, "1/1(水)" },
            { "20261231", DateTimeAgent.DateFormatKind.LONG, "2026/12/31" },
            { "20261231", DateTimeAgent.DateFormatKind.SHORT, "26/12/31" },
            { "20261231", DateTimeAgent.DateFormatKind.WLONG, "2026/12/31(木)" },
            { "20261231", DateTimeAgent.DateFormatKind.WSHORT, "26/12/31(木)" },
            { "20261231", DateTimeAgent.DateFormatKind.J1, "令和08年12月31日" },
            { "20261231", DateTimeAgent.DateFormatKind.J2, "R08/12/31" },
            { "20261231", DateTimeAgent.DateFormatKind.JW1, "令和08年12月31日(木)" },
            { "20261231", DateTimeAgent.DateFormatKind.JW2, "R08/12/31(木)" },
            { "20261231", DateTimeAgent.DateFormatKind.MD, "12/31" },
            { "20261231", DateTimeAgent.DateFormatKind.MDD, "12/31" },
            { "20261231", DateTimeAgent.DateFormatKind.MDW, "12/31(木)" },
            { "00000000", DateTimeAgent.DateFormatKind.LONG, "0000/00/00" },
            { "00000000", DateTimeAgent.DateFormatKind.SHORT, "00/00/00" },
            { "00000000", DateTimeAgent.DateFormatKind.WLONG, "" },
            { "00000000", DateTimeAgent.DateFormatKind.WSHORT, "" },
            { "00000000", DateTimeAgent.DateFormatKind.J1, "" },
            { "00000000", DateTimeAgent.DateFormatKind.J2, "" },
            { "00000000", DateTimeAgent.DateFormatKind.JW1, "" },
            { "00000000", DateTimeAgent.DateFormatKind.JW2, "" },
            { "00000000", DateTimeAgent.DateFormatKind.MD, "" },
            { "00000000", DateTimeAgent.DateFormatKind.MDD, "" },
            { "00000000", DateTimeAgent.DateFormatKind.MDW, "" },
            { "20260230", DateTimeAgent.DateFormatKind.LONG, "2026/02/30" },
            { "20260230", DateTimeAgent.DateFormatKind.SHORT, "26/02/30" },
            { "20260230", DateTimeAgent.DateFormatKind.WLONG, "" },
            { "20260230", DateTimeAgent.DateFormatKind.WSHORT, "" },
            { "20260230", DateTimeAgent.DateFormatKind.J1, "" },
            { "20260230", DateTimeAgent.DateFormatKind.J2, "" },
            { "20260230", DateTimeAgent.DateFormatKind.JW1, "" },
            { "20260230", DateTimeAgent.DateFormatKind.JW2, "" },
            { "20260230", DateTimeAgent.DateFormatKind.MD, "" },
            { "20260230", DateTimeAgent.DateFormatKind.MDD, "" },
            { "20260230", DateTimeAgent.DateFormatKind.MDW, "" },
            { "2026091", DateTimeAgent.DateFormatKind.LONG, "" },
            { "2026091", DateTimeAgent.DateFormatKind.SHORT, "" },
            { "2026091", DateTimeAgent.DateFormatKind.WLONG, "" },
            { "2026091", DateTimeAgent.DateFormatKind.WSHORT, "" },
            { "2026091", DateTimeAgent.DateFormatKind.J1, "" },
            { "2026091", DateTimeAgent.DateFormatKind.J2, "" },
            { "2026091", DateTimeAgent.DateFormatKind.JW1, "" },
            { "2026091", DateTimeAgent.DateFormatKind.JW2, "" },
            { "2026091", DateTimeAgent.DateFormatKind.MD, "" },
            { "2026091", DateTimeAgent.DateFormatKind.MDD, "" },
            { "2026091", DateTimeAgent.DateFormatKind.MDW, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.LONG, "abcd/ef/gh" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.SHORT, "cd/ef/gh" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.WLONG, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.WSHORT, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.J1, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.J2, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.JW1, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.JW2, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.MD, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.MDD, "" },
            { "abcdefgh", DateTimeAgent.DateFormatKind.MDW, "" },
        };

        [TestMethod]
        public void DateFormat_全種類の書式()
        {
            List<string> errors = new List<string>();

            for (int i = 0; i < DateFormatCases.GetLength(0); i++)
            {
                string date = (string)DateFormatCases[i, 0];
                DateTimeAgent.DateFormatKind kind = (DateTimeAgent.DateFormatKind)DateFormatCases[i, 1];
                string expected = (string)DateFormatCases[i, 2];
                string actual = DateTimeAgent.DateFormat(date, kind);

                if (actual != expected)
                {
                    errors.Add(date + " " + kind + ": expected=" + expected + " actual=" + actual);
                }
            }

            Assert.AreEqual(0, errors.Count, string.Join("\n", errors));
        }

        [TestMethod]
        public void DateFormat_数値の日付()
        {
            Assert.AreEqual("令和01年5月1日", DateTimeAgent.DateFormat(20190501, DateTimeAgent.DateFormatKind.J1));
            Assert.AreEqual("H20/5/1", DateTimeAgent.DateFormat(20080501, DateTimeAgent.DateFormatKind.J2));
            Assert.AreEqual("", DateTimeAgent.DateFormat(2019050, DateTimeAgent.DateFormatKind.J1));
        }

        [TestMethod]
        public void JtoW_元号と年月日から西暦8桁()
        {
            int[] expected = { 0, 18680203, 19120203, 19260203, 19890203, 20190203, 0 };

            for (int gen = 0; gen <= 6; gen++)
            {
                Assert.AreEqual(expected[gen], DateTimeAgent.JtoW(gen, 1, 2, 3), "JtoW(gen, gy, m, d) gen=" + gen);
                Assert.AreEqual(expected[gen], DateTimeAgent.JtoW(gen, 10203), "JtoW(gen, gyymmdd) gen=" + gen);
            }

            Assert.AreEqual(20260924, DateTimeAgent.JtoW(5, 8, 9, 24));
            Assert.AreEqual(19890107, DateTimeAgent.JtoW(3, 640107));
        }
    }
}
