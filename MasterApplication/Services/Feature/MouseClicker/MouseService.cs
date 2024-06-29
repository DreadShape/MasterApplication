using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using MasterApplication.Models;
using MasterApplication.Models.Enums;
using MasterApplication.Models.Structs;

namespace MasterApplication.Services.Feature.MouseClicker;

/// <summary>
/// Implementation of the <see cref="IMouseService"/>.
/// </summary>
public class MouseService : IMouseService
{
    #region ClickLeftMouseButton

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)] public MOUSEINPUT Mouse;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int X;
        public int Y;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    private const uint INPUT_MOUSE = 0;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_MOVE = 0x0001;
    private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, [In] INPUT[] pInputs, int cbSize);

    public void MoveCursorTo(int x, int y)
    {
        INPUT input = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new InputUnion
            {
                Mouse = new MOUSEINPUT
                {
                    X = (x * 65535) / (int)SystemParameters.PrimaryScreenWidth,
                    Y = (y * 65535) / (int)SystemParameters.PrimaryScreenHeight,
                    DwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
    }

    public void ClickLeftMouseButton()
    {
        INPUT[] inputs = new INPUT[2];

        inputs[0] = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new InputUnion
            {
                Mouse = new MOUSEINPUT
                {
                    DwFlags = MOUSEEVENTF_LEFTDOWN,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        inputs[1] = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new InputUnion
            {
                Mouse = new MOUSEINPUT
                {
                    DwFlags = MOUSEEVENTF_LEFTUP,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }

    #endregion

    #region GetMousePos

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out MouseCoordinate lpPoint);

    public MouseCoordinate GetMousePos()
    {
        if (GetCursorPos(out MouseCoordinate lpPoint))
            return lpPoint;

        return new MouseCoordinate(0, 0);
    }

    #endregion

    #region MouseHook

    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;

    private readonly LowLevelMouseProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    public event EventHandler<MouseButtonEventArgs>? MouseButtonClicked;

    public MouseService()
    {
        _proc = HookCallback;
    }

    public void StartMouseHook()
    {
        if (_hookID == IntPtr.Zero)
        {
            _hookID = SetHook(_proc);
        }
    }

    public void StopMouseHook()
    {
        if (_hookID != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_hookID);
            _hookID = IntPtr.Zero;
        }
    }

    private static IntPtr SetHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            if (curModule != null)
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);

            return IntPtr.Zero;
        }
    }

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            MouseButton button = MouseButton.None;
            if (wParam == (IntPtr)WM_LBUTTONDOWN)
            {
                button = MouseButton.Left;
            }
            else if (wParam == (IntPtr)WM_RBUTTONDOWN)
            {
                button = MouseButton.Right;
            }
            else if (wParam == (IntPtr)WM_MBUTTONDOWN)
            {
                button = MouseButton.Middle;
            }

            if (button != MouseButton.None)
            {
                MSLLHOOKSTRUCT hookStruct = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
                MouseCoordinate coordinate = new MouseCoordinate(hookStruct.pt.X, hookStruct.pt.Y);
                MouseButtonClicked?.Invoke(this, new MouseButtonEventArgs(button, coordinate));
            }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct MSLLHOOKSTRUCT
    {
        public MouseCoordinate pt;
        public uint mouseData;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    #endregion
}
