using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// ExcelControl のうち Excel COM に依存しないロジック
    /// （数字判定・バーコード画像生成・EyeDataSettings.ini 読込）の動作確認。
    /// private メンバーはリフレクションで呼び出す。
    /// </summary>
    [TestClass]
    public class ExcelControlTests
    {
        static object Invoke(ExcelControl target, string method, params object[] args)
        {
            MethodInfo mi = typeof(ExcelControl).GetMethod(method,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            Assert.IsNotNull(mi, method + " が存在すること");

            return mi.Invoke(target, args);
        }

        static object GetField(ExcelControl target, string field)
        {
            FieldInfo fi = typeof(ExcelControl).GetField(field,
                BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.IsNotNull(fi, field + " が存在すること");

            return fi.GetValue(target);
        }

        [TestMethod]
        public void IsAllDigits_半角数字のみtrueを返す()
        {
            Assert.IsTrue((bool)Invoke(null, "isAllDigits", "0123456789"));
            Assert.IsFalse((bool)Invoke(null, "isAllDigits", "12a4"));
            Assert.IsFalse((bool)Invoke(null, "isAllDigits", "１２３"), "全角数字は不可");
        }

        [TestMethod]
        public void GenerateBarcodeImage_既定設定で759x80のPNGを生成する()
        {
            // 36桁: シンボル数 = 2 + 18 = 20, 総モジュール = 20*11 + 13 + 10*2 = 253
            // 幅 = ceil(253 * 3) = 759 px, 高さ = 80 px
            string barcodeValue = "000001001399110070051920260705123456";
            string path = (string)Invoke(new ExcelControl(), "generateBarcodeImage", barcodeValue);

            try
            {
                Assert.IsTrue(File.Exists(path), "PNGファイルが生成されること");

                using (Bitmap bmp = new Bitmap(path))
                {
                    Assert.AreEqual(759, bmp.Width);
                    Assert.AreEqual(80, bmp.Height);
                }
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        [TestMethod]
        public void LoadSettings_INIの有効値は反映し不正値は既定値を維持する()
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EyeDataSettings.ini");

            File.WriteAllText(iniPath,
                "; コメント行\r\n" +
                "[BARCODE_SETTINGS]\r\n" +
                "BARCODE_LINE_WIDTH=2.5\r\n" +
                "BARCODE_HEIGHT=-5\r\n" +
                "BARCODE_QUIET_MODULES=4\r\n" +
                "OPERATION_RECORD_CODE=12ab\r\n" +
                "SURGICAL_NURSING_RECORD_CODE=55555\r\n");

            try
            {
                ExcelControl target = new ExcelControl();
                Invoke(target, "loadSettings");

                Assert.AreEqual(2.5f, (float)GetField(target, "barcodeLineWidth"), "有効値は反映");
                Assert.AreEqual(80f, (float)GetField(target, "barcodeHeight"), "負値は既定値を維持");
                Assert.AreEqual(4, (int)GetField(target, "barcodeQuietModules"), "有効値は反映");
                Assert.AreEqual("39911", (string)GetField(target, "operationRecordCode"), "数字以外を含む値は既定値を維持");
                Assert.AreEqual("55555", (string)GetField(target, "surgicalNursingRecordCode"), "有効値は反映");
            }
            finally
            {
                File.Delete(iniPath);
            }
        }

        [TestMethod]
        public void LoadSettings_INIが無ければ既定値のまま()
        {
            string iniPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EyeDataSettings.ini");

            Assert.IsFalse(File.Exists(iniPath), "前提: テスト実行フォルダに INI が無いこと");

            ExcelControl target = new ExcelControl();
            Invoke(target, "loadSettings");

            Assert.AreEqual(3f, (float)GetField(target, "barcodeLineWidth"));
            Assert.AreEqual(80f, (float)GetField(target, "barcodeHeight"));
            Assert.AreEqual(10, (int)GetField(target, "barcodeQuietModules"));
            Assert.AreEqual("39911", (string)GetField(target, "operationRecordCode"));
            Assert.AreEqual("34411", (string)GetField(target, "surgicalNursingRecordCode"));
        }
    }
}
