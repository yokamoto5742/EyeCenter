using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MedicalLibrary.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// FormSumColumnSelect の ParseCont / FixedValue（サマリーCONTのKey-Value解析）の動作確認。
    /// どちらも static private メソッドのためリフレクションで呼び出す。
    /// </summary>
    [TestClass]
    public class FormSumColumnSelectTests
    {
        static Dictionary<string, string> ParseCont(string cont)
        {
            MethodInfo mi = typeof(FormSumColumnSelect).GetMethod("ParseCont",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "ParseCont が存在すること");

            return (Dictionary<string, string>)mi.Invoke(null, new object[] { cont });
        }

        static string FixedValue(EyeSummary sum, string code)
        {
            MethodInfo mi = typeof(FormSumColumnSelect).GetMethod("FixedValue",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "FixedValue が存在すること");

            return (string)mi.Invoke(null, new object[] { sum, code });
        }

        [TestMethod]
        public void ParseCont_カンマ区切りのKeyValueを辞書化する()
        {
            Dictionary<string, string> dict = ParseCont("A001,値1\rA002,値2");

            Assert.AreEqual("値1", dict["A001"]);
            Assert.AreEqual("値2", dict["A002"]);
        }

        [TestMethod]
        public void ParseCont_重複Keyは先勝ちで後の行は無視される()
        {
            Dictionary<string, string> dict = ParseCont("A001,先勝ち\rA001,後勝ちしない");

            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("先勝ち", dict["A001"]);
        }

        [TestMethod]
        public void ParseCont_カンマを含まない行は無視される()
        {
            Dictionary<string, string> dict = ParseCont("ノーカンマ行\rA002,値2");

            Assert.AreEqual(1, dict.Count);
            Assert.IsTrue(dict.ContainsKey("A002"));
        }

        [TestMethod]
        public void ParseCont_CRLFトークンは改行に復元される()
        {
            Dictionary<string, string> dict = ParseCont("A001,1行目<CR+LF>2行目");

            Assert.AreEqual("1行目\r\n2行目", dict["A001"]);
        }

        [TestMethod]
        public void ParseCont_空文字列は空の辞書になる()
        {
            Dictionary<string, string> dict = ParseCont("");

            Assert.AreEqual(0, dict.Count);
        }

        [TestMethod]
        public void FixedValue_コードに対応するEyeSummaryのプロパティを返す()
        {
            EyeSummary sum = new EyeSummary();
            sum.Diag = "白内障";
            sum.Kind1 = "分類1";
            sum.Kind2 = "分類2";
            sum.Kind3 = "分類3";
            sum.Plan = "経過観察";
            sum.Pass = "経過";
            sum.Hist = "既往歴";

            Assert.AreEqual("白内障", FixedValue(sum, "DIAG"));
            Assert.AreEqual("分類1", FixedValue(sum, "KIND1"));
            Assert.AreEqual("分類2", FixedValue(sum, "KIND2"));
            Assert.AreEqual("分類3", FixedValue(sum, "KIND3"));
            Assert.AreEqual("経過観察", FixedValue(sum, "PLAN"));
            Assert.AreEqual("経過", FixedValue(sum, "PASS"));
            Assert.AreEqual("既往歴", FixedValue(sum, "HIST"));
        }

        [TestMethod]
        public void FixedValue_未知のコードは空文字を返す()
        {
            EyeSummary sum = new EyeSummary();
            sum.Diag = "白内障";

            Assert.AreEqual("", FixedValue(sum, "UNKNOWN"));
        }

        static List<string> LoadSelection()
        {
            MethodInfo mi = typeof(FormSumColumnSelect).GetMethod("LoadSelection",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "LoadSelection が存在すること");

            return (List<string>)mi.Invoke(null, null);
        }

        static void SaveSelection(List<string> keys)
        {
            Type sumColumnType = typeof(FormSumColumnSelect).GetNestedType("SumColumn", BindingFlags.NonPublic);
            Assert.IsNotNull(sumColumnType, "SumColumn ネスト型が存在すること");

            System.Collections.IList selected = (System.Collections.IList)Activator.CreateInstance(
                typeof(List<>).MakeGenericType(sumColumnType));

            FieldInfo keyField = sumColumnType.GetField("Key", BindingFlags.Public | BindingFlags.Instance);
            Assert.IsNotNull(keyField, "SumColumn.Key が存在すること");

            foreach (string key in keys)
            {
                object col = Activator.CreateInstance(sumColumnType);
                keyField.SetValue(col, key);
                selected.Add(col);
            }

            MethodInfo mi = typeof(FormSumColumnSelect).GetMethod("SaveSelection",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "SaveSelection が存在すること");

            mi.Invoke(null, new object[] { selected });
        }

        static string SelectionFilePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"EyeCenter\SumJoinColumns.txt");
        }

        [TestMethod]
        public void SaveSelectionしたものをLoadSelectionで復元できる()
        {
            List<string> original = LoadSelection();

            try
            {
                SaveSelection(new List<string> { "1_A001", "3_B001" });

                List<string> loaded = LoadSelection();

                CollectionAssert.AreEqual(new[] { "1_A001", "3_B001" }, loaded);
            }
            finally
            {
                // 他テスト・実運用の選択状態を壊さないよう元に戻す
                SaveSelection(original);
            }
        }
    }
}
