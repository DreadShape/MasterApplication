using System.Windows;

namespace MasterApplication.Models;

/// <summary>
/// Class that models everything a template needs.
/// </summary>
public class AutoClickerTemplate
{
    public string ImagePath { get; set; }
    public byte[]? Image { get; set; }

    public Point ClickCoordinates { get; set; }
    public int ClickCoordinatesInterval { get; set; }

    public bool MonitorForChange { get; set; }
    public int MonitorForChangeInterval { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="AutoClickerTemplate"/>.
    /// </summary>
    public AutoClickerTemplate()
    {
        ImagePath = string.Empty;
        Image = null;

        ClickCoordinates = new Point();
        ClickCoordinatesInterval = 0;

        MonitorForChange = false;
        MonitorForChangeInterval = 0;
    }
}
