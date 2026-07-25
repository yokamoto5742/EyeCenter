using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Entity;
using MedicalLibrary.Utility;

namespace EyeCenter
{
    /// <summary>
    /// スキーマ定義の出力。
    /// アプリが読み書きする EYE_* 系テーブルの定義を Oracle のデータディクショナリから取得し、
    /// CSV（機械取込用）と Markdown（定義書）で書き出す。
    /// </summary>
    public partial class FormExport
    {
        /// <summary>スキーマ定義の出力対象テーブル</summary>
        static readonly List<string> SchemaTableList = new List<string>()
        {
            "EYE_KENSA",
            "EYE_KENSA2",
            "EYE_INTERVIEW",
            "EYE_OPE",
            "EYE_OPE_RECORD",
            "EYE_OPE_DOCTOR",
            "EYE_OPE_PASS",
            "EYE_OPE_RSV",
            "EYE_SUMMARY",
        };

        /// <summary>採番に使用しているシーケンス</summary>
        static readonly List<string> SchemaSequenceList = new List<string>()
        {
            "EYE_OPE_SEQ",
            "EYE_INTERVIEW_SEQ",
        };

        /// <summary>列定義１行分</summary>
        public class SchemaColumn
        {
            public string Owner = "";
            public string TableName = "";
            public string ColumnId = "";
            public string ColumnName = "";
            public string DataType = "";
            public string DataLength = "";
            public string DataPrecision = "";
            public string DataScale = "";
            public string Nullable = "";
            public string DataDefault = "";
            public string Comments = "";
        }

        /// <summary>キー・索引１行分（主キー／一意キー／外部キー／索引を同じ形で保持する）</summary>
        public class SchemaKey
        {
            public string TableName = "";

            /// <summary>PRIMARY KEY / UNIQUE / FOREIGN KEY / INDEX</summary>
            public string Kind = "";

            public string Name = "";
            public string ColumnName = "";
            public string Position = "";

            /// <summary>外部キーの参照先、索引の一意性など</summary>
            public string Extra = "";
        }

        /// <summary>シーケンス１件分</summary>
        public class SchemaSequence
        {
            public string Owner = "";
            public string Name = "";
            public string MinValue = "";
            public string MaxValue = "";
            public string IncrementBy = "";
            public string LastNumber = "";
            public string CacheSize = "";
        }

        /// <summary>
        /// 名前のリストを SQL の IN 句の中身（'A','B'）に変換する。
        /// 対象はコード内の定数のみのため文字列連結でよい。
        /// </summary>
        static string InClause(List<string> name_list)
        {
            List<string> quoted = new List<string>();

            for (int i = 0; i < name_list.Count; i++)
            {
                quoted.Add("'" + name_list[i] + "'");
            }

            return string.Join(",", quoted);
        }

        /// <summary>
        /// スキーマ照会を専用接続で実行する。
        /// 共有接続（DB.Db2）の OracleCommand に LONG 取得設定を残さないため、
        /// SearchTask と同じく接続文字列だけを流用した使い捨ての接続を使う。
        /// </summary>
        static List<StdClass> SchemaQuery(string cmd)
        {
            DB db = new DB();
            db.Init(DB.Db2.InitString);

            // ALL_TAB_COLUMNS.DATA_DEFAULT は LONG 型。既定（0）のままだと内容が取得できない
            db.Command.InitialLONGFetchSize = -1;

            try
            {
                return StdClass.GetList(db, cmd);
            }
            finally
            {
                try { db.Connection.Dispose(); } catch (Exception) { }
            }
        }

