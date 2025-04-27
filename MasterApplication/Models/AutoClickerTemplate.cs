using System.Text.Json.Serialization;
using System.Windows;

namespace MasterApplication.Models;

/// <summary>
/// Class that models everything a template needs.
/// </summary>
public class AutoClickerTemplate
{
    public string ImagePath { get; set; }

    [JsonIgnore]
    public byte[]? Image { get; set; }

    public Point ClickCoordinates { get; set; }
    public int DelayBeforeClicking { get; set; }
    public int DelayAfterClicking { get; set; }

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
        DelayBeforeClicking = 0;
        DelayAfterClicking = 0;

        MonitorForChange = false;
        MonitorForChangeInterval = 0;
    }
}
