using System.Drawing;

namespace EyeCenter
{
    /// <summary>
    /// CODE128 バーコードを Graphics に描画する。（AgentlabUtilityLibrary の Barcode128 を移植）
    /// </summary>
    class Barcode128
    {
        public enum CODE
        {
            A = 1,
            B,
            C
        }

        private int[][] barTable = new int[107][];

        public Barcode128()
        {
            barTable[0] = new int[6] { 2, 5, 2, 6, 2, 6 };
            barTable[1] = new int[6] { 2, 6, 2, 5, 2, 6 };
            barTable[2] = new int[6] { 2, 6, 2, 6, 2, 5 };
            barTable[3] = new int[6] { 1, 6, 1, 6, 2, 7 };
            barTable[4] = new int[6] { 1, 6, 1, 7, 2, 6 };
            barTable[5] = new int[6] { 1, 7, 1, 6, 2, 6 };
            barTable[6] = new int[6] { 1, 6, 2, 6, 1, 7 };
            barTable[7] = new int[6] { 1, 6, 2, 7, 1, 6 };
            barTable[8] = new int[6] { 1, 7, 2, 6, 1, 6 };
            barTable[9] = new int[6] { 2, 6, 1, 6, 1, 7 };
            barTable[10] = new int[6] { 2, 6, 1, 7, 1, 6 };
            barTable[11] = new int[6] { 2, 7, 1, 6, 1, 6 };
            barTable[12] = new int[6] { 1, 5, 2, 6, 3, 6 };
            barTable[13] = new int[6] { 1, 6, 2, 5, 3, 6 };
            barTable[14] = new int[6] { 1, 6, 2, 6, 3, 5 };
            barTable[15] = new int[6] { 1, 5, 3, 6, 2, 6 };
            barTable[16] = new int[6] { 1, 6, 3, 5, 2, 6 };
            barTable[17] = new int[6] { 1, 6, 3, 6, 2, 5 };
            barTable[18] = new int[6] { 2, 6, 3, 6, 1, 5 };
            barTable[19] = new int[6] { 2, 6, 1, 5, 3, 6 };
            barTable[20] = new int[6] { 2, 6, 1, 6, 3, 5 };
            barTable[21] = new int[6] { 2, 5, 3, 6, 1, 6 };
            barTable[22] = new int[6] { 2, 6, 3, 5, 1, 6 };
            barTable[23] = new int[6] { 3, 5, 2, 5, 3, 5 };
            barTable[24] = new int[6] { 3, 5, 1, 6, 2, 6 };
            barTable[25] = new int[6] { 3, 6, 1, 5, 2, 6 };
            barTable[26] = new int[6] { 3, 6, 1, 6, 2, 5 };
            barTable[27] = new int[6] { 3, 5, 2, 6, 1, 6 };
            barTable[28] = new int[6] { 3, 6, 2, 5, 1, 6 };
            barTable[29] = new int[6] { 3, 6, 2, 6, 1, 5 };
            barTable[30] = new int[6] { 2, 5, 2, 5, 2, 7 };
            barTable[31] = new int[6] { 2, 5, 2, 7, 2, 5 };
            barTable[32] = new int[6] { 2, 7, 2, 5, 2, 5 };
            barTable[33] = new int[6] { 1, 5, 1, 7, 2, 7 };
            barTable[34] = new int[6] { 1, 7, 1, 5, 2, 7 };
            barTable[35] = new int[6] { 1, 7, 1, 7, 2, 5 };
            barTable[36] = new int[6] { 1, 5, 2, 7, 1, 7 };
            barTable[37] = new int[6] { 1, 7, 2, 5, 1, 7 };
            barTable[38] = new int[6] { 1, 7, 2, 7, 1, 5 };
            barTable[39] = new int[6] { 2, 5, 1, 7, 1, 7 };
            barTable[40] = new int[6] { 2, 7, 1, 5, 1, 7 };
            barTable[41] = new int[6] { 2, 7, 1, 7, 1, 5 };
            barTable[42] = new int[6] { 1, 5, 2, 5, 3, 7 };
            barTable[43] = new int[6] { 1, 5, 2, 7, 3, 5 };
            barTable[44] = new int[6] { 1, 7, 2, 5, 3, 5 };
            barTable[45] = new int[6] { 1, 5, 3, 5, 2, 7 };
            barTable[46] = new int[6] { 1, 5, 3, 7, 2, 5 };
            barTable[47] = new int[6] { 1, 7, 3, 5, 2, 5 };
            barTable[48] = new int[6] { 3, 5, 3, 5, 2, 5 };
            barTable[49] = new int[6] { 2, 5, 1, 7, 3, 5 };
            barTable[50] = new int[6] { 2, 7, 1, 5, 3, 5 };
            barTable[51] = new int[6] { 2, 5, 3, 5, 1, 7 };
            barTable[52] = new int[6] { 2, 5, 3, 7, 1, 5 };
            barTable[53] = new int[6] { 2, 5, 3, 5, 3, 5 };
            barTable[54] = new int[6] { 3, 5, 1, 5, 2, 7 };
            barTable[55] = new int[6] { 3, 5, 1, 7, 2, 5 };
            barTable[56] = new int[6] { 3, 7, 1, 5, 2, 5 };
            barTable[57] = new int[6] { 3, 5, 2, 5, 1, 7 };
            barTable[58] = new int[6] { 3, 5, 2, 7, 1, 5 };
            barTable[59] = new int[6] { 3, 7, 2, 5, 1, 5 };
            barTable[60] = new int[6] { 3, 5, 4, 5, 1, 5 };
            barTable[61] = new int[6] { 2, 6, 1, 8, 1, 5 };
            barTable[62] = new int[6] { 4, 7, 1, 5, 1, 5 };
            barTable[63] = new int[6] { 1, 5, 1, 6, 2, 8 };
            barTable[64] = new int[6] { 1, 5, 1, 8, 2, 6 };
            barTable[65] = new int[6] { 1, 6, 1, 5, 2, 8 };
            barTable[66] = new int[6] { 1, 6, 1, 8, 2, 5 };
            barTable[67] = new int[6] { 1, 8, 1, 5, 2, 6 };
            barTable[68] = new int[6] { 1, 8, 1, 6, 2, 5 };
            barTable[69] = new int[6] { 1, 5, 2, 6, 1, 8 };
            barTable[70] = new int[6] { 1, 5, 2, 8, 1, 6 };
            barTable[71] = new int[6] { 1, 6, 2, 5, 1, 8 };
            barTable[72] = new int[6] { 1, 6, 2, 8, 1, 5 };
            barTable[73] = new int[6] { 1, 8, 2, 5, 1, 6 };
            barTable[74] = new int[6] { 1, 8, 2, 6, 1, 5 };
            barTable[75] = new int[6] { 2, 8, 1, 6, 1, 5 };
            barTable[76] = new int[6] { 2, 6, 1, 5, 1, 8 };
            barTable[77] = new int[6] { 4, 5, 3, 5, 1, 5 };
            barTable[78] = new int[6] { 2, 8, 1, 5, 1, 6 };
            barTable[79] = new int[6] { 1, 7, 4, 5, 1, 5 };
            barTable[80] = new int[6] { 1, 5, 1, 6, 4, 6 };
            barTable[81] = new int[6] { 1, 6, 1, 5, 4, 6 };
            barTable[82] = new int[6] { 1, 6, 1, 6, 4, 5 };
            barTable[83] = new int[6] { 1, 5, 4, 6, 1, 6 };
            barTable[84] = new int[6] { 1, 6, 4, 5, 1, 6 };
            barTable[85] = new int[6] { 1, 6, 4, 6, 1, 5 };
            barTable[86] = new int[6] { 4, 5, 1, 6, 1, 6 };
            barTable[87] = new int[6] { 4, 6, 1, 5, 1, 6 };
            barTable[88] = new int[6] { 4, 6, 1, 6, 1, 5 };
            barTable[89] = new int[6] { 2, 5, 2, 5, 4, 5 };
            barTable[90] = new int[6] { 2, 5, 4, 5, 2, 5 };
            barTable[91] = new int[6] { 4, 5, 2, 5, 2, 5 };
            barTable[92] = new int[6] { 1, 5, 1, 5, 4, 7 };
            barTable[93] = new int[6] { 1, 5, 1, 7, 4, 5 };
            barTable[94] = new int[6] { 1, 7, 1, 5, 4, 5 };
            barTable[95] = new int[6] { 1, 5, 4, 5, 1, 7 };
            barTable[96] = new int[6] { 1, 5, 4, 7, 1, 5 };
            barTable[97] = new int[6] { 4, 5, 1, 5, 1, 7 };
            barTable[98] = new int[6] { 4, 5, 1, 7, 1, 5 };
            barTable[99] = new int[6] { 1, 5, 3, 5, 4, 5 };
            barTable[100] = new int[6] { 1, 5, 4, 5, 3, 5 };
            barTable[101] = new int[6] { 3, 5, 1, 5, 4, 5 };
            barTable[102] = new int[6] { 4, 5, 1, 5, 3, 5 };
            barTable[103] = new int[6] { 2, 5, 1, 8, 1, 6 };
            barTable[104] = new int[6] { 2, 5, 1, 6, 1, 8 };
            barTable[105] = new int[6] { 2, 5, 1, 6, 3, 6 };
            barTable[106] = new int[7] { 2, 7, 3, 5, 1, 5, 2 };
        }

