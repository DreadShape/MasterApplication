using System.Runtime.InteropServices;
using System.Windows;

namespace MasterApplication.Services.Mouse;

public static class MouseSimulator
{
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

        // Other input structures can be added here if needed
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

    private static void MoveCursorTo(int x, int y)
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

    public static void ClickLeftMouseButton(int x, int y)
    {
        // Move the cursor to the specified coordinates
        MoveCursorTo(x, y);

        INPUT[] inputs = new INPUT[2];

        // Move the mouse to the specified coordinates (optional, can be skipped if clicking at the current position)
        // Set the mouse event parameters
        inputs[0] = new INPUT
        {
            Type = INPUT_MOUSE,
            U = new InputUnion
            {
                Mouse = new MOUSEINPUT
                {
                    X = x,
                    Y = y,
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
                    X = x,
                    Y = y,
                    DwFlags = MOUSEEVENTF_LEFTUP,
                    DwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}
