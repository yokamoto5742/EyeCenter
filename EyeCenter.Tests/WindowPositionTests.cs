using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// WindowPosition の保存値の解釈と、画面外判定の動作確認。
    /// </summary>
    [TestClass]
    public class WindowPositionTests
    {
        static readonly Rectangle[] TwoScreens =
        {
            new Rectangle(0, 0, 1920, 1040),
            new Rectangle(1920, 0, 1280, 984)
        };

        [TestMethod]
        public void TryParse_カンマ区切りの座標を読み取る()
        {
            Point p;

            Assert.IsTrue(WindowPosition.TryParse("-8,120", out p));
            Assert.AreEqual(new Point(-8, 120), p);
        }

        [TestMethod]
        public void TryParse_空や不正な値はfalse()
        {
            Point p;

            Assert.IsFalse(WindowPosition.TryParse(null, out p));
            Assert.IsFalse(WindowPosition.TryParse("", out p));
            Assert.IsFalse(WindowPosition.TryParse("100", out p));
            Assert.IsFalse(WindowPosition.TryParse("a,b", out p));
            Assert.IsFalse(WindowPosition.TryParse("1,2,3", out p));
        }

        [TestMethod]
        public void IsVisible_作業領域内ならtrue()
        {
            Assert.IsTrue(WindowPosition.IsVisible(new Point(100, 100), TwoScreens));
            Assert.IsTrue(WindowPosition.IsVisible(new Point(2000, 50), TwoScreens));
        }

        [TestMethod]
        public void IsVisible_最大化相当の負の座標でもtrue()
        {
            Assert.IsTrue(WindowPosition.IsVisible(new Point(-8, -8), TwoScreens));
        }

        [TestMethod]
        public void IsVisible_外したモニター上の位置ならfalse()
        {
            Assert.IsFalse(WindowPosition.IsVisible(new Point(3300, 100), TwoScreens));
            Assert.IsFalse(WindowPosition.IsVisible(new Point(-1500, 100), TwoScreens));
        }
    }
}
