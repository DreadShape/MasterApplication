﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace MasterApplication.Services.Feature.MouseClicker;

/// <summary>
/// Implementation of the <see cref="IKeyboardService"/>.
/// </summary>
public class KeyboardService : IKeyboardService
{
    public event EventHandler<int>? KeyPressed;

    public KeyboardService()
    {
        _proc = HookCallback;
    }

    ~KeyboardService()
    {
        StopKeyboardHook();
    }

    #region GetKeyByCode

    public string GetKeyByCode(int vkCode)
    {
        var keyState = new byte[256];
        GetKeyboardState(keyState);

        uint scanCode = (uint)MapVirtualKey((uint)vkCode, MAPVK_VK_TO_VSC);
        var stringBuilder = new StringBuilder(2);

        int result = ToUnicode((uint)vkCode, scanCode, keyState, stringBuilder, stringBuilder.Capacity, 0);
        if (result > 0)
        {
            return stringBuilder.ToString();
        }

        return ((Key)vkCode).ToString();

        //throw new InvalidOperationException($"Invalid key code. No key is assigned to '{vkCode}'.");
    }

    /// <summary>
    /// Tells us if the keyboard hook is listening to key presses or not.
    /// </summary>
    /// <returns></returns>
    public bool IsKeyboardHookAttached()
    {
        return _isHooked;
    }

    [DllImport("user32.dll")]
    private static extern int MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ToUnicode(
        uint wVirtKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)] StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags);

    [DllImport("user32.dll")]
    private static extern bool GetKeyboardState(byte[] lpKeyState);

    #endregion

    #region KeyboardHook

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const uint MAPVK_VK_TO_VSC = 0x00;

    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;
    private bool _isHooked;

    /// <summary>
    /// Hooks to the keyboard to be able to use the keys.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call the method again while already being hooked.</exception>
    public void StartKeyboardHook()
    {
        if (_isHooked)
            throw new InvalidOperationException("Keyboard hook is already active.");

        _hookID = SetHook(_proc);
        _isHooked = true;
    }

    /// <summary>
    /// Hooks from the keyboard.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call this method while not being hooked.</exception>
    public void StopKeyboardHook()
    {
        if (!_isHooked)
            throw new InvalidOperationException("Keyboard hook is not currently active.");

        UnhookWindowsHookEx(_hookID);
        _isHooked = false;
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            if (curModule != null)
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);

            return IntPtr.Zero;
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);
            KeyPressed?.Invoke(this, vkCode);
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion
}
