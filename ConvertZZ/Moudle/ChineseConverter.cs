using ConvertZZ.Moudle;
using Flier.Toolbox.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ConvertZZ
{
    public class ChineseConverter
    {
        #region OS的轉換
        internal const int LOCALE_SYSTEM_DEFAULT = 0x0800;
        internal const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        internal const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;

        /// <summary> 
        /// 使用OS的kernel.dll做為簡繁轉換工具，只要有裝OS就可以使用，不用額外引用dll，但只能做逐字轉換，無法進行詞意的轉換 
        /// <para>所以無法將電腦轉成計算機</para> 
        /// </summary> 
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapString(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest);


        /// <summary> 
        /// 使用OS的kernel.dll做為簡繁轉換工具，只要有裝OS就可以使用，不用額外引用dll，但只能做逐字轉換，無法進行詞意的轉換 
        /// <para>所以無法將電腦轉成計算機</para> 
        /// </summary> 
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern int LCMapStringEx(int Locale, int dwMapFlags, string lpSrcStr, int cchSrc, [Out] string lpDestStr, int cchDest, int lpVersionInformation = 0, int lpReserved = 0, int sortHandle = 0);

        /// <summary> 
        /// 繁體轉簡體 
        /// </summary> 
        /// <param name="pSource">要轉換的繁體字：體</param> 
        /// <returns>轉換後的簡體字：體</returns> 
        public string ToSimplified(string pSource)
        {
            pSource = FR_Reserved.ReplaceAll(pSource);
            String tTarget = new String(' ', pSource.Length);
            int tReturn = LCMapStringEx(LOCALE_SYSTEM_DEFAULT, LCMAP_SIMPLIFIED_CHINESE, pSource, pSource.Length, tTarget, pSource.Length);
            tTarget = FRRevert_Reserved.ReplaceAll(tTarget);
            return tTarget;
        }

        /// <summary> 
        /// 簡體轉繁體 
        /// </summary> 
        /// <param name="pSource">要轉換的繁體字：體</param> 
        /// <returns>轉換後的簡體字：體</returns> 
        public string ToTraditional(string pSource)
        {
            pSource = FR_Reserved.ReplaceAll(pSource);
            String tTarget = new String(' ', pSource.Length);
            int tReturn = LCMapStringEx(LOCALE_SYSTEM_DEFAULT, LCMAP_TRADITIONAL_CHINESE, pSource, pSource.Length, tTarget, pSource.Length);
            tTarget = FRRevert_Reserved.ReplaceAll(tTarget);
            return tTarget;
        }

        #endregion OS的轉換

        internal List<DictionaryFile_Helper.Line> Lines { get; set; } = new List<DictionaryFile_Helper.Line>();
        FastReplace FR = null, FRRevert = null;
        Dictionary<string, string> ReservedWordTable = new Dictionary<string, string>();
        Dictionary<string, string> ReservedWordTable_Revert = new Dictionary<string, string>();
        public FastReplace FR_Reserved = new FastReplace(new Dictionary<string, string>()), FRRevert_Reserved = new FastReplace(new Dictionary<string, string>());
        public ChineseConverter()
        {
        }
        public async Task Load(string fileName)
        {
            Lines.Clear();
            Lines.AddRange(await DictionaryFile_Helper.Load(fileName));
            Reload();
        }
        public void Reload()
        {
            var lines = Lines.ToLookup(x => x.SimplifiedChinese).Select(coll => coll.First()).ToList();
            FR = new FastReplace(lines.Where(x => x.Enable).OrderByDescending(x => x.SimplifiedChinese_Priority).ThenByDescending(x => x.SimplifiedChinese.Length).ToDictionary(x => x.SimplifiedChinese, x => x.TraditionalChinese));

            lines = Lines.ToLookup(x => x.TraditionalChinese).Select(coll => coll.First()).ToList();
            FRRevert = new FastReplace(lines.Where(x => x.Enable).OrderByDescending(x => x.TraditionalChinese_Priority).ThenByDescending(x => x.TraditionalChinese.Length).ToDictionary(x => x.TraditionalChinese, x => x.SimplifiedChinese));
            ReservedWordTable.Clear();
            ReservedWordTable_Revert.Clear();
            lines.Where(x => x.Enable).Where(x => x.SimplifiedChinese_Priority == 9999 && x.TraditionalChinese_Priority == 9999 && x.SimplifiedChinese == x.TraditionalChinese).ToList().ForEach(x => InsertTo_ReservedWordTable(x.SimplifiedChinese));
            FR_Reserved = new FastReplace(ReservedWordTable);
            FRRevert_Reserved = new FastReplace(ReservedWordTable_Revert);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="C2T">True:簡體轉繁體  False:繁體轉簡體</param>
        /// <returns></returns>
        public string Convert(string input, bool C2T)
        {
            //這個方法最快
            if (C2T)
                return FR.ReplaceAll(input);
            else
                return FRRevert.ReplaceAll(input);
            /* 第二快
            foreach (var temp in _dictionary)
            {
                input = input.Replace(temp.Key, temp.Value);
            }
            return input;*/
            /* 最慢
            StringBuilder sb = new StringBuilder(input);
            foreach(var temp in _dictionary)
            {
                sb.Replace(temp.Key, temp.Value);                    
            }
            return input;*/
        }
        private void InsertTo_ReservedWordTable(string str)
        {
            string temp;
            do
            {
                temp = $"❂{Guid.NewGuid().ToString()}❂";
            }
            while (ReservedWordTable.ContainsKey(temp));
            ReservedWordTable.Add(str, temp);
            ReservedWordTable_Revert.Add(temp, str);
        }
    }


    public class OpenCCConverter
    {
        // 標識一個 opencc 實例的指針
        private IntPtr ptrOpencc = IntPtr.Zero;

        /// <summary>
        /// OpenCC 簡繁轉換接口
        /// 匯東製作
        /// </summary>
        #region OpenCC 的轉換
        [DllImport("opencc")]
        private static extern IntPtr opencc_open(IntPtr configFileName);

        [DllImport("opencc")]
        private static extern int opencc_close(IntPtr opencc);

        [DllImport("opencc")]
        private static extern IntPtr opencc_convert_utf8(IntPtr opencc, IntPtr input, Int64 length);

        [DllImport("opencc")]
        private static extern void opencc_convert_utf8_free(IntPtr str);

        [DllImport("opencc")]
        private static extern IntPtr opencc_error();

        /// <summary>
        /// 開啓 OpenCC
        /// </summary>
        /// <param name="configFileName">OpenCC 配置文件名（例如 "s2t.json"）</param>
        /// <returns>是否成功開啓 OpenCC。如果開啓 OpenCC 失敗，請使用 Error 函數獲取錯誤信息。</returns>
        public Boolean Open(string configFileName)
        {
            // open
            IntPtr pStr_configfile = Marshal.StringToHGlobalAnsi(configFileName);
            try
            {
                ptrOpencc = opencc_open(pStr_configfile);
            }
            finally
            {
                Marshal.FreeHGlobal(pStr_configfile);
            }

            return IsOpen();
        }

        /// <summary>
        /// 判斷 OpenCC 是否已經開啓
        /// </summary>
        /// <returns>已經開啓返回 True，否則返回 False</returns>
        public Boolean IsOpen()
        {
            /* OpenCC 實際上規定 Open 失敗時返回的是 (void*)-1 */
            return ptrOpencc != IntPtr.Zero && ptrOpencc != IntPtr.Zero - 1;
        }

        /// <summary>
        /// 銷燬 OpenCC 實例
        /// </summary>
        /// <returns>0 表示銷燬成功，非 0 表示銷燬失敗。若失敗，請使用 Error 函數獲取錯誤信息</returns>
        public int Close()
        {
            return opencc_close(ptrOpencc);
        }

        /// <summary>
        /// 進行 OpenCC 轉換
        /// </summary>
        /// <param name="input">待轉換字符串</param>
        /// <param name="output">返回轉換完畢的字符串</param>
        /// <returns>轉換是否成功。若失敗，請使用 Error 函數獲取錯誤信息</returns>
        public bool Convert(string input, out string output)
        {
            // convert
            // 首先將輸入字符串轉爲 UTF-8 編碼的字符串
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(input);
            IntPtr pStr_input = Marshal.AllocHGlobal(utf8Bytes.Length + 1);
            IntPtr pStr_converted;
            try
            {
                Marshal.Copy(utf8Bytes, 0, pStr_input, utf8Bytes.Length);
                Marshal.WriteByte(pStr_input + utf8Bytes.Length, 0);    // 手動添加末尾 '\0'
                pStr_converted = opencc_convert_utf8(ptrOpencc, pStr_input, utf8Bytes.Length);
            }
            finally
            {
                Marshal.FreeHGlobal(pStr_input);
            }
            if (pStr_converted == IntPtr.Zero)
            {
                output = "";
                return false;
            }

            // 注意：由於此項目 .Net 版本過低，因此使用下面的替代方法進行 UTF-8 轉換
            // output = Marshal.PtrToStringUTF8(pStr_converted);
            output = PtrToStringUTF8Alternative(pStr_converted);
            opencc_convert_utf8_free(pStr_converted);
            return true;
        }

        /// <summary>
        /// 獲取 OpenCC 的最後一個錯誤
        /// 注意：此函數是唯一一個非線程安全的函數
        /// </summary>
        /// <returns>錯誤信息</returns>
        public string Error()
        {
            return Marshal.PtrToStringAnsi(opencc_error());
        }

        #endregion OpenCC 的轉換

        // 此函數用於在低版本 .Net 中替代 Marshal.PtrToStringUTF8 方法
        static string PtrToStringUTF8Alternative(IntPtr ptr)
        {
            int i;
            for (i = 0; Marshal.ReadByte(ptr, i) != 0; i++) ;
            byte[] realBytes = new byte[i];
            Marshal.Copy(ptr, realBytes, 0, i);
            return Encoding.UTF8.GetString(realBytes);
        }
    }

}
