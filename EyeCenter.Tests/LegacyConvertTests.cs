using System.IO;
using System.Text;
using System.Windows.Forms;
using MedicalLibrary.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// MedicalLibrary の検査機器データ変換（NidekARK1.Convert / EyeRefKrt.Read）の動作確認。
    /// 入力は TestData の合成データ、期待値はリファクタリング前の実装の出力で固定している。
    /// </summary>
    [TestClass]
    public class LegacyConvertTests
    {
        static string TestData(string name)
        {
            return Path.Combine(Path.GetDirectoryName(typeof(LegacyConvertTests).Assembly.Location), "TestData", name);
        }

        static string Expected(string name)
        {
            return File.ReadAllText(TestData(name + ".expected.txt"), Encoding.UTF8);
        }

        [DataTestMethod]
        [DataRow("NidekARK1_full")]
        [DataRow("NidekARK1_partial")]
        [DataRow("NidekARK1_broken")]
        public void NidekARK1_Convert_左右の眼をテキストにする(string name)
        {
            Assert.AreEqual(Expected(name), NidekARK1.Convert(TestData(name + ".xml")));
        }

        [TestMethod]
        public void NidekARK1_Convert_ファイルが無いかXMLでなければ空()
        {
            Assert.AreEqual("", NidekARK1.Convert(TestData("NoSuchFile.xml")));
            Assert.AreEqual("", NidekARK1.Convert(TestData("EyeRefKrt_canon.txt")));
        }

        [DataTestMethod]
        [DataRow("EyeRefKrt_data.csv")]
        [DataRow("EyeRefKrt_canon.txt")]
        public void EyeRefKrt_Read_左右の眼をテキストにする(string name)
        {
            TextBox text_box = new TextBox { Multiline = true };

            Assert.IsTrue(EyeRefKrt.Read(TestData(name), text_box));
            Assert.AreEqual(Expected(name), text_box.Text);
        }

        [TestMethod]
        public void EyeRefKrt_Read_列が足りなければfalse()
        {
            TextBox text_box = new TextBox { Multiline = true, Text = "before" };

            Assert.IsFalse(EyeRefKrt.Read(TestData("EyeRefKrt_short.csv"), text_box));
            Assert.IsFalse(EyeRefKrt.Read(TestData("NoSuchFile.csv"), text_box));
            Assert.AreEqual("before", text_box.Text);
        }
    }
}
