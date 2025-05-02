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
    public double MatchThreshold { get; set; }
    public bool ResetPosition { get; set; }

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
        ResetPosition = false;
        DelayBeforeClicking = 0;
        DelayAfterClicking = 0;

        MonitorForChange = false;
        MonitorForChangeInterval = 0;
        MatchThreshold = 0;
    }

    /// <summary>
    /// Makes a copy of the <see cref="AutoClickerTemplate"/>.
    /// </summary>
    /// <returns></returns>
    public AutoClickerTemplate Clone()
    {
        return new AutoClickerTemplate
        {
            ImagePath = this.ImagePath,
            Image = this.Image != null ? (byte[])this.Image.Clone() : null,
            ClickCoordinates = this.ClickCoordinates,
            DelayBeforeClicking = this.DelayBeforeClicking,
            DelayAfterClicking = this.DelayAfterClicking,
            ResetPosition = this.ResetPosition,
            MonitorForChange = this.MonitorForChange,
            MonitorForChangeInterval = this.MonitorForChangeInterval
        };
    }
}
