using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EyeCenter.Tests
{
    /// <summary>
    /// BarcodeKeyFilter のバーコード判定（打鍵間隔・桁数）と患者ID抽出の動作確認。
    /// </summary>
    [TestClass]
    public class BarcodeKeyFilterTests
    {
        const string Digits36 = "000001001399110070051920260705123456";

        /// <summary>code を interval ミリ秒間隔で入力し、最後の文字の時刻を返す。</summary>
        static int Type(BarcodeKeyFilter f, string code, int start, int interval)
        {
            int tick = start;

            for (int i = 0; i < code.Length; i++)
            {
                if (i > 0)
                {
                    tick += interval;
                }

                f.AddDigit(code[i], tick);
            }

            return tick;
        }

        [TestMethod]
        public void 速い入力の36桁とEnterはバーコードとみなす()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, Digits36, 1000, 5);

            Assert.AreEqual(Digits36, f.Complete(tick + 5));
        }

        [TestMethod]
        public void 人の打鍵速度の入力はバーコードとみなさない()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, Digits36, 1000, 150);

            Assert.IsNull(f.Complete(tick + 150));
        }

        [TestMethod]
        public void 桁数が36でなければバーコードとみなさない()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, Digits36.Substring(0, 35), 1000, 5);
            Assert.IsNull(f.Complete(tick + 5), "35桁");

            tick = Type(f, Digits36 + "1", 2000, 5);
            Assert.IsNull(f.Complete(tick + 5), "37桁");
        }

        [TestMethod]
        public void 手入力の数字の直後に読み取っても読み取り分だけで判定する()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, "12", 1000, 300);

            Assert.IsTrue(f.AddDigit(Digits36[0], tick + 300), "間隔が空いたら新しい読み取りの始まり");
            tick = Type(f, Digits36.Substring(1), tick + 305, 5);

            Assert.AreEqual(Digits36, f.Complete(tick + 5));
        }

        [TestMethod]
        public void Enterが遅れた場合はバーコードとみなさない()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, Digits36, 1000, 5);

            Assert.IsNull(f.Complete(tick + 500));
        }

        [TestMethod]
        public void 判定後はバッファが空になる()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, Digits36, 1000, 5);
            f.Complete(tick + 5);

            Assert.IsNull(f.Complete(tick + 10));
        }

        [TestMethod]
        public void TickCountが一周しても判定できる()
        {
            BarcodeKeyFilter f = new BarcodeKeyFilter(50);
            int tick = Type(f, Digits36, int.MaxValue - 100, 5);

            Assert.AreEqual(Digits36, f.Complete(unchecked(tick + 5)));
        }

        [TestMethod]
        public void 先頭9桁から患者IDを取り出す()
        {
            Assert.AreEqual("1001", BarcodeKeyFilter.ParsePatientId(Digits36), "ゼロ埋めを除く");
            Assert.AreEqual("123456789", BarcodeKeyFilter.ParsePatientId("123456789" + Digits36.Substring(9)));
        }

        [TestMethod]
        public void 患者IDが0なら取り出さない()
        {
            Assert.IsNull(BarcodeKeyFilter.ParsePatientId("000000000" + Digits36.Substring(9)));
        }
    }
}
