using MasterApplication.Models.Enums;
using MasterApplication.Models.Structs;

namespace MasterApplication.Models;

public class MouseButtonEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public MouseCoordinate Coordinate { get; }

    public MouseButtonEventArgs(MouseButton button, MouseCoordinate coordinate)
    {
        Button = button;
        Coordinate = coordinate;
    }
}
