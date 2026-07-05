using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// Barcode128（CODE128 描画）の動作確認。
    /// Bitmap 上の Graphics へ描画するだけなので外部依存なしで実行できる。
    /// </summary>
    [TestClass]
    public class Barcode128Tests
    {
        // 36桁（オペ録・申し送り書のバーコード値と同じ桁数）
        const string Digits36 = "000001001399110070051920260705123456";

        /// <summary>白で初期化した Bitmap に描画し、結果とビットマップを返す。</summary>
        static bool Draw(Barcode128.CODE code, string bar, out Bitmap bmp)
        {
            bmp = new Bitmap(900, 100);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White);

                return new Barcode128().Draw(code, bar, g, 30f, 0f, 80f, 3f);
            }
        }

        static int CountBlackPixels(Bitmap bmp, int y)
        {
            int count = 0;

            for (int x = 0; x < bmp.Width; x++)
            {
                if (bmp.GetPixel(x, y).ToArgb() == Color.Black.ToArgb())
                {
                    count++;
                }
            }

            return count;
        }

        [TestMethod]
        public void Draw_数字以外を含む入力はfalseを返し何も描画しない()
        {
            Bitmap bmp;
            bool result = Draw(Barcode128.CODE.C, "12345A", out bmp);

            using (bmp)
            {
                Assert.IsFalse(result);
                Assert.AreEqual(0, CountBlackPixels(bmp, 40));
            }
        }

        [TestMethod]
        public void Draw_CodeCで36桁の数字はtrueを返しバーを描画する()
        {
            Bitmap bmp;
            bool result = Draw(Barcode128.CODE.C, Digits36, out bmp);

            using (bmp)
            {
                Assert.IsTrue(result);
                Assert.IsTrue(CountBlackPixels(bmp, 40) > 100, "黒バーが描画されていること");

                // 左クワイエットゾーン（left=30 より左）は白のまま
                for (int x = 0; x < 28; x++)
                {
                    Assert.AreEqual(Color.White.ToArgb(), bmp.GetPixel(x, 40).ToArgb(),
                        "クワイエットゾーン x=" + x + " が白であること");
                }
            }
        }

        [TestMethod]
        public void Draw_CodeAとCodeBも数字入力でtrueを返す()
        {
            Bitmap bmpA;
            Bitmap bmpB;
            bool resultA = Draw(Barcode128.CODE.A, "12345678", out bmpA);
            bool resultB = Draw(Barcode128.CODE.B, "12345678", out bmpB);

            using (bmpA)
            using (bmpB)
            {
                Assert.IsTrue(resultA);
                Assert.IsTrue(resultB);
                Assert.IsTrue(CountBlackPixels(bmpA, 40) > 0);
                Assert.IsTrue(CountBlackPixels(bmpB, 40) > 0);
            }
        }

        [TestMethod]
        public void Draw_同じ入力からは同じ描画結果になる()
        {
            Bitmap bmp1;
            Bitmap bmp2;
            Draw(Barcode128.CODE.C, Digits36, out bmp1);
            Draw(Barcode128.CODE.C, Digits36, out bmp2);

            using (bmp1)
            using (bmp2)
            {
                for (int x = 0; x < bmp1.Width; x++)
                {
                    Assert.AreEqual(bmp1.GetPixel(x, 40).ToArgb(), bmp2.GetPixel(x, 40).ToArgb(),
                        "x=" + x + " のピクセルが一致すること");
                }
            }
        }
    }
}