        /// <summary>
        /// 列定義を取得する。
        /// DATA_DEFAULT（LONG）が取得できない環境では、既定値を空欄にして続行する。
        /// </summary>
        static List<SchemaColumn> LoadSchemaColumns()
        {
            List<StdClass> row_list;

            try
            {
                row_list = SchemaQuery(SchemaColumnSql("t.DATA_DEFAULT"));
            }
            catch (Exception)
            {
                row_list = SchemaQuery(SchemaColumnSql("'' DATA_DEFAULT"));
            }

            List<SchemaColumn> list = new List<SchemaColumn>();

            foreach (StdClass tmp in row_list)
            {
                SchemaColumn obj = new SchemaColumn();

                obj.Owner = tmp.GetDataString("OWNER");
                obj.TableName = tmp.GetDataString("TABLE_NAME");
                obj.ColumnId = tmp.GetDataString("COLUMN_ID");
                obj.ColumnName = tmp.GetDataString("COLUMN_NAME");
                obj.DataType = tmp.GetDataString("DATA_TYPE");
                obj.DataLength = tmp.GetDataString("DATA_LENGTH");
                obj.DataPrecision = tmp.GetDataString("DATA_PRECISION");
                obj.DataScale = tmp.GetDataString("DATA_SCALE");
                obj.Nullable = tmp.GetDataString("NULLABLE");
                obj.DataDefault = tmp.GetDataString("DATA_DEFAULT").Trim();
                obj.Comments = tmp.GetDataString("COMMENTS");

                list.Add(obj);
            }

            return list;
        }

        static string SchemaColumnSql(string data_default_expr)
        {
            return "select t.OWNER, t.TABLE_NAME, t.COLUMN_ID, t.COLUMN_NAME, t.DATA_TYPE," +
                " t.DATA_LENGTH, t.DATA_PRECISION, t.DATA_SCALE, t.NULLABLE," +
                " " + data_default_expr + ", c.COMMENTS" +
                " from ALL_TAB_COLUMNS t" +
                " left join ALL_COL_COMMENTS c" +
                " on c.OWNER = t.OWNER and c.TABLE_NAME = t.TABLE_NAME and c.COLUMN_NAME = t.COLUMN_NAME" +
                " where t.TABLE_NAME in (" + InClause(SchemaTableList) + ")" +
                " order by t.OWNER, t.TABLE_NAME, t.COLUMN_ID";
        }

        /// <summary>
        /// 制約と索引を取得する。
        /// チェック制約（CONSTRAINT_TYPE='C'）は SEARCH_CONDITION が LONG 型で、
        /// Oracle 11.2 には代替の SEARCH_CONDITION_VC が無いため対象外とする。
        /// </summary>
        static List<SchemaKey> LoadSchemaKeys()
        {
            List<SchemaKey> list = new List<SchemaKey>();

            string in_clause = InClause(SchemaTableList);

            string cons_cmd = "select c.TABLE_NAME, c.CONSTRAINT_TYPE, c.CONSTRAINT_NAME," +
                " cc.COLUMN_NAME, cc.POSITION, c.R_OWNER, c.R_CONSTRAINT_NAME" +
                " from ALL_CONSTRAINTS c" +
                " inner join ALL_CONS_COLUMNS cc on cc.OWNER = c.OWNER and cc.CONSTRAINT_NAME = c.CONSTRAINT_NAME" +
                " where c.TABLE_NAME in (" + in_clause + ") and c.CONSTRAINT_TYPE in ('P','U','R')" +
                " order by c.TABLE_NAME, c.CONSTRAINT_TYPE, c.CONSTRAINT_NAME, cc.POSITION";

            foreach (StdClass tmp in SchemaQuery(cons_cmd))
            {
                SchemaKey obj = new SchemaKey();

                obj.TableName = tmp.GetDataString("TABLE_NAME");
                obj.Kind = ConstraintKind(tmp.GetDataString("CONSTRAINT_TYPE"));
                obj.Name = tmp.GetDataString("CONSTRAINT_NAME");
                obj.ColumnName = tmp.GetDataString("COLUMN_NAME");
                obj.Position = tmp.GetDataString("POSITION");

                if (tmp.GetDataString("R_CONSTRAINT_NAME").Length > 0)
                {
                    obj.Extra = "参照先制約 " + tmp.GetDataString("R_OWNER") + "." + tmp.GetDataString("R_CONSTRAINT_NAME");
                }

                list.Add(obj);
            }

            string index_cmd = "select i.TABLE_NAME, i.INDEX_NAME, i.UNIQUENESS," +
                " ic.COLUMN_NAME, ic.COLUMN_POSITION" +
                " from ALL_INDEXES i" +
                " inner join ALL_IND_COLUMNS ic on ic.INDEX_OWNER = i.OWNER and ic.INDEX_NAME = i.INDEX_NAME" +
                " where i.TABLE_NAME in (" + in_clause + ")" +
                " order by i.TABLE_NAME, i.INDEX_NAME, ic.COLUMN_POSITION";

            foreach (StdClass tmp in SchemaQuery(index_cmd))
            {
                SchemaKey obj = new SchemaKey();

                obj.TableName = tmp.GetDataString("TABLE_NAME");
                obj.Kind = "INDEX";
                obj.Name = tmp.GetDataString("INDEX_NAME");
                obj.ColumnName = tmp.GetDataString("COLUMN_NAME");
                obj.Position = tmp.GetDataString("COLUMN_POSITION");
                obj.Extra = tmp.GetDataString("UNIQUENESS");

                list.Add(obj);
            }

            return list;
        }

