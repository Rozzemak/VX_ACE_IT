using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VX_ACE_IT_CORE.Debug;
using VX_ACE_IT_CORE.MVC.Model.Async;
using VX_ACE_IT_CORE.MVC._Common;
using VX_ACE_IT_CORE.MVC.Model.GameProcess;

namespace VX_ACE_IT_CORE.MVC.Model.GameWindow
{
    public enum WindowStyles
    {
        Border,
        NoBorder,
    }

    class GameWindow : BaseAsync<object>
    {
        public static int GwlStyle = -16;
        public static int WsBorder = 0x00800000; //window with border
        public static int WsDlgframe = 0x00400000; //window with double border but no title
        public static int WsCaption = WsBorder | WsDlgframe; //window with a title bar
        public const uint WsSizebox = 0x00040000;

        private readonly Config _config;

        public GameWindow(BaseDebug debug, Config config, GameProcess.GameProcess gameProcess)
                    :base(debug, gameProcess)
        {
            _config = config;
        }

        public void SetWindowFromConfig()
        {
            AddWork(new Task<List<object>>(() =>
            {
                SetForegroundWindow(GameProcess.Process.MainWindowHandle);
                ShowWindowAsync(GameProcess.Process.MainWindowHandle, 9);
                SetWindowPos(GameProcess.Process.MainWindowHandle, new IntPtr(-2), 0, 0, _config.ConfigVariables.Width, _config.ConfigVariables.Height, 0);
                return null;
            }));

        }

        public void SetWindowStyle(int? style = null)
        {
            AddWork(new Task<List<object>>(() =>
            {
                IntPtr hwnd = GameProcess.Process.MainWindowHandle;
                SetForegroundWindow(hwnd);
                ShowWindowAsync(hwnd, 9);
                SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, style ?? -16) & ~WsCaption);
                return null;
            }));
        }


        public void SetWindowStyleBorder(int? style = null)
        {
            AddWork(new Task<List<object>>(() =>
            {
                IntPtr hwnd = GameProcess.Process.MainWindowHandle;
                SetForegroundWindow(hwnd);
                ShowWindowAsync(hwnd, 9);
                SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, style ?? -16) | WsCaption);
                return null;
            }));
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);

        [DllImport("USER32.DLL")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.DLL")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

    }
}
