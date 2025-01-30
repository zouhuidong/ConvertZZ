using ConvertZZ.Moudle;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ConvertZZ
{
    public class ClipBoardHelper
    {
        public static string GetClipBoard(TextDataFormat format)
        {
            string idat = null;
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        idat = Clipboard.GetText(format);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            return idat;
        }

        /// <summary>
        /// 這個函數僅能單獨設置某一種類型的剪切板內容，不能做到 CF_HTML 和 CF_TEXT 共存
        /// </summary>
        /// <param name="str">將設置的內容字符串</param>
        /// <param name="format">設置內容類型</param>
        public static void SetClipBoard(string str, TextDataFormat format)
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        Clipboard.SetText(str, format);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }

        /// <summary>
        /// 匯東新增函數：同時設置 CF_HTML 和 CF_TEXT 類型的剪切板內容
        /// </summary>
        /// <param name="strUnitext">Unicode Text 內容</param>
        /// <param name="strHtml">HTML 內容</param>
        public static void SetClipBoard_Html(string strUnitext, string strHtml)
        {
            Exception threadEx = null;
            Thread staThread = new Thread(
                delegate ()
                {
                    try
                    {
                        // 创建一个 DataObject 对象
                        DataObject dataObject = new DataObject();
                        dataObject.SetData(DataFormats.Html, strHtml);
                        //dataObject.SetData(DataFormats.Text, Encoding.ASCII.GetBytes(strUnitext).ToString());
                        dataObject.SetData(DataFormats.UnicodeText, strUnitext);

                        // 将 DataObject 设置到剪贴板
                        Clipboard.SetDataObject(dataObject, true);
                    }

                    catch (Exception ex)
                    {
                        threadEx = ex;
                    }
                });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }

        internal static void Copy()
        {
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(VK_C, 0, 0, 0);
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
        internal static void Copy(Key key, KeyModifier keyModifiers)
        {
            HotKey_KeyUp(key, keyModifiers);
            Copy();
        }
        internal static void Paste()
        {
            keybd_event(VK_CONTROL, 0, 0, 0);
            keybd_event(VK_V, 0, 0, 0);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
        }
        internal static void Paste(Key key, KeyModifier keyModifiers)
        {
            HotKey_KeyUp(key, keyModifiers);
            Paste();
        }


        private static void HotKey_KeyUp(Key key, KeyModifier keyModifiers)
        {
            System.Windows.Forms.Keys k = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(key);
            keybd_event((byte)k, 0, KEYEVENTF_KEYUP, 0);
            var array = new System.Collections.BitArray(new int[] { (int)keyModifiers });
            System.Windows.Forms.Keys m;
            if (array[0])
            {
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.LeftAlt);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.RightAlt);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
            }
            if (array[1])
            {
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.LeftCtrl);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.RightCtrl);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
            }
            if (array[2])
            {
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.LeftShift);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.RightShift);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
            }
            if (array[3])
            {
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.LWin);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
                m = (System.Windows.Forms.Keys)KeyInterop.VirtualKeyFromKey(Key.RWin);
                keybd_event((byte)m, 0, KEYEVENTF_KEYUP, 0);
            }
        }
        #region Win32
        static readonly uint KEYEVENTF_KEYUP = 2;
        static readonly byte VK_CONTROL = 0x11;
        static readonly byte VK_C = 0x43;
        static readonly byte VK_V = 0x56;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);
        #endregion
    }
}
