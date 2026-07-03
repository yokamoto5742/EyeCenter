using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using MedicalLibrary.Agent;

namespace EyeCenter
{
    /// <summary>
    /// オペ録・眼科申し送り書のExcelテンプレートを開き、共通情報シートへの書き込み・
    /// バーコード画像の生成と挿入・別名保存を行う。（EyeAgree の ExcelControl を移植）
    /// </summary>
    class ExcelControl
    {
        Excel.Application exApp;
        Excel._Workbook exWorkbook;
        Excel._Worksheet exWorksheet;

        /// <summary>
        /// 共通情報シート名。バーコード画像の挿入対象から除外する。
        /// </summary>
        const string CommonSheetName = "共通情報";

        /// <summary>
        /// 診療科コード・診療科名（眼科固定）。B5・B6 とバーコード値に使用する。
        /// </summary>
        const string DeptCode = "007";
        const string DeptName = "眼科";

        // バーコード画像の解像度設定。既定値は EyeDataSettings.ini の
        // [BARCODE_SETTINGS] で上書きできる。1モジュール=barcodeLineWidth px。
        float barcodeLineWidth = 3f;

        float barcodeHeight = 80f;

        int barcodeQuietModules = 10;

        // バーコードに埋め込む文書コード（EyeDataSettings.ini で上書きできる）
        string operationRecordCode = "39911";       // オペ録

        string surgicalNursingRecordCode = "34411"; // 眼科申し送り書

        // バーコード画像を挿入するシート名（カンマ区切り）。空なら共通情報以外の全シート。
        string operationRecordSheets = "";

        string surgicalNursingRecordSheets = "";

