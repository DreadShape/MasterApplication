namespace MasterApplication.Models;

/// <summary>
/// Class that models everything a template needs.
/// </summary>
public class AutoClickerTemplate
{
    public string ImagePath { get; set; } = string.Empty;

    public byte[]? Image { get; set; }

    public System.Windows.Point ClickCoordinates { get; set; }

    public int DelayBeforeClicking { get; set; }

    public bool MonitorForChange { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="AutoClickerTemplate"/>.
    /// </summary>
    public AutoClickerTemplate()
    {
    }
}