        static string ConstraintKind(string constraint_type)
        {
            if (constraint_type.Equals("P"))
            {
                return "PRIMARY KEY";
            }

            if (constraint_type.Equals("U"))
            {
                return "UNIQUE";
            }

            if (constraint_type.Equals("R"))
            {
                return "FOREIGN KEY";
            }

            return constraint_type;
        }

        static List<SchemaSequence> LoadSchemaSequences()
        {
            string cmd = "select SEQUENCE_OWNER, SEQUENCE_NAME, MIN_VALUE, MAX_VALUE, INCREMENT_BY, LAST_NUMBER, CACHE_SIZE" +
                " from ALL_SEQUENCES" +
                " where SEQUENCE_NAME in (" + InClause(SchemaSequenceList) + ")" +
                " order by SEQUENCE_NAME";

            List<SchemaSequence> list = new List<SchemaSequence>();

            foreach (StdClass tmp in SchemaQuery(cmd))
            {
                SchemaSequence obj = new SchemaSequence();

                obj.Owner = tmp.GetDataString("SEQUENCE_OWNER");
                obj.Name = tmp.GetDataString("SEQUENCE_NAME");
                obj.MinValue = tmp.GetDataString("MIN_VALUE");
                obj.MaxValue = tmp.GetDataString("MAX_VALUE");
                obj.IncrementBy = tmp.GetDataString("INCREMENT_BY");
                obj.LastNumber = tmp.GetDataString("LAST_NUMBER");
                obj.CacheSize = tmp.GetDataString("CACHE_SIZE");

                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        /// 桁数・精度を含めた型表記（VARCHAR2(20) / NUMBER(10,2) など）を組み立てる。
        /// </summary>
        static string FormatDataType(SchemaColumn obj)
        {
            if (obj.DataPrecision.Length > 0)
            {
                if (obj.DataScale.Length == 0 || obj.DataScale.Equals("0"))
                {
                    return obj.DataType + "(" + obj.DataPrecision + ")";
                }

                return obj.DataType + "(" + obj.DataPrecision + "," + obj.DataScale + ")";
            }

            if (obj.DataType.Equals("CHAR") || obj.DataType.Equals("VARCHAR2") ||
                obj.DataType.Equals("NCHAR") || obj.DataType.Equals("NVARCHAR2") ||
                obj.DataType.Equals("RAW"))
            {
                return obj.DataType + "(" + obj.DataLength + ")";
            }

            return obj.DataType;
        }

        static string FormatNullable(string nullable)
        {
            if (nullable.Equals("N"))
            {
                return "NOT NULL";
            }

            return "";
        }

        static string BuildColumnCsv(List<SchemaColumn> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Join(",", new string[] {
                CsvCell("OWNER"), CsvCell("TABLE_NAME"), CsvCell("COLUMN_ID"), CsvCell("COLUMN_NAME"),
                CsvCell("DATA_TYPE"), CsvCell("TYPE_TEXT"), CsvCell("DATA_LENGTH"), CsvCell("DATA_PRECISION"),
                CsvCell("DATA_SCALE"), CsvCell("NULLABLE"), CsvCell("DATA_DEFAULT"), CsvCell("COMMENTS") }));

            foreach (SchemaColumn obj in list)
            {
                sb.AppendLine(string.Join(",", new string[] {
                    CsvCell(obj.Owner), CsvCell(obj.TableName), CsvCell(obj.ColumnId), CsvCell(obj.ColumnName),
                    CsvCell(obj.DataType), CsvCell(FormatDataType(obj)), CsvCell(obj.DataLength), CsvCell(obj.DataPrecision),
                    CsvCell(obj.DataScale), CsvCell(obj.Nullable), CsvCell(obj.DataDefault), CsvCell(obj.Comments) }));
            }

            return sb.ToString();
        }

        static string BuildKeyCsv(List<SchemaKey> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Join(",", new string[] {
                CsvCell("TABLE_NAME"), CsvCell("KIND"), CsvCell("NAME"),
                CsvCell("COLUMN_NAME"), CsvCell("POSITION"), CsvCell("EXTRA") }));

            foreach (SchemaKey obj in list)
            {
                sb.AppendLine(string.Join(",", new string[] {
                    CsvCell(obj.TableName), CsvCell(obj.Kind), CsvCell(obj.Name),
                    CsvCell(obj.ColumnName), CsvCell(obj.Position), CsvCell(obj.Extra) }));
            }

            return sb.ToString();
        }

