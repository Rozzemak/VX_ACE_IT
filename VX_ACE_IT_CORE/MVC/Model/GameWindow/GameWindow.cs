using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using VX_ACE_IT_CORE.MVC._Common;

namespace VX_ACE_IT_CORE.MVC.Model.GameWindow
{
    public enum WindowStyles
    {
        Border,
        NoBorder,
    }

    class GameWindow
    {
        public static int GwlStyle = -16;
        public static int WsBorder = 0x00800000; //window with border
        public static int WsDlgframe = 0x00400000; //window with double border but no title
        public static int WsCaption = WsBorder | WsDlgframe; //window with a title bar
        public const uint WsSizebox = 0x00040000;


        private readonly Config _config;
        private readonly GameProcess.GameProcess _gameProcess;

        public GameWindow(GameProcess.GameProcess gameProcess, Config config)
        {
            _gameProcess = gameProcess;
            _config = config;
        }
            
        public void SetWindowFromConfig()
        {
            SetForegroundWindow(_gameProcess.Process.MainWindowHandle);
            ShowWindowAsync(_gameProcess.Process.MainWindowHandle, 9);
            SetWindowPos(_gameProcess.Process.MainWindowHandle, new IntPtr(-2), 0, 0, _config.ConfigVariables.Width, _config.ConfigVariables.Height, 0);
        }

        public void SetWindowStyle(int? style = null)
        {
            IntPtr hwnd = _gameProcess.Process.MainWindowHandle;
            SetForegroundWindow(hwnd);
            ShowWindowAsync(hwnd, 9);
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, style ?? -16) & ~WsCaption);
        }


        public void SetWindowStyleBorder(int? style = null)
        {
            IntPtr hwnd = _gameProcess.Process.MainWindowHandle;
            SetForegroundWindow(hwnd);
            ShowWindowAsync(hwnd, 9);
            SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, style ?? -16) | WsCaption);
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
