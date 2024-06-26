namespace MasterApplication.Models.Structs;

/// <summary>
/// Contains coordinates and a sleep flag between each clicks.
/// </summary>
public struct AutoClickerSequence
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Sleep { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="AutoClickerSequence"/>.
    /// </summary>
    public AutoClickerSequence(int x, int y, int sleep)
    {
        X = x;
        Y = y;
        Sleep = sleep;
    }
}
