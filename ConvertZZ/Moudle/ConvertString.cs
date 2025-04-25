using ConvertZZ.Enums;
using Fanhuaji_API;
using Fanhuaji_API.Class;
using Flier.Toolbox.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Fanhuaji_API.Fanhuaji;

namespace ConvertZZ.Moudle
{
    public class ConvertHelper
    {
        // opencc 轉換實例
        static OpenCCConverter convOpencc2Simp = new OpenCCConverter();
        static OpenCCConverter convOpencc2Trad = new OpenCCConverter();
        static string strOld2Simp, strOld2Trad;

        /// <summary>
        /// 轉換文字
        /// </summary>
        /// <param name="origin">原始文字</param>
        /// <param name="ToChinese">0: 不轉換 1: 簡體轉繁體 2:繁體轉簡體</param>
        /// <param name="VocabularyCorrection">-1: 依照設定值變動 0:不使用辭典轉換 1:使用辭典轉換</param>
        /// <returns></returns>
        public static async Task<string> ConvertAsync(string origin, int ToChinese, int VocabularyCorrection = -1)
        {
            if (String.IsNullOrWhiteSpace(origin)) return origin;
            if (App.Settings.Engine == Enum_Engine.Local && App.DictionaryStatus != Enum_DictionaryStatus.Loaded)
            {
                if (App.DictionaryStatus == Enum_DictionaryStatus.NotLoad || App.DictionaryStatus == Enum_DictionaryStatus.Error)
                    await App.LoadDictionary(Enum_Engine.Local);
                System.Threading.SpinWait.SpinUntil(() => App.DictionaryStatus == Enum_DictionaryStatus.Loaded, 30000);
                if (App.DictionaryStatus != Enum_DictionaryStatus.Loaded)
                    throw new Exception("詞彙修正的Dictionary載入失敗");
            }
            if ((App.Settings.VocabularyCorrection && VocabularyCorrection != 0) || VocabularyCorrection == 1)
            {
                if (App.Settings.Engine == Enum_Engine.Local)
                {
                    switch (ToChinese)
                    {
                        case 1:
                            origin = App.ChineseConverter.Convert(origin, C2T: true);
                            origin = App.ChineseConverter.ToTraditional(origin);
                            break;
                        case 2:
                            origin = App.ChineseConverter.Convert(origin, C2T: false);
                            origin = App.ChineseConverter.ToSimplified(origin);
                            break;
                    }
                    return origin;
                }

                // OpenCC 轉換引擎
                else if (App.Settings.Engine == Enum_Engine.OpenCC)
                {
                    // 不轉換的情況，單獨處理
                    if (ToChinese == 0)
                    {
                        return origin;
                    }

                    // 轉換結果
                    string strConverted;
                    
                    // 讀取配置文件路徑
                    string strJsonPath = ToChinese == 1 ?
                        App.Settings.OpenCC_Setting.PathJson_ToTrad : App.Settings.OpenCC_Setting.PathJson_ToSimp;

                    // 上一次的配置文件路徑
                    // 注意：此處的 string 變量雖然是引用，但是 string 是不可變的引用類型，修改 strOldPath 不會延續到 strOld2Simp 或者 strOld2Trad。
                    string strOldPath = ToChinese == 1 ? strOld2Trad : strOld2Simp;

                    // 本次應該使用的 OpenCC 實例
                    OpenCCConverter convOpenCC = ToChinese == 1 ? convOpencc2Trad : convOpencc2Simp;

                    if (strJsonPath == null)
                    {
                        MessageBox.Show("OpenCC 配置文件爲 null", "Warning");
                    }

                    // 配置文件若有更新，則重新打開 OpenCC
                    // 由於 old_path 初始爲空串，因此開啓程序後第一次執行簡繁轉換，也會出發 OpenCC 在此處開啓
                    if (strOldPath != strJsonPath)
                    {
                        // 已開啓 opencc 實例的情況下將其關閉
                        if (convOpenCC.IsOpen() && convOpenCC.Close() != 0)
                        {
                            MessageBox.Show(convOpenCC.Error(), "OpenCC Close Error");
                        }

                        // 開啓 OpenCC
                        if (!convOpenCC.Open(strJsonPath))
                        {
                            MessageBox.Show(convOpenCC.Error(), "OpenCC Open Error");
                        }

                        // 更新 OldPath
                        if (ToChinese == 1)     strOld2Trad = strJsonPath;
                        else                    strOld2Simp = strJsonPath;
                    }

                    // 實際進行轉換
                    if (!convOpenCC.Convert(origin, out strConverted))
                    {
                        MessageBox.Show(convOpenCC.Error(), "OpenCC Convert Error");
                    }
                    
                    return strConverted;
                }
                else if (App.Settings.Engine == Enum_Engine.Fanhuaji)
                {
                    if (ToChinese != 0)
                    {
                        if (!Fanhuaji.CheckConnection())
                        {
                            throw new FanhuajiException("無法連線至繁化姬，請確認連線狀態");
                        }
                        Callback callback = await App.Fanhuaji.ConvertAsync(origin, (ToChinese == 1) ? App.Settings.Fanhuaji_Setting.Converter_S_to_T : App.Settings.Fanhuaji_Setting.Converter_T_to_S, (Config)App.Settings.Fanhuaji_Setting);
                        if (callback.Code != 0)
                        {
                            throw new FanhuajiException("使用繁化姬時出現一些意料外的錯誤");
                        }
                        origin = callback.Data.Text;
                    }
                }
            }
            else
            {
                switch (ToChinese)
                {
                    case 1:
                        origin = App.ChineseConverter.ToTraditional(origin);
                        break;
                    case 2:
                        origin = App.ChineseConverter.ToSimplified(origin);
                        break;
                }
            }
            return origin;
        }
        /// <summary>
        /// 轉換文字
        /// </summary>
        /// <param name="origin">原始文字</param>
        /// <param name="encoding">encoding[0]:來源編碼  encoding[1]:目標編碼</param>
        /// <param name="ToChinese">0: 不轉換 1: 簡體轉繁體 2:繁體轉簡體</param>
        /// <param name="VocabularyCorrection">-1: 依照設定值變動 0:不使用辭典轉換 1:使用辭典轉換</param>
        /// <returns></returns>
        public static async Task<string> ConvertAsync(string origin, Encoding[] encoding, int ToChinese, int VocabularyCorrection = -1)
        {
            if (String.IsNullOrWhiteSpace(origin)) return origin;
            switch (ToChinese)
            {
                case 1:
                    return await ConvertAsync(encoding[0].GetString(encoding[1].GetBytes(origin)), ToChinese);
                case 2:
                default:
                    return encoding[0].GetString(encoding[1].GetBytes(await ConvertAsync(origin, ToChinese, VocabularyCorrection)));
            }
        }
        public static async Task<Dictionary<string, string>> ConvertDictionary(Dictionary<string, string> PathParts, int ToChinese, int VocabularyCorrection = -1)
        {
            return await ConvertDictionary(PathParts, new Encoding[] { Encoding.UTF8, Encoding.UTF8 }, ToChinese, VocabularyCorrection);
        }
        public static async Task<Dictionary<string, string>> ConvertDictionary(Dictionary<string, string> PathParts, Encoding[] encoding, int ToChinese, int VocabularyCorrection = -1)
        {
            PathParts = new Dictionary<string, string>(PathParts);
            List<string> list_Input, list_Output;
            list_Input = new List<string>(PathParts.Keys.ToList());
            string input = string.Join("\r\n", PathParts.Values.ToList());
            string output;
            try
            {
                output = await ConvertAsync(input, encoding, ToChinese, VocabularyCorrection);
            }
            catch
            {
                throw;
            }
            list_Output = output.Split(new string[] { "\r\n" }, StringSplitOptions.None).ToList();
            if (list_Input.Count == list_Output.Count && list_Input.Count == PathParts.Count || input == "")
            {
                for (int i = 0; i < list_Input.Count; i++)
                    PathParts[list_Input[i]] = list_Output[i];
            }
            else
            {
                throw new Exception("使用詞彙修正時出現錯誤，請勿使用有換行符號的字典");
            }
            return PathParts;
        }

