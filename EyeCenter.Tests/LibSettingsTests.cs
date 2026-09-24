using System.IO;
using System.Reflection;
using MedicalLibrary.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// MedicalLibrary の LibSettings（MedicalLibrary_Settings.xml の読み込み）の動作確認。
    /// Init は固定の場所の設定ファイルを読むため、private の Read をリフレクションで呼ぶ。
    /// </summary>
    [TestClass]
    public class LibSettingsTests
    {
        static void Read(string xml_file)
        {
            typeof(LibSettings)
                .GetMethod("Read", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, new object[] { xml_file });
        }

        [TestCleanup]
        public void Cleanup()
        {
            LibSettings.Current = new LibSettings();
        }

        [TestMethod]
        public void Read_接続文字列と患者基本情報コードを読む()
        {
            string dir = Path.GetDirectoryName(typeof(LibSettingsTests).Assembly.Location);

            Read(Path.Combine(dir, "TestData", "LibSettings.xml"));

            LibSettings s = LibSettings.Current;

            // XML に無い項目は既定値のまま
            Assert.AreEqual(new LibSettings().DBConnectionString1, s.DBConnectionString1);
            Assert.AreEqual("User Id=a;Password=b;Data Source=c2;", s.DBConnectionString2);
            Assert.AreEqual("User Id=a;Password=b;Data Source=c3;", s.DBConnectionString3);

            Assert.AreEqual(6, s.BaseInfoCodes.BaseInfoCodeList.Count);
            Assert.AreEqual("99", s.BaseInfoCodes.BaseInfoCodeList[5].Code);
            Assert.AreEqual("", s.BaseInfoCodes.BaseInfoCodeList[5].Name);

            Assert.AreEqual("01", s.BaseInfoCodes.Diag);
            Assert.AreEqual("02", s.BaseInfoCodes.Allergy);
            Assert.AreEqual("03", s.BaseInfoCodes.Drug);
            Assert.AreEqual("10", s.BaseInfoCodes.Height);
            Assert.AreEqual("11", s.BaseInfoCodes.Weight);
        }

        [TestMethod]
        public void BaseInfoCodes_該当が無ければ空文字()
        {
            Assert.AreEqual("", new BaseInfoCodes().Diag);
            Assert.AreEqual("", new BaseInfoCodes().Weight);
        }
    }
}
