using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// FormExport の CSV 整形ロジック（CsvCell / WriteCsvLine）の動作確認。
    /// どちらも static private メソッドのためインスタンス生成せずリフレクションで呼び出す。
    /// </summary>
    [TestClass]
    public class FormExportTests
    {
        static string CsvCell(string value)
        {
            MethodInfo mi = typeof(FormExport).GetMethod("CsvCell",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "CsvCell が存在すること");

            return (string)mi.Invoke(null, new object[] { value });
        }

        static string WriteCsvLine(List<string> cells)
        {
            MethodInfo mi = typeof(FormExport).GetMethod("WriteCsvLine",
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, "WriteCsvLine が存在すること");

            // WriteCsvLine は System.IO.StreamWriter を要求するため、MemoryStream 経由で結果を読み出す
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(ms, new System.Text.UTF8Encoding(false)))
                {
                    writer.NewLine = "\n";
                    mi.Invoke(null, new object[] { writer, cells });
                    writer.Flush();

                    ms.Position = 0;
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(ms, System.Text.Encoding.UTF8))
                    {
                        return reader.ReadToEnd().TrimEnd('\n');
                    }
                }
            }
        }

        [TestMethod]
        public void CsvCell_通常文字列は引用符で囲む()
        {
            Assert.AreEqual("\"abc\"", CsvCell("abc"));
        }

        [TestMethod]
        public void CsvCell_ダブルクォートは二重化する()
        {
            Assert.AreEqual("\"a\"\"b\"", CsvCell("a\"b"));
        }

        [TestMethod]
        public void CsvCell_カンマを含んでも引用符で保護される()
        {
            Assert.AreEqual("\"a,b\"", CsvCell("a,b"));
        }

        [TestMethod]
        public void CsvCell_改行はすべてCRLFトークンに変換する()
        {
            Assert.AreEqual("\"a<CR+LF>b\"", CsvCell("a\r\nb"), "\\r\\n");
            Assert.AreEqual("\"a<CR+LF>b\"", CsvCell("a\rb"), "\\r 単独");
            Assert.AreEqual("\"a<CR+LF>b\"", CsvCell("a\nb"), "\\n 単独");
        }

        [TestMethod]
        public void CsvCell_CRLFトークン文字列自体を含む場合は往復不能な既知制約がある()
        {
            // "<CR+LF>" という文字列と実際の改行復元後の結果が区別できなくなる既知の制約
            Assert.AreEqual("\"<CR+LF>\"", CsvCell("<CR+LF>"));
            Assert.AreEqual("\"<CR+LF>\"", CsvCell("\r\n"));
        }

        [TestMethod]
        public void CsvCell_空文字列は空の引用符になる()
        {
            Assert.AreEqual("\"\"", CsvCell(""));
        }

        [TestMethod]
        public void WriteCsvLine_複数セルをカンマ区切りで引用符囲みし末尾カンマは付かない()
        {
            string line = WriteCsvLine(new List<string> { "a", "b,c", "d\"e" });

            Assert.AreEqual("\"a\",\"b,c\",\"d\"\"e\"", line);
        }

        [TestMethod]
        public void WriteCsvLine_空セルのみの行も出力できる()
        {
            string line = WriteCsvLine(new List<string> { "", "" });

            Assert.AreEqual("\"\",\"\"", line);
        }
    }
}