        public static async Task<string> FileConvert(string origin, Encoding[] encoding, int ToChinese, int VocabularyCorrection = -1)
        {
            if (ToChinese == 0)
            {
                //經研究，ConvertZ在轉檔案時，做了一些小動作，他會先把原本big5顯示不出來的字轉成繁體，證據是'软'都變成'軟'了
                StringBuilder sb = new StringBuilder(origin.Length);
                foreach (char c in origin.ToCharArray())
                    if (encoding[1].GetChars(encoding[1].GetBytes(new char[] { c }))[0] != c)
                    {
                        sb.Append(App.ChineseConverter.ToTraditional(new String(c, 1)));
                    }
                    else
                        sb.Append(c);
                origin = sb.ToString();
            }
            if (encoding[1] == Encoding.Default || encoding[1] == Encoding.UTF8 || encoding[1] == Encoding.Unicode || encoding[1] == Encoding.GetEncoding("UnicodeFFFE") || encoding[0] == encoding[1])
                return await ConvertAsync(origin, ToChinese, VocabularyCorrection);
            else
                return await ConvertAsync(origin, new Encoding[2] { Encoding.Default, encoding[1] }, ToChinese, VocabularyCorrection);
        }

        /// <summary>
        /// 進行標點符號轉換
        /// </summary>
        /// <param name="origin">來源字串</param>
        /// <param name="mode">0: 半形轉全形 ; 1:全形轉半形</param>
        /// <returns></returns>
        public static string ConvertSymbol(string origin, int mode)
        {
            FastReplace fastReplace = new FastReplace(mode == 0 ? SymbolTable : (SymbolTable.ToLookup(pair => pair.Value, pair => pair.Key).ToDictionary(grp => grp.Key, grp => grp.ToArray()[0])));
            return fastReplace.ReplaceAll(origin);
        }
        private static Dictionary<string, string> SymbolTable = new Dictionary<string, string>()
        {
            { "," , "，" },
            { "~" , "～" },
            { "!" , "！" },
            { "#" , "＃" },
            { "$" , "＄" },
            { "%" , "％" },
            { "^" , "︿" },
            { "&" , "＆" },
            { "*" , "＊" },
            { "-" , "－" },
            { "+" , "＋" },
            { "{" , "｛" },
            { "}" , "｝" },
            { ";" , "；" },
            { "|" , "｜" },
            { "?" , "？" },
            { "(" , "（" },
            { ")" , "）" },
            { "“" , "「" },
            { "”" , "」" },
            { "‘" , "『" },
            { "’" , "』" },
            { "[" , "［" },
            { "]" , "］" },
            //{ "·" , "．" },
            { " " , "　" },

            { ":" , "：" },
            { "." , "。" },
            { "\"" , "、" },
            { "@" , "＠" },
            { "<" , "＜" },
            { ">" , "＞" },
            { "=" , "＝" },
        };
    }
}
