namespace MasterApplication.Models.Structs;

/// <summary>
/// Contains coordinates and a sleep flag between each clicks.
/// </summary>
public class AutoClickerSequence
{
    public int X { get; set; }
    public int Y { get; set; }

    public string Template { get; set; }
    public bool MonitorForChange { get; set; }
    public int Sleep { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="AutoClickerSequence"/>.
    /// </summary>
    public AutoClickerSequence(string template, bool monitorForChange, int sleep, int x, int y)
    {
        Template = template;
        MonitorForChange = monitorForChange;
        Sleep = sleep;
        X = x;
        Y = y;
    }
}
