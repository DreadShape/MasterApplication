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
    #region MoveAndClickLeftMouseButton

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

    /// <summary>
    /// Moves the mouse cursor to the specified coordinates.
    /// </summary>
    /// <param name="x">Horizontal coordinate.</param>
    /// <param name="y">Vertical coordinate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the one or both of the mouse coordinates are negative values.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the mouse cannot move to the specified coordinates.</exception>
    public void MoveCursorTo(int x, int y)
    {
        // Check for negative coordinates
        if (x < 0 || y < 0)
            throw new ArgumentOutOfRangeException($"Coordinates cannot be negative. x: {x}, y: {y}");

        // Scale the coordinates for absolute mouse movement
        uint scaledX = (uint)((x * 65536.0) / SystemParameters.PrimaryScreenWidth);

        //I don't know why but the scaledY always results in 1 less pixel on the screen, so we add it at the end of the calculation to adjust for that.
        uint scaledY = ((uint)((y * 65536.0) / SystemParameters.PrimaryScreenHeight)) + 1;

        INPUT input = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new InputUnion
            {
                Mouse = new MOUSEINPUT
                {
                    X = (int)scaledX,
                    Y = (int)scaledY,
                    DwFlags = MOUSEEVENTF_MOVE | MOUSEEVENTF_ABSOLUTE,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        uint result = SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));

        if (result == 0)
        {
            // If SendInput fails, retrieve the last error code
            int errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to move cursor. Error Code: {errorCode}");
        }
    }

    /// <summary>
    /// Clicks the left click mouse button on the current cursor's position.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the mouse cannot click the button on the current position.</exception>
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

        uint result = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));

        if (result == 0)
        {
            // If SendInput fails, retrieve the last error code
            int errorCode = Marshal.GetLastWin32Error();
            throw new InvalidOperationException($"Failed to simulate left mouse click. Error Code: {errorCode}");
        }
    }

    #endregion

    #region GetMousePos

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetCursorPos(out MouseCoordinate lpPoint);

    /// <summary>
    /// Gets the current mouse position as a <see cref="MouseCoordinate"/> struct.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when it fails to get the mouse position.</exception>
    public MouseCoordinate GetMousePos()
    {
        if (GetCursorPos(out MouseCoordinate lpPoint))
            return lpPoint;

        // Throw an exception if the API call fails
        throw new InvalidOperationException("Failed to get mouse position.");
    }

    #endregion

    #region MouseHook

    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;

    private readonly LowLevelMouseProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    public event EventHandler<AutoClickerMouseButtonEventArgs>? MouseButtonClicked;

    public MouseService()
    {
        _proc = HookCallback;
    }

    /// <summary>
    /// Hooks to the mouse to be able to use the click buttons.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call the method again while already being hooked.</exception>
    public void StartMouseHook()
    {
        if (_hookID != IntPtr.Zero)
            throw new InvalidOperationException("Mouse hook is already active.");

        _hookID = SetHook(_proc);
    }

    /// <summary>
    /// Unhooks from the mouse.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call this method while not being hooked.</exception>
    public void StopMouseHook()
    {
        if (_hookID == IntPtr.Zero)
            throw new InvalidOperationException("Mouse hook is not currently active.");

        UnhookWindowsHookEx(_hookID);
        _hookID = IntPtr.Zero;
    }

    /// <summary>
    /// Tells us if the mouse is hooked.
    /// </summary>
    /// <returns></returns>
    public bool IsMouseHookAttached()
    {
        return _hookID != 0;
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
                MouseButtonClicked?.Invoke(this, new AutoClickerMouseButtonEventArgs(button, coordinate));
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