        static string BuildSequenceCsv(List<SchemaSequence> list)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Join(",", new string[] {
                CsvCell("OWNER"), CsvCell("SEQUENCE_NAME"), CsvCell("MIN_VALUE"), CsvCell("MAX_VALUE"),
                CsvCell("INCREMENT_BY"), CsvCell("LAST_NUMBER"), CsvCell("CACHE_SIZE") }));

            foreach (SchemaSequence obj in list)
            {
                sb.AppendLine(string.Join(",", new string[] {
                    CsvCell(obj.Owner), CsvCell(obj.Name), CsvCell(obj.MinValue), CsvCell(obj.MaxValue),
                    CsvCell(obj.IncrementBy), CsvCell(obj.LastNumber), CsvCell(obj.CacheSize) }));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Markdown の表セル用に整形する（区切り文字と改行を無害化する）。
        /// </summary>
        static string MdCell(string value)
        {
            return value.Replace("|", "\\|").Replace("\r\n", "<br>").Replace("\r", "<br>").Replace("\n", "<br>");
        }

        /// <summary>
        /// 列定義・キー・シーケンスをまとめた Markdown の定義書を組み立てる。
        /// </summary>
        static string BuildMarkdown(List<SchemaColumn> column_list, List<SchemaKey> key_list,
            List<SchemaSequence> sequence_list, string timestamp)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("# EyeData データベーススキーマ定義");
            sb.AppendLine();
            sb.AppendLine("出力日時: " + timestamp);
            sb.AppendLine();
            sb.AppendLine("対象: 本アプリケーションが読み書きする EYE_* 系テーブル（" + SchemaTableList.Count.ToString() + "テーブル）");
            sb.AppendLine();
            sb.AppendLine("> チェック制約は Oracle 11.2 のデータディクショナリから取得できないため含まれない。");
            sb.AppendLine();

            string last_table = "";

            foreach (SchemaColumn obj in column_list)
            {
                if (!obj.TableName.Equals(last_table))
                {
                    if (last_table.Length > 0)
                    {
                        AppendMarkdownKeys(sb, key_list, last_table);
                    }

                    last_table = obj.TableName;

                    sb.AppendLine("## " + obj.TableName);
                    sb.AppendLine();
                    sb.AppendLine("所有者: " + obj.Owner);
                    sb.AppendLine();
                    sb.AppendLine("| # | 列名 | 型 | NULL | 既定値 | コメント |");
                    sb.AppendLine("|---|---|---|---|---|---|");
                }

                sb.AppendLine("| " + MdCell(obj.ColumnId) +
                    " | " + MdCell(obj.ColumnName) +
                    " | " + MdCell(FormatDataType(obj)) +
                    " | " + MdCell(FormatNullable(obj.Nullable)) +
                    " | " + MdCell(obj.DataDefault) +
                    " | " + MdCell(obj.Comments) + " |");
            }

            if (last_table.Length > 0)
            {
                AppendMarkdownKeys(sb, key_list, last_table);
            }

            AppendMarkdownSequences(sb, sequence_list);

            return sb.ToString();
        }

