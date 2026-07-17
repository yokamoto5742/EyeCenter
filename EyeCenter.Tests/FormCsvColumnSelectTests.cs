using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MedicalLibrary.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// FormCsvColumnSelect の列名生成・列削除・除外設定の永続化ロジックの動作確認。
    /// すべて static private メソッドのためリフレクションで呼び出す。
    /// </summary>
    [TestClass]
    public class FormCsvColumnSelectTests
    {
        static List<string> ColumnNames(TableData data)
        {
            MethodInfo mi = typeof(FormCsvColumnSelect).GetMethod("ColumnNames",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "ColumnNames が存在すること");

            return (List<string>)mi.Invoke(null, new object[] { data });
        }

        static void RemoveExcludedColumns(TableData data, List<string> excluded)
        {
            MethodInfo mi = typeof(FormCsvColumnSelect).GetMethod("RemoveExcludedColumns",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "RemoveExcludedColumns が存在すること");

            mi.Invoke(null, new object[] { data, excluded });
        }

        static List<string> LoadExclusion(string key)
        {
            MethodInfo mi = typeof(FormCsvColumnSelect).GetMethod("LoadExclusion",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "LoadExclusion が存在すること");

            return (List<string>)mi.Invoke(null, new object[] { key });
        }

        static void SaveExclusion(string key, List<string> excluded)
        {
            MethodInfo mi = typeof(FormCsvColumnSelect).GetMethod("SaveExclusion",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "SaveExclusion が存在すること");

            mi.Invoke(null, new object[] { key, excluded });
        }

        static string ExclusionFilePath(string key)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"EyeCenter\CsvColumns_" + key + ".txt");
        }

        static TableData MakeData(string[] titles, string[] titles2)
        {
            TableData data = new TableData();

            foreach (string t in titles)
            {
                data.Title.Add(t);
            }

            foreach (string t2 in titles2)
            {
                data.Title2.Add(t2);
            }

            return data;
        }

        [TestMethod]
        public void ColumnNames_Title2があれば結合しなければTitleのみ()
        {
            TableData data = MakeData(new[] { "PATIENT_ID", "OPE_DATE" }, new[] { "", "手術日" });

            List<string> names = ColumnNames(data);

            Assert.AreEqual("PATIENT_ID", names[0]);
            Assert.AreEqual("OPE_DATE 手術日", names[1]);
        }

        [TestMethod]
        public void ColumnNames_Title2がTitleより短い場合は範囲外を無視する()
        {
            TableData data = MakeData(new[] { "A", "B", "C" }, new[] { "第一" });

            List<string> names = ColumnNames(data);

            Assert.AreEqual("A 第一", names[0]);
            Assert.AreEqual("B", names[1]);
            Assert.AreEqual("C", names[2]);
        }

        [TestMethod]
        public void RemoveExcludedColumns_除外なしなら何も変わらない()
        {
            TableData data = MakeData(new[] { "A", "B" }, new string[0]);
            data.RecordList.Add(new TableDataRecord());
            data.RecordList[0].DataList.Add("a1");
            data.RecordList[0].DataList.Add("b1");

            RemoveExcludedColumns(data, new List<string>());

            Assert.AreEqual(2, data.Title.Count);
            Assert.AreEqual(2, data.RecordList[0].DataList.Count);
        }

        [TestMethod]
        public void RemoveExcludedColumns_除外列をTitleTitle2Recordから同時に取り除く()
        {
            TableData data = MakeData(new[] { "A", "B", "C" }, new[] { "", "第二", "" });
            data.RecordList.Add(new TableDataRecord());
            data.RecordList[0].DataList.Add("a1");
            data.RecordList[0].DataList.Add("b1");
            data.RecordList[0].DataList.Add("c1");

            RemoveExcludedColumns(data, new List<string> { "B 第二" });

            CollectionAssert.AreEqual(new[] { "A", "C" }, data.Title);
            CollectionAssert.AreEqual(new[] { "", "" }, data.Title2);
            CollectionAssert.AreEqual(new[] { "a1", "c1" }, data.RecordList[0].DataList);
        }

        [TestMethod]
        public void RemoveExcludedColumns_Title2が短い列でも範囲外アクセスにならない()
        {
            TableData data = MakeData(new[] { "A", "B" }, new string[0]);
            data.RecordList.Add(new TableDataRecord());
            data.RecordList[0].DataList.Add("a1");
            data.RecordList[0].DataList.Add("b1");

            RemoveExcludedColumns(data, new List<string> { "A" });

            CollectionAssert.AreEqual(new[] { "B" }, data.Title);
            Assert.AreEqual(0, data.Title2.Count);
            CollectionAssert.AreEqual(new[] { "b1" }, data.RecordList[0].DataList);
        }

        [TestMethod]
        public void LoadExclusion_未保存ならファイルが無く空リストを返す()
        {
            string key = "UnitTest_" + Guid.NewGuid().ToString("N");

            Assert.IsFalse(File.Exists(ExclusionFilePath(key)));
            Assert.AreEqual(0, LoadExclusion(key).Count);
        }

        [TestMethod]
        public void SaveExclusionしたものをLoadExclusionで復元できる()
        {
            string key = "UnitTest_" + Guid.NewGuid().ToString("N");

            try
            {
                SaveExclusion(key, new List<string> { "COL_A", "COL_B" });

                List<string> loaded = LoadExclusion(key);

                CollectionAssert.AreEqual(new[] { "COL_A", "COL_B" }, loaded);
            }
            finally
            {
                if (File.Exists(ExclusionFilePath(key)))
                {
                    File.Delete(ExclusionFilePath(key));
                }
            }
        }
    }
}
