using System.Configuration;

namespace EyeCenter
{
    /// <summary>
    /// App.config (ビルド後は EyeCenter.exe.config) の appSettings から設定値を読み込む。
    /// </summary>
    internal static class AppConfig
    {
        /// <summary>
        /// appSettings の値を int で取得する。未設定・空欄・不正な値の場合は def を返す。
        /// </summary>
        internal static int GetInt(string key, int def)
        {
            int v;
            string s = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrEmpty(s) && int.TryParse(s, out v) && v > 0)
            {
                return v;
            }

            return def;
        }

        /// <summary>
        /// appSettings の値を string で取得する。未設定・空欄の場合は def を返す。
        /// </summary>
        internal static string GetString(string key, string def)
        {
            string s = ConfigurationManager.AppSettings[key];

            if (!string.IsNullOrEmpty(s))
            {
                return s;
            }

            return def;
        }
    }
}
