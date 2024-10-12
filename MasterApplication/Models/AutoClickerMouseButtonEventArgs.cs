using MasterApplication.Models.Enums;
using MasterApplication.Models.Structs;

namespace MasterApplication.Models;

public class AutoClickerMouseButtonEventArgs : EventArgs
{
    public MouseButton Button { get; }
    public MouseCoordinate Coordinate { get; }

    public AutoClickerMouseButtonEventArgs(MouseButton button, MouseCoordinate coordinate)
    {
        Button = button;
        Coordinate = coordinate;
    }
}
