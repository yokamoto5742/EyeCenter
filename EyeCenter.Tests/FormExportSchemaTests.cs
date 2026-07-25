using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// FormExport のスキーマ定義出力（型表記・CSV・Markdown の整形）の動作確認。
    /// DB 照会部分は対象外で、整形メソッドのみをリフレクションで呼び出す。
    /// </summary>
    [TestClass]
    public class FormExportSchemaTests
    {
        static readonly string NL = Environment.NewLine;

        static object Invoke(string name, params object[] args)
        {
            MethodInfo mi = typeof(FormExport).GetMethod(name,
                BindingFlags.NonPublic | BindingFlags.Static);

            Assert.IsNotNull(mi, name + " が存在すること");

            return mi.Invoke(null, args);
        }

        static FormExport.SchemaColumn Column(string table, string id, string name, string type,
            string length, string precision, string scale, string nullable)
        {
            FormExport.SchemaColumn obj = new FormExport.SchemaColumn();

            obj.Owner = "MEDB";
            obj.TableName = table;
            obj.ColumnId = id;
            obj.ColumnName = name;
            obj.DataType = type;
            obj.DataLength = length;
            obj.DataPrecision = precision;
            obj.DataScale = scale;
            obj.Nullable = nullable;

            return obj;
        }

        static FormExport.SchemaKey Key(string table, string kind, string name, string column, string position)
        {
            FormExport.SchemaKey obj = new FormExport.SchemaKey();

            obj.TableName = table;
            obj.Kind = kind;
            obj.Name = name;
            obj.ColumnName = column;
            obj.Position = position;

            return obj;
        }

        [TestMethod]
        public void FormatDataType_文字型は桁数を付ける()
        {
            Assert.AreEqual("VARCHAR2(20)",
                Invoke("FormatDataType", Column("T", "1", "C", "VARCHAR2", "20", "", "", "Y")));
            Assert.AreEqual("CHAR(1)",
                Invoke("FormatDataType", Column("T", "1", "C", "CHAR", "1", "", "", "Y")));
        }

        [TestMethod]
        public void FormatDataType_数値型は精度と位取りを付ける()
        {
            Assert.AreEqual("NUMBER(10,2)",
                Invoke("FormatDataType", Column("T", "1", "C", "NUMBER", "22", "10", "2", "Y")));
        }

        [TestMethod]
        public void FormatDataType_位取りが0または未設定の数値型は精度のみ付ける()
        {
            Assert.AreEqual("NUMBER(10)",
                Invoke("FormatDataType", Column("T", "1", "C", "NUMBER", "22", "10", "0", "Y")));
            Assert.AreEqual("NUMBER(10)",
                Invoke("FormatDataType", Column("T", "1", "C", "NUMBER", "22", "10", "", "Y")));
        }

        [TestMethod]
        public void FormatDataType_桁数を持たない型はそのまま出す()
        {
            Assert.AreEqual("DATE",
                Invoke("FormatDataType", Column("T", "1", "C", "DATE", "7", "", "", "Y")));
            Assert.AreEqual("CLOB",
                Invoke("FormatDataType", Column("T", "1", "C", "CLOB", "4000", "", "", "Y")));
            Assert.AreEqual("NUMBER",
                Invoke("FormatDataType", Column("T", "1", "C", "NUMBER", "22", "", "", "Y")));
        }

        [TestMethod]
        public void FormatNullable_NのときだけNOT_NULLと表示する()
        {
            Assert.AreEqual("NOT NULL", Invoke("FormatNullable", "N"));
            Assert.AreEqual("", Invoke("FormatNullable", "Y"));
            Assert.AreEqual("", Invoke("FormatNullable", ""));
        }

        [TestMethod]
        public void MdCell_区切り文字と改行を無害化する()
        {
            Assert.AreEqual("a\\|b", Invoke("MdCell", "a|b"));
            Assert.AreEqual("a<br>b", Invoke("MdCell", "a\r\nb"));
            Assert.AreEqual("a<br>b", Invoke("MdCell", "a\nb"));
            Assert.AreEqual("a<br>b", Invoke("MdCell", "a\rb"));
        }

        [TestMethod]
        public void BuildColumnCsv_ヘッダーと型表記の列を出力する()
        {
            List<FormExport.SchemaColumn> list = new List<FormExport.SchemaColumn>();
            list.Add(Column("EYE_KENSA", "1", "PATIENT_ID", "NUMBER", "22", "10", "0", "N"));

            string csv = (string)Invoke("BuildColumnCsv", list);
            string[] lines = csv.TrimEnd('\r', '\n').Split(new string[] { NL }, StringSplitOptions.None);

            Assert.AreEqual(2, lines.Length, "ヘッダー1行＋データ1行");
            Assert.AreEqual("\"OWNER\",\"TABLE_NAME\",\"COLUMN_ID\",\"COLUMN_NAME\",\"DATA_TYPE\",\"TYPE_TEXT\"," +
                "\"DATA_LENGTH\",\"DATA_PRECISION\",\"DATA_SCALE\",\"NULLABLE\",\"DATA_DEFAULT\",\"COMMENTS\"", lines[0]);
            Assert.AreEqual("\"MEDB\",\"EYE_KENSA\",\"1\",\"PATIENT_ID\",\"NUMBER\",\"NUMBER(10)\"," +
                "\"22\",\"10\",\"0\",\"N\",\"\",\"\"", lines[1]);
        }

        [TestMethod]
        public void BuildKeyCsv_キーと索引を同じ形式で出力する()
        {
            List<FormExport.SchemaKey> list = new List<FormExport.SchemaKey>();
            list.Add(Key("EYE_OPE", "PRIMARY KEY", "PK_EYE_OPE", "ID", "1"));

            string csv = (string)Invoke("BuildKeyCsv", list);
            string[] lines = csv.TrimEnd('\r', '\n').Split(new string[] { NL }, StringSplitOptions.None);

            Assert.AreEqual("\"TABLE_NAME\",\"KIND\",\"NAME\",\"COLUMN_NAME\",\"POSITION\",\"EXTRA\"", lines[0]);
            Assert.AreEqual("\"EYE_OPE\",\"PRIMARY KEY\",\"PK_EYE_OPE\",\"ID\",\"1\",\"\"", lines[1]);
        }

        [TestMethod]
        public void BuildMarkdown_テーブルごとに見出しと列表を出力する()
        {
            List<FormExport.SchemaColumn> column_list = new List<FormExport.SchemaColumn>();
            column_list.Add(Column("EYE_KENSA", "1", "PATIENT_ID", "NUMBER", "22", "10", "0", "N"));
            column_list.Add(Column("EYE_OPE", "1", "ID", "NUMBER", "22", "10", "0", "N"));

            string md = (string)Invoke("BuildMarkdown", column_list,
                new List<FormExport.SchemaKey>(), new List<FormExport.SchemaSequence>(), "2026/07/25 10:00:00");

            StringAssert.Contains(md, "出力日時: 2026/07/25 10:00:00");
            StringAssert.Contains(md, "## EYE_KENSA");
            StringAssert.Contains(md, "## EYE_OPE");
            StringAssert.Contains(md, "| 1 | PATIENT_ID | NUMBER(10) | NOT NULL |  |  |");
            StringAssert.Contains(md, "キー・索引: なし");
        }

        [TestMethod]
        public void BuildMarkdown_複合キーは1行にまとめて列を並べる()
        {
            List<FormExport.SchemaColumn> column_list = new List<FormExport.SchemaColumn>();
            column_list.Add(Column("EYE_KENSA", "1", "PATIENT_ID", "NUMBER", "22", "10", "0", "N"));

            List<FormExport.SchemaKey> key_list = new List<FormExport.SchemaKey>();
            key_list.Add(Key("EYE_KENSA", "PRIMARY KEY", "PK_EYE_KENSA", "PATIENT_ID", "1"));
            key_list.Add(Key("EYE_KENSA", "PRIMARY KEY", "PK_EYE_KENSA", "KENSA_ID", "2"));
            key_list.Add(Key("EYE_KENSA", "PRIMARY KEY", "PK_EYE_KENSA", "KENSA_DATE", "3"));
            key_list.Add(Key("EYE_KENSA", "INDEX", "IX_EYE_KENSA_DATE", "KENSA_DATE", "1"));

            string md = (string)Invoke("BuildMarkdown", column_list, key_list,
                new List<FormExport.SchemaSequence>(), "2026/07/25 10:00:00");

            StringAssert.Contains(md, "| PRIMARY KEY | PK_EYE_KENSA | PATIENT_ID, KENSA_ID, KENSA_DATE |  |");
            StringAssert.Contains(md, "| INDEX | IX_EYE_KENSA_DATE | KENSA_DATE |  |");
        }

        [TestMethod]
        public void BuildMarkdown_他テーブルのキーを混入させない()
        {
            List<FormExport.SchemaColumn> column_list = new List<FormExport.SchemaColumn>();
            column_list.Add(Column("EYE_KENSA", "1", "PATIENT_ID", "NUMBER", "22", "10", "0", "N"));
            column_list.Add(Column("EYE_OPE", "1", "ID", "NUMBER", "22", "10", "0", "N"));

            List<FormExport.SchemaKey> key_list = new List<FormExport.SchemaKey>();
            key_list.Add(Key("EYE_KENSA", "PRIMARY KEY", "PK_EYE_KENSA", "PATIENT_ID", "1"));
            key_list.Add(Key("EYE_OPE", "PRIMARY KEY", "PK_EYE_OPE", "ID", "1"));

            string md = (string)Invoke("BuildMarkdown", column_list, key_list,
                new List<FormExport.SchemaSequence>(), "2026/07/25 10:00:00");

            int kensa_index = md.IndexOf("## EYE_KENSA");
            int ope_index = md.IndexOf("## EYE_OPE");

            string kensa_section = md.Substring(kensa_index, ope_index - kensa_index);

            StringAssert.Contains(kensa_section, "PK_EYE_KENSA");
            Assert.IsFalse(kensa_section.Contains("PK_EYE_OPE"), "EYE_KENSA の節に EYE_OPE のキーが混ざらないこと");
        }

        [TestMethod]
        public void BuildMarkdown_シーケンスの節を出力する()
        {
            FormExport.SchemaSequence seq = new FormExport.SchemaSequence();
            seq.Owner = "MEDB";
            seq.Name = "EYE_OPE_SEQ";
            seq.MinValue = "1";
            seq.MaxValue = "9999999999";
            seq.IncrementBy = "1";
            seq.LastNumber = "1234";
            seq.CacheSize = "20";

            List<FormExport.SchemaSequence> sequence_list = new List<FormExport.SchemaSequence>();
            sequence_list.Add(seq);

            string md = (string)Invoke("BuildMarkdown", new List<FormExport.SchemaColumn>(),
                new List<FormExport.SchemaKey>(), sequence_list, "2026/07/25 10:00:00");

            StringAssert.Contains(md, "## シーケンス");
            StringAssert.Contains(md, "| MEDB | EYE_OPE_SEQ | 1 | 9999999999 | 1 | 1234 | 20 |");
        }

        [TestMethod]
        public void BuildMarkdown_シーケンスが無い場合はなしと出力する()
        {
            string md = (string)Invoke("BuildMarkdown", new List<FormExport.SchemaColumn>(),
                new List<FormExport.SchemaKey>(), new List<FormExport.SchemaSequence>(), "2026/07/25 10:00:00");

            StringAssert.Contains(md, "## シーケンス" + NL + NL + "なし");
        }
    }
}
