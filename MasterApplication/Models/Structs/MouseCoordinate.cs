using System.Runtime.InteropServices;

namespace MasterApplication.Models.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MouseCoordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public MouseCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
