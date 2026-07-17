using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// AppConfig.GetInt（appSettings の int 変換・既定値フォールバック）の動作確認。
    /// App.config の appSettings に境界値ごとのキーを定義し、実際の設定読込経路で検証する。
    /// </summary>
    [TestClass]
    public class AppConfigTests
    {
        [TestMethod]
        public void GetInt_未設定なら既定値を返す()
        {
            Assert.AreEqual(42, AppConfig.GetInt("AppConfigTests_NotDefined", 42));
        }

        [TestMethod]
        public void GetInt_空文字なら既定値を返す()
        {
            Assert.AreEqual(42, AppConfig.GetInt("AppConfigTests_Empty", 42));
        }

        [TestMethod]
        public void GetInt_非数値なら既定値を返す()
        {
            Assert.AreEqual(42, AppConfig.GetInt("AppConfigTests_NonNumeric", 42));
        }

        [TestMethod]
        public void GetInt_0なら既定値を返す()
        {
            Assert.AreEqual(42, AppConfig.GetInt("AppConfigTests_Zero", 42));
        }

        [TestMethod]
        public void GetInt_負数なら既定値を返す()
        {
            Assert.AreEqual(42, AppConfig.GetInt("AppConfigTests_Negative", 42));
        }

        [TestMethod]
        public void GetInt_正数なら採用される()
        {
            Assert.AreEqual(100, AppConfig.GetInt("AppConfigTests_Positive", 42));
        }
    }
}