        public bool Draw(CODE code, string bar, Graphics g, float left, float top, float height, float line_width)
        {
            char[] array = bar.ToCharArray();

            foreach (char c in array)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }

            Pen[] array2 = new Pen[9]
            {
                new Pen(Brushes.White, 0f),
                new Pen(Brushes.Black, line_width),
                new Pen(Brushes.Black, line_width * 2f),
                new Pen(Brushes.Black, line_width * 3f),
                new Pen(Brushes.Black, line_width * 4f),
                new Pen(Brushes.White, line_width),
                new Pen(Brushes.White, line_width * 2f),
                new Pen(Brushes.White, line_width * 3f),
                new Pen(Brushes.White, line_width * 4f)
            };

            float num = left;
            int num2 = 105;

            switch (code)
            {
            case CODE.A:
                num2 = 103;
                break;
            case CODE.B:
                num2 = 104;
                break;
            }

            for (int j = 0; j < 6; j++)
            {
                g.DrawLine(array2[barTable[num2][j]], num + array2[barTable[num2][j]].Width / 2f, top, num + array2[barTable[num2][j]].Width / 2f, top + height);
                num += array2[barTable[num2][j]].Width;
            }

            switch (code)
            {
            case CODE.A:
            case CODE.B:
            {
                for (int m = 0; m < bar.Length; m++)
                {
                    int num4 = bar[m] - 32;

                    for (int n = 0; n < 6; n++)
                    {
                        g.DrawLine(array2[barTable[num4][n]], num + array2[barTable[num4][n]].Width / 2f, top, num + array2[barTable[num4][n]].Width / 2f, top + height);
                        num += array2[barTable[num4][n]].Width;
                    }

                    num2 += num4 * (m + 1);
                }

                break;
            }
            case CODE.C:
            {
                for (int k = 0; k < bar.Length / 2; k++)
                {
                    int num3 = short.Parse(bar.Substring(k * 2, 2));

                    for (int l = 0; l < 6; l++)
                    {
                        g.DrawLine(array2[barTable[num3][l]], num + array2[barTable[num3][l]].Width / 2f, top, num + array2[barTable[num3][l]].Width / 2f, top + height);
                        num += array2[barTable[num3][l]].Width;
                    }

                    num2 += num3 * (k + 1);
                }

                break;
            }
            }

            num2 %= 103;

            for (int num5 = 0; num5 < 6; num5++)
            {
                g.DrawLine(array2[barTable[num2][num5]], num + array2[barTable[num2][num5]].Width / 2f, top, num + array2[barTable[num2][num5]].Width / 2f, top + height);
                num += array2[barTable[num2][num5]].Width;
            }

            for (int num6 = 0; num6 < 7; num6++)
            {
                g.DrawLine(array2[barTable[106][num6]], num + array2[barTable[106][num6]].Width / 2f, top, num + array2[barTable[106][num6]].Width / 2f, top + height);
                num += array2[barTable[106][num6]].Width;
            }

            return true;
        }
    }
}