        /// <summary>
        /// テンプレートを開いて共通情報シートに書き込み、バーコード画像を挿入して
        /// TEMP へ「患者ID_日時_テンプレート名」の別名で保存する。
        /// オペ録・眼科申し送り書以外のテンプレートは従来どおり EyeDoc.ExcelOpen に委ねる。
        /// 呼び出し側で例外を処理し、finally で ReleaseExcel を呼ぶこと。
        /// </summary>
        public void MakeDocument(EyeDoc doc)
        {
            loadSettings();

            string documentCode;
            string targetSheets;
            string templateName = Path.GetFileNameWithoutExtension(doc.FileName);

            if (templateName.Equals("オペ録"))
            {
                documentCode = operationRecordCode;
                targetSheets = operationRecordSheets;
            }
            else if (templateName.Equals("眼科申し送り書"))
            {
                documentCode = surgicalNursingRecordCode;
                targetSheets = surgicalNursingRecordSheets;
            }
            else
            {
                // バーコード対象外のテンプレート（同意書など）は従来の処理のまま開く
                doc.ExcelOpen();
                return;
            }

            if (!File.Exists(doc.FileName))
            {
                return;
            }

            exApp = new Excel.Application();
            exApp.Visible = true;

            // 自動処理中はテンプレートのマクロを起動させない
            exApp.EnableEvents = false;

            exWorkbook = (Excel._Workbook)(exApp.Workbooks.Open(doc.FileName,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value));

            exWorksheet = (Excel._Worksheet)(exWorkbook.Sheets[CommonSheetName]);

            // セル書込み・シート切替・バーコード挿入の途中経過を画面に見せない
            exApp.ScreenUpdating = false;

            // 処理後に元へ戻すため、開いた時点のアクティブシート名を控える
            Excel._Worksheet startSheet = (Excel._Worksheet)(exWorkbook.ActiveSheet);
            string startSheetName = startSheet.Name;
            Marshal.ReleaseComObject(startSheet);

            // 保存時に「セッション中アクティブにされていないシートのフォームコントロール
            // （ボタン）が脱落する」既知の挙動を回避するため、全シートを一度アクティブ化する。
            // EnableEvents=false 中なのでイベントは発火しない。
            Excel.Sheets sheets = exWorkbook.Sheets;
            int sheetCount = sheets.Count;

            for (int i = 1; i <= sheetCount; i++)
            {
                Excel._Worksheet ws = (Excel._Worksheet)(sheets[i]);

                try
                {
                    ws.Activate();
                }
                catch
                {
                    // 非表示シートはアクティブ化できないためスキップ
                }

                Marshal.ReleaseComObject(ws);
            }

            // 共通情報シート B1〜B10
            exWorksheet.Cells[1, 2] = doc.PtId;
            exWorksheet.Cells[2, 2] = doc.Pat.Kana;
            exWorksheet.Cells[3, 2] = doc.Pat.Name;
            exWorksheet.Cells[4, 2] = doc.Pat.SexNameShort;
            exWorksheet.Cells[5, 2] = DeptCode;
            exWorksheet.Cells[6, 2] = DeptName;
            exWorksheet.Cells[7, 2] = doc.UserId.PadLeft(5, '0');
            exWorksheet.Cells[8, 2] = doc.SaveDate;
            exWorksheet.Cells[9, 2] = doc.SaveTime;

            // 36桁バーコード値（患者ID9桁 + 文書コード5桁 + 診療科3桁 + 入力者ID5桁 + 作成日8桁 + 作成時刻6桁）
            string barcodeValue = doc.PtId.PadLeft(9, '0') + documentCode.PadLeft(5, '0') + DeptCode
                + doc.UserId.PadLeft(5, '0') + doc.SaveDate + doc.SaveTime;

            exWorksheet.Cells[10, 2] = barcodeValue;

            // 27行目以降の各リスト（従来の EyeDoc.ExcelOpen と同じ列配置）
            int row = 27;

            foreach (EyeDoc.Item item in doc.ItemList)
            {
                exWorksheet.Cells[row, 1] = item.Kind;
                exWorksheet.Cells[row, 2] = item.Name;
                exWorksheet.Cells[row, 3] = item.Value;

                row++;
            }

            row = 27;

            foreach (EyeDoc.Item item in doc.ContactList)
            {
                exWorksheet.Cells[row, 5] = item.Kind;
                exWorksheet.Cells[row, 6] = item.Name;
                exWorksheet.Cells[row, 7] = item.Value;

                row++;
            }

            row = 27;

            foreach (EyeDoc.Item item in doc.PatInfoList)
            {
                exWorksheet.Cells[row, 9] = item.Kind;
                exWorksheet.Cells[row, 10] = item.Name;
                exWorksheet.Cells[row, 11] = item.Value;

                row++;
            }

            row = 27;

            foreach (EyeDoc.Item item in doc.SumList)
            {
                exWorksheet.Cells[row, 13] = item.Kind;
                exWorksheet.Cells[row, 14] = item.Name;
                exWorksheet.Cells[row, 15] = item.Value;

                row++;
            }

            row = 27;

            foreach (EyeDoc.Item item in doc.AllergyList)
            {
                exWorksheet.Cells[row, 17] = item.Kind;
                exWorksheet.Cells[row, 18] = item.Name;
                exWorksheet.Cells[row, 19] = item.Value;

                row++;
            }

            // バーコード画像の挿入対象シート（INI 指定が空なら共通情報以外の全シート）
            List<string> targetSheetNames = new List<string>();

            foreach (string s in targetSheets.Split(','))
            {
                if (s.Trim().Length > 0)
                {
                    targetSheetNames.Add(s.Trim());
                }
            }

            for (int i = 1; i <= sheetCount; i++)
            {
                Excel._Worksheet ws = (Excel._Worksheet)(sheets[i]);

                if (!ws.Name.Equals(CommonSheetName)
                    && (targetSheetNames.Count == 0 || targetSheetNames.Contains(ws.Name)))
                {
                    insertBarcode(ws, barcodeValue);
                }

                Marshal.ReleaseComObject(ws);
            }

            Marshal.ReleaseComObject(sheets);

            // 別名（患者ID_日時_テンプレート名）で TEMP へ保存する。
            // 52 = xlOpenXMLWorkbookMacroEnabled（この Interop.Excel の XlFileFormat には定義がないため数値指定）
            string saveName = Environment.GetEnvironmentVariable("TEMP") + "\\"
                + doc.PtId + "_" + doc.SaveDate + doc.SaveTime + "_" + Path.GetFileName(doc.FileName);

            exWorkbook.SaveAs(saveName, 52, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Excel.XlSaveAsAccessMode.xlExclusive, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            // 開いた時点のアクティブシートへ戻す
            Excel._Worksheet endSheet = (Excel._Worksheet)(exWorkbook.Sheets[startSheetName]);
            endSheet.Select(Missing.Value);
            Marshal.ReleaseComObject(endSheet);

            exApp.EnableEvents = true;
            exApp.ScreenUpdating = true;
        }

        /// <summary>
        /// COM オブジェクトを解放する。MakeDocument 失敗時も無条件に呼べるよう null ガードする。
        /// </summary>
        public void ReleaseExcel()
        {
            if (exWorksheet != null)
            {
                Marshal.ReleaseComObject(exWorksheet);
                exWorksheet = null;
            }

            if (exWorkbook != null)
            {
                Marshal.ReleaseComObject(exWorkbook);
                exWorkbook = null;
            }

            if (exApp != null)
            {
                Marshal.ReleaseComObject(exApp);
                exApp = null;
            }
        }

        /// <summary>
        /// EyeDataSettings.ini の [BARCODE_SETTINGS] を読み込む。
        /// ファイルが無い・読めない・値が不正な場合は既定値を維持する。
        /// </summary>
        private void loadSettings()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EyeDataSettings.ini");

                if (!File.Exists(configPath))
                {
                    return;
                }

                string[] lines = File.ReadAllLines(configPath, Encoding.Default);

                foreach (string line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                    {
                        continue;
                    }

                    string[] parts = line.Split('=');

                    if (parts.Length != 2)
                    {
                        continue;
                    }

                    string key = parts[0].Trim();
                    string val = parts[1].Trim();

                    if (key == "BARCODE_LINE_WIDTH")
                    {
                        float f;

                        if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out f) && f > 0f)
                        {
                            barcodeLineWidth = f;
                        }
                    }
                    else if (key == "BARCODE_HEIGHT")
                    {
                        float f;

                        if (float.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out f) && f > 0f)
                        {
                            barcodeHeight = f;
                        }
                    }
                    else if (key == "BARCODE_QUIET_MODULES")
                    {
                        int n;

                        if (int.TryParse(val, NumberStyles.Integer, CultureInfo.InvariantCulture, out n) && n >= 0)
                        {
                            barcodeQuietModules = n;
                        }
                    }
                    else if (key == "OPERATION_RECORD_CODE")
                    {
                        // バーコードの桁数を保つため、数字のみ・空でない値だけ採用する。
                        if (!string.IsNullOrEmpty(val) && isAllDigits(val))
                        {
                            operationRecordCode = val;
                        }
                    }
                    else if (key == "SURGICAL_NURSING_RECORD_CODE")
                    {
                        if (!string.IsNullOrEmpty(val) && isAllDigits(val))
                        {
                            surgicalNursingRecordCode = val;
                        }
                    }
                    else if (key == "OPERATION_RECORD_SHEETS")
                    {
                        operationRecordSheets = val;
                    }
                    else if (key == "SURGICAL_NURSING_RECORD_SHEETS")
                    {
                        surgicalNursingRecordSheets = val;
                    }
                }
            }
            catch (IOException)
            {
            }
        }

        private void insertBarcode(Excel._Worksheet sheet, string barcodeText)
        {
            if (barcodeText.Length != 36 || !isAllDigits(barcodeText))
            {
                // CODE128-C は偶数桁の数字のみ。36桁の数字でなければ挿入しない。
                return;
            }

            string tempPath = null;
            Excel.Range anchor = null;
            object shapes = null;
            object picture = null;

            try
            {
                tempPath = generateBarcodeImage(barcodeText);
                anchor = (Excel.Range)(sheet.Cells[1, 4]); // D1
                float left = (float)(double)anchor.Left;
                float top = (float)(double)anchor.Top;
                shapes = sheet.Shapes;

                // この Interop.Excel の AddPicture は MsoTriState 型引数（Office コア参照）を
                // 要求しコンパイルできないため、遅延バインディングで呼び出す。
                // 引数: ファイル, LinkToFile=msoFalse(0), SaveWithDocument=msoTrue(-1), 左, 上, 幅, 高さ
                picture = shapes.GetType().InvokeMember("AddPicture", BindingFlags.InvokeMethod, null, shapes,
                    new object[] { tempPath, 0, -1, left, top, 250f, 30f });
                ((Excel.Shape)picture).Placement = Excel.XlPlacement.xlMove;
            }
            finally
            {
                if (picture != null)
                {
                    Marshal.ReleaseComObject(picture);
                }

                if (shapes != null)
                {
                    Marshal.ReleaseComObject(shapes);
                }

                if (anchor != null)
                {
                    Marshal.ReleaseComObject(anchor);
                }

                if (tempPath != null && File.Exists(tempPath))
                {
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                    }
                }
            }
        }

        private string generateBarcodeImage(string barcodeText)
        {
            // CODE128-C のモジュール数: (開始1 + データ(桁/2) + チェック1)*11 + 停止13 + クワイエットゾーン両側。
            int symbols = 2 + barcodeText.Length / 2;
            int totalModules = symbols * 11 + 13 + barcodeQuietModules * 2;
            int widthPx = (int)Math.Ceiling(totalModules * barcodeLineWidth);
            int heightPx = (int)Math.Ceiling(barcodeHeight);
            string tempPath = Path.Combine(Path.GetTempPath(), "barcode_" + Guid.NewGuid().ToString("N") + ".png");

            using (Bitmap bitmap = new Bitmap(widthPx, heightPx))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                    Barcode128 barcode = new Barcode128();
                    float left = barcodeQuietModules * barcodeLineWidth;
                    barcode.Draw(Barcode128.CODE.C, barcodeText, g, left, 0f, barcodeHeight, barcodeLineWidth);
                }

                bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            }

            return tempPath;
        }

        private static bool isAllDigits(string s)
        {
            foreach (char c in s)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
