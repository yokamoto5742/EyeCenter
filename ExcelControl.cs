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
    /// Excelテンプレートを開き、共通情報シートへの書き込み・別名保存を行う。
    /// オペ録・眼科申し送り書はバーコード画像の生成と挿入も行い、
    /// コンタクトレンズ注文書・眼鏡処方はバーコードなしで出力する。（EyeAgree の ExcelControl を移植）
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

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        const int SW_RESTORE = 9;

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

        // バーコードなし帳票のテンプレートファイル名の既定値。
        // EyeDataSettings.ini の [DOCUMENT_SETTINGS] で上書きできる。
        const string DefaultContactOrderSeedFile = "コンタクトレンズ注文書(シード).xlsx";
        const string DefaultContactOrderPanacomFile = "コンタクトレンズ注文書(パナコム).xlsx";
        const string DefaultGlassPrescriptionFile = "眼鏡処方.xlsx";
        const string DefaultNsFile = "眼科申し送り書.xlsm";
        const string DefaultRecordFile = "オペ録.xlsm";

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
                // バーコード対象外のテンプレート（同意書など）は従来の処理のまま開く。
                // Application は EyeDoc.ExcelOpen 内部で管理され取得できないため、ROT経由で前面化する。
                doc.ExcelOpen();
                bringLatestExcelToFront();
                return;
            }

            createDocument(doc, documentCode, targetSheets, true, true);
        }

        /// <summary>
        /// バーコードを付与せず、共通情報シートへの書き込みと TEMP への別名保存のみ行う
        /// （コンタクトレンズ注文書・眼鏡処方用）。writeLists が false の場合は
        /// B1〜B12 のみ書き込み、27行目以降の各リストは出力しない。
        /// 呼び出し側で例外を処理し、finally で ReleaseExcel を呼ぶこと。
        /// </summary>
        public void MakeSimpleDocument(EyeDoc doc, bool writeLists)
        {
            createDocument(doc, "", "", false, writeLists);
        }

        /// <summary>
        /// テンプレートを開いて共通情報シートに書き込み、TEMP へ別名保存して表示する共通処理。
        /// useBarcode が true の場合のみバーコード値の組み立てと画像挿入を行う。
        /// </summary>
        private void createDocument(EyeDoc doc, string documentCode, string targetSheets, bool useBarcode, bool writeLists)
        {
            if (!File.Exists(doc.FileName))
            {
                return;
            }

            exApp = new Excel.Application();

            // 自動処理中はテンプレートのマクロを起動させない
            exApp.EnableEvents = false;

            // 生成が完了するまで非表示・描画停止のまま処理する（最後に表示する）
            exApp.ScreenUpdating = false;

            exWorkbook = (Excel._Workbook)(exApp.Workbooks.Open(doc.FileName,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Missing.Value));

            exWorksheet = (Excel._Worksheet)(exWorkbook.Sheets[CommonSheetName]);

            // セル書込み中の逐次再計算を止める（保存前に自動計算へ戻す）
            exApp.Calculation = Excel.XlCalculation.xlCalculationManual;

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

            string barcodeValue = useBarcode
                ? buildBarcodeValue(doc.PtId, documentCode, doc.UserId, getOpeDate(doc), doc.SaveTime)
                : "";

            // コンタクトレンズ注文書（シード・パナコム）のみ B12・B13 に住所・電話番号を出力する
            bool isContactOrder = !useBarcode && writeLists;

            // 共通情報シート B1〜B12（コンタクトレンズ注文書は B13 まで。セル単位の COM 呼び出しを避けるため一括代入する）
            object[,] commonData = new object[isContactOrder ? 13 : 12, 1];
            commonData[0, 0] = doc.PtId;
            commonData[1, 0] = doc.Pat.Kana;
            commonData[2, 0] = doc.Pat.Name;
            commonData[3, 0] = doc.Pat.SexNameShort;
            commonData[4, 0] = doc.Pat.BirthString;
            commonData[5, 0] = doc.Pat.Age;
            commonData[6, 0] = DeptCode;
            commonData[7, 0] = DeptName;
            commonData[8, 0] = doc.UserId.PadLeft(5, '0');
            commonData[9, 0] = doc.SaveDate;
            commonData[10, 0] = doc.SaveTime;

            if (isContactOrder)
            {
                commonData[11, 0] = doc.Pat.Addr;
                commonData[12, 0] = doc.Pat.Tel;
            }
            else
            {
                commonData[11, 0] = barcodeValue;
            }

            writeRange(1, 2, commonData);

            // 27行目以降の各リスト（従来の EyeDoc.ExcelOpen と同じ列配置）
            if (writeLists)
            {
                writeItemList(doc.ItemList, 1);
                writeItemList(doc.ContactList, 5);
                writeItemList(doc.PatInfoList, 9);
                writeItemList(doc.SumList, 13);
                writeItemList(doc.AllergyList, 17);
            }

            if (useBarcode)
            {
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
            }

            Marshal.ReleaseComObject(sheets);

            // 自動計算へ戻し、保存前に一度だけ再計算させる
            exApp.Calculation = Excel.XlCalculation.xlCalculationAutomatic;

            // 別名（患者ID_日時_テンプレート名）で TEMP へ保存する。
            // 52 = xlOpenXMLWorkbookMacroEnabled / 51 = xlOpenXMLWorkbook
            // （この Interop.Excel の XlFileFormat には定義がないため数値指定）。
            // テンプレートの拡張子と形式を一致させないと保存時に警告・失敗が発生する。
            int fileFormat = Path.GetExtension(doc.FileName).Equals(".xlsm", StringComparison.OrdinalIgnoreCase) ? 52 : 51;

            string saveName = Environment.GetEnvironmentVariable("TEMP") + "\\"
                + doc.PtId + "_" + doc.SaveDate + doc.SaveTime + "_" + Path.GetFileName(doc.FileName);

            exWorkbook.SaveAs(saveName, fileFormat, Missing.Value, Missing.Value, Missing.Value,
                Missing.Value, Excel.XlSaveAsAccessMode.xlExclusive, Missing.Value,
                Missing.Value, Missing.Value, Missing.Value, Missing.Value);

            // 開いた時点のアクティブシートへ戻す
            Excel._Worksheet endSheet = (Excel._Worksheet)(exWorkbook.Sheets[startSheetName]);
            endSheet.Select(Missing.Value);
            Marshal.ReleaseComObject(endSheet);

            exApp.EnableEvents = true;
            exApp.ScreenUpdating = true;
            exApp.Visible = true;
        }

        /// <summary>
        /// 2次元配列を startRow, startColumn 起点の範囲へ一括代入する。
        /// </summary>
        private void writeRange(int startRow, int startColumn, object[,] data)
        {
            Excel.Range start = (Excel.Range)(exWorksheet.Cells[startRow, startColumn]);
            Excel.Range end = (Excel.Range)(exWorksheet.Cells[
                startRow + data.GetLength(0) - 1, startColumn + data.GetLength(1) - 1]);
            Excel.Range range = exWorksheet.get_Range(start, end);

            range.Value2 = data;

            Marshal.ReleaseComObject(range);
            Marshal.ReleaseComObject(end);
            Marshal.ReleaseComObject(start);
        }

        /// <summary>
        /// EyeDoc.Item のリストを 27 行目起点で Kind・Name・Value の3列に一括書き込みする。
        /// </summary>
        private void writeItemList(List<EyeDoc.Item> list, int startColumn)
        {
            if (list.Count == 0)
            {
                return;
            }

            object[,] data = new object[list.Count, 3];

            for (int i = 0; i < list.Count; i++)
            {
                data[i, 0] = list[i].Kind;
                data[i, 1] = list[i].Name;
                data[i, 2] = list[i].Value;
            }

            writeRange(27, startColumn, data);
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
                // 非表示のまま処理しているため、途中で失敗しても Excel を隠れたまま残さない
                try
                {
                    exApp.EnableEvents = true;
                    exApp.ScreenUpdating = true;
                    exApp.Visible = true;
                    bringToFront(exApp);
                }
                catch
                {
                }

                Marshal.ReleaseComObject(exApp);
                exApp = null;
            }
        }

        /// <summary>
        /// Excel の Application ウィンドウを最前面に表示する。Visible=true だけでは
        /// 別プロセスのウィンドウとして背面に隠れることがあるため、明示的に前面化する。
        /// 失敗しても生成処理自体には影響しないよう例外は握りつぶす。
        /// </summary>
        private static void bringToFront(Excel.Application app)
        {
            try
            {
                IntPtr hWnd = new IntPtr(app.Hwnd);

                if (IsIconic(hWnd))
                {
                    ShowWindow(hWnd, SW_RESTORE);
                }

                SetForegroundWindow(hWnd);
            }
            catch
            {
            }
        }

        /// <summary>
        /// EyeDoc.ExcelOpen が内部で開いた Excel の Application を ROT（Running Object Table）
        /// 経由で取得し、前面化する。取得できない場合は何もしない。
        /// </summary>
        private static void bringLatestExcelToFront()
        {
            object app = null;

            try
            {
                app = Marshal.GetActiveObject("Excel.Application");
                bringToFront((Excel.Application)app);
            }
            catch
            {
            }
            finally
            {
                if (app != null)
                {
                    Marshal.ReleaseComObject(app);
                }
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

        /// <summary>
        /// コンタクトレンズ注文書テンプレートのフルパスを返す。
        /// ボタン表示名に「パナコム」を含むかどうかでシード用／パナコム用を切り替える。
        /// </summary>
        public static string GetContactOrderFileName(string buttonText)
        {
            if (buttonText.Contains("パナコム"))
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    readIniValue("CONTACT_ORDER_PANACOM_FILE", DefaultContactOrderPanacomFile));
            }

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                readIniValue("CONTACT_ORDER_SEED_FILE", DefaultContactOrderSeedFile));
        }

        /// <summary>
        /// 眼鏡処方テンプレートのフルパスを返す。
        /// </summary>
        public static string GetGlassPrescriptionFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                readIniValue("GLASS_PRESCRIPTION_FILE", DefaultGlassPrescriptionFile));
        }

        /// <summary>
        /// 眼科申し送り書テンプレートのフルパスを返す。
        /// </summary>
        public static string GetNsFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                readIniValue("NS_FILE", DefaultNsFile));
        }

        /// <summary>
        /// オペ録テンプレートのフルパスを返す。
        /// </summary>
        public static string GetRecordFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                readIniValue("RECORD_FILE", DefaultRecordFile));
        }

        /// <summary>
        /// EyeDataSettings.ini から指定キーの値を読み取る。
        /// ファイルが無い・読めない・キーが無い場合は既定値を返す。
        /// </summary>
        private static string readIniValue(string key, string defaultValue)
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EyeDataSettings.ini");

                if (File.Exists(configPath))
                {
                    foreach (string line in File.ReadAllLines(configPath, Encoding.Default))
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";"))
                        {
                            continue;
                        }

                        string[] parts = line.Split('=');

                        if (parts.Length == 2 && parts[0].Trim() == key && parts[1].Trim().Length > 0)
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }
            catch (IOException)
            {
            }

            return defaultValue;
        }

        /// <summary>
        /// ItemList の手術基本情報から手術日を yyyyMMdd の8桁で取り出す。
        /// 取得できない場合は作成日（doc.SaveDate）へフォールバックする。
        /// </summary>
        private static string getOpeDate(EyeDoc doc)
        {
            foreach (EyeDoc.Item item in doc.ItemList)
            {
                if (item.Kind == "手術基本情報" && item.Name == "手術日")
                {
                    DateTime d;

                    if (DateTime.TryParse(item.Value, out d))
                    {
                        return d.ToString("yyyyMMdd");
                    }

                    break;
                }
            }

            return doc.SaveDate;
        }

        /// <summary>
        /// 36桁バーコード値を組み立てる（患者ID9桁 + 文書コード5桁 + 診療科3桁 + 入力者ID5桁 + 手術日8桁 + 作成時刻6桁）。
        /// </summary>
        private static string buildBarcodeValue(string ptId, string documentCode, string userId, string saveDate, string saveTime)
        {
            return ptId.PadLeft(9, '0') + documentCode.PadLeft(5, '0') + DeptCode
                + userId.PadLeft(5, '0') + saveDate + saveTime;
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