        /// <summary>
        /// １テーブル分のキー・索引を、同一の種別＋名称ごとに列をまとめて出力する。
        /// key_list は種別・名称・列位置の順に整列している前提。
        /// </summary>
        static void AppendMarkdownKeys(StringBuilder sb, List<SchemaKey> key_list, string table_name)
        {
            List<string> line_list = new List<string>();

            string last_kind = "";
            string last_name = "";
            string last_extra = "";
            List<string> column_name_list = new List<string>();

            foreach (SchemaKey obj in key_list)
            {
                if (!obj.TableName.Equals(table_name))
                {
                    continue;
                }

                if (!obj.Kind.Equals(last_kind) || !obj.Name.Equals(last_name))
                {
                    if (column_name_list.Count > 0)
                    {
                        line_list.Add("| " + MdCell(last_kind) + " | " + MdCell(last_name) +
                            " | " + MdCell(string.Join(", ", column_name_list)) + " | " + MdCell(last_extra) + " |");
                    }

                    last_kind = obj.Kind;
                    last_name = obj.Name;
                    last_extra = obj.Extra;
                    column_name_list = new List<string>();
                }

                column_name_list.Add(obj.ColumnName);
            }

            if (column_name_list.Count > 0)
            {
                line_list.Add("| " + MdCell(last_kind) + " | " + MdCell(last_name) +
                    " | " + MdCell(string.Join(", ", column_name_list)) + " | " + MdCell(last_extra) + " |");
            }

            sb.AppendLine();

            if (line_list.Count == 0)
            {
                sb.AppendLine("キー・索引: なし");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("| 種別 | 名称 | 列 | 備考 |");
            sb.AppendLine("|---|---|---|---|");

            foreach (string line in line_list)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine();
        }

        static void AppendMarkdownSequences(StringBuilder sb, List<SchemaSequence> sequence_list)
        {
            sb.AppendLine("## シーケンス");
            sb.AppendLine();

            if (sequence_list.Count == 0)
            {
                sb.AppendLine("なし");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("| 所有者 | 名称 | 最小 | 最大 | 増分 | 次の採番 | キャッシュ |");
            sb.AppendLine("|---|---|---|---|---|---|---|");

            foreach (SchemaSequence obj in sequence_list)
            {
                sb.AppendLine("| " + MdCell(obj.Owner) +
                    " | " + MdCell(obj.Name) +
                    " | " + MdCell(obj.MinValue) +
                    " | " + MdCell(obj.MaxValue) +
                    " | " + MdCell(obj.IncrementBy) +
                    " | " + MdCell(obj.LastNumber) +
                    " | " + MdCell(obj.CacheSize) + " |");
            }

            sb.AppendLine();
        }

        /// <summary>
        /// スキーマ定義を書き出す。指定されたファイル名を基点に
        /// 列定義CSV・キー索引CSV・シーケンスCSV・Markdown定義書・マニフェストを出力する。
        /// </summary>
        void SaveSchema(string file_name)
        {
            string title = this.Text;

            try
            {
                this.Cursor = Cursors.WaitCursor;
                ExeButton.Enabled = false;
                CloseButton.Enabled = false;

                List<SchemaColumn> column_list = LoadSchemaColumns();
                List<SchemaKey> key_list = LoadSchemaKeys();
                List<SchemaSequence> sequence_list = LoadSchemaSequences();

                Encoding encoding = this.GetSelectedEncoding();

                System.IO.File.WriteAllText(file_name, BuildColumnCsv(column_list), encoding);
                System.IO.File.WriteAllText(file_name + ".keys.csv", BuildKeyCsv(key_list), encoding);
                System.IO.File.WriteAllText(file_name + ".sequences.csv", BuildSequenceCsv(sequence_list), encoding);
                System.IO.File.WriteAllText(file_name + ".md",
                    BuildMarkdown(column_list, key_list, sequence_list, DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")), encoding);

                this.WriteManifest(file_name, "スキーマ定義（EYE_*）", column_list.Count);

                MessageBox.Show("スキーマ定義を出力しました（" + column_list.Count.ToString("#,0") + "列）");
            }
            catch (Exception ex)
            {
                string err = ex.Message;
                MessageBox.Show(err);
            }
            finally
            {
                if (!this.IsDisposed)
                {
                    this.Text = title;
                    ExeButton.Enabled = true;
                    CloseButton.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }
    }
}
