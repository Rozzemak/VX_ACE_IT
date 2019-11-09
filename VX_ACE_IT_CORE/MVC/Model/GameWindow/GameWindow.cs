using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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
        NoBorder,
        Border,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MOUSEINPUT
    {
        int dx;
        int dy;
        uint mouseData;
        uint dwFlags;
        uint time;
        IntPtr dwExtraInfo;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HARDWAREINPUT
    {
        uint uMsg;
        ushort wParamL;
        ushort wParamH;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct INPUT
    {
        public uint Type;
        public MOUSEKEYBDHARDWAREINPUT Data;
    }

    /// <summary>
    /// http://social.msdn.microsoft.com/Forums/en/csharplanguage/thread/f0e82d6e-4999-4d22-b3d3-32b25f61fb2a
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;
    }


    internal enum INPUT_TYPE : uint
    {
        INPUT_MOUSE = 0,
        INPUT_KEYBOARD = 1,
        INPUT_HARDWARE = 2
    }

    class GameWindow : BaseAsync<object>
    {
        public static int GwlStyle = -16;
        public static int WsBorder = 0x00800000; //window with border
        public static int WsDlgframe = 0x00400000; //window with double border but no title
        public static int WsCaption = WsBorder | WsDlgframe; //window with a title bar
        public const uint WsSizebox = 0x00040000;
        public const int WsMaximize = 0x01000000;
        public const uint WsPopup = 0x80000000;

        private readonly Config _config;

        public GameWindow(BaseDebug debug, Config config, GameProcess.GameProcess gameProcess)
                    : base(debug, gameProcess)
        {
            _config = config;
        }

        public void SetWindowFromConfig()
        {
            AddWork(new Task<List<object>>(() =>
            {
                OnIconicRestore();
                ShowWindowAsync(GameProcess.Process.MainWindowHandle, 5);
                SetWindowPos(GameProcess.Process.MainWindowHandle, new IntPtr(-2), 0, 0, _config.ConfigVariables.Width, _config.ConfigVariables.Height, 0);
                SetForegroundWindow(GameProcess.Process.MainWindowHandle);
                return new List<object>(){};
            }));

        }

        public void SetWindowStyle(int? style = null)
        {
            AddWork(new Task<List<object>>(() =>
            {
                OnIconicRestore();
                var hwnd = GameProcess.Process.MainWindowHandle;
                ShowWindowAsync(hwnd, 5);
                SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, style ?? -16) & ~WsCaption);
                SetForegroundWindow(hwnd);
                return new List<object>(){};
            }));
        }


        public void SetWindowStyleBorder(int? style = null)
        {
            AddWork(new Task<List<object>>(() =>
            {
                var hwnd = GameProcess.Process.MainWindowHandle;
                ShowWindowAsync(hwnd, 5);
                SetWindowLong(hwnd, GwlStyle, GetWindowLong(hwnd, style ?? -16) | WsCaption);
                SetForegroundWindow(hwnd);
                return new List<object>(){};
            }));
        }

        public void OnIconicRestore(int sleep = 300)
        {
            if (IsIconic(GameProcess.Process.MainWindowHandle))
            {
                //ShowWindowAsync(hwnd, 11); // Minimize, caution -> mc different thread reserver flag. Use 6 if doesnt work.
                //Thread.Sleep(sleep);
                ShowWindowAsync(GameProcess.Process.MainWindowHandle, 9); // Restore
            }
            if(IsWindowFullscreen()) ExitFullscreen();
        }

        public void ExitFullscreen()
        {
            SimulateKeyPress(new List<int>(){ 0x12, 0x0D }); // 0x12 ALT , 0x0D ENTER
        }

        public bool IsWindowFullscreen()
        {
            var style = GetWindowLong(GameProcess.Process.MainWindowHandle, GwlStyle);
            if ((style & WsPopup) != 0)
            {
                //It's maximized
                Debug.AddMessage<object>(new Message<object>("Window is fullscreen"));
                return true;
            }

            Debug.AddMessage<object>(new Message<object>("Window is not fullscreen"));
            return false;
        }

        public void SimulateKeyPress(List<int> vkInputs, bool release = false)
        {
            AddWork(new Task<List<object>>((() =>
            {
                SwitchWindow(GameProcess.Process.MainWindowHandle);
                var inputs = new INPUT[vkInputs.Count];

                foreach (var vkey in vkInputs)
                {
                    //inputs[vkInputs.IndexOf(vkey)].type = (int)INPUT_TYPE.INPUT_KEYBOARD;
                    //inputs[vkInputs.IndexOf(vkey)].ki.dwFlags = 0;
                    //inputs[vkInputs.IndexOf(vkey)].ki.wScan = (ushort)(vkey & 0xff);
                    var input = new INPUT
                    {
                        Type = 1
                    };
                    input.Data.Keyboard = new KEYBDINPUT()
                    {

                        wVk = (ushort)vkey,
                        wScan = 0,
                        dwFlags = release ? (uint)0x0002 : 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero,
                    };
                    inputs[vkInputs.IndexOf(vkey)] = input;
                }

                var intReturn = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
                if (intReturn == 0)
                {
                    var str = "";
                    foreach (var input in vkInputs)
                    {
                        str += "[0x" + input.ToString("X") + "]";
                    }

                    throw new Exception("Could not send key: {" + str + "}");
                }
                else
                {
                    var str = "";
                    foreach (var input in vkInputs)
                    {
                        str += "[0x" + input.ToString("X") + "]";
                    }

                    Debug.AddMessage<object>(new Message<object>("Input VK:{" + str + "} send."));
                    SwitchWindow(GameProcess.Process.MainWindowHandle);
                    SetWindowFromConfig();
                    if (!release) SimulateKeyPress(vkInputs, true);
                    return null;
                }
            })));
           
        }


        public static void SwitchWindow(IntPtr windowHandle)
        {
            if (GetForegroundWindow() == windowHandle)
                return;

            var foregroundWindowHandle = GetForegroundWindow();
            var currentThreadId = GetCurrentThreadId();
            var foregroundThreadId = GetWindowThreadProcessId(foregroundWindowHandle, out var temp);
            AttachThreadInput(currentThreadId, foregroundThreadId, true);
            SetForegroundWindow(windowHandle);
            AttachThreadInput(currentThreadId, foregroundThreadId, false);

            while (GetForegroundWindow() != windowHandle)
            {
            }

        }


        /// <summary>
        /// I use this specificaly to identify fullscreen/minimized windows.
        /// </summary>
        /// <param name="hWnd"></param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsIconic(IntPtr hWnd);

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

        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern UInt32 SendInput(UInt32 nInputs, [MarshalAs(UnmanagedType.LPArray, SizeConst = 1)] INPUT[] pInputs, Int32 cbSize);

    }
}
