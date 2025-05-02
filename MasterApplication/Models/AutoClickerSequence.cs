using System.Drawing;

using MasterApplication.Models.Structs;

namespace MasterApplication.Models;

/// <summary>
/// Contains coordinates and a sleep flag between each clicks.
/// </summary>
public class AutoClickerSequence
{
    public string Name { get; set; } = string.Empty;

    public Keybind StartKeybind { get; set; }
    public Keybind StopKeybind { get; set; }
    public Rectangle TemplateSearchBounds { get; set; }

    public IList<AutoClickerTemplate> Templates { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="AutoClickerSequence"/>.
    /// </summary>
    public AutoClickerSequence()
    {
        Name = string.Empty;
        StartKeybind = new("", 0);
        StopKeybind = new("", 0);
        Templates = new List<AutoClickerTemplate>();
    }

    /// <summary>
    /// Makes a copy of <see cref="AutoClickerSequence"/>.
    /// </summary>
    /// <returns></returns>
    public AutoClickerSequence Clone()
    {
        return new AutoClickerSequence
        {
            Name = this.Name,
            StartKeybind = new Keybind(this.StartKeybind.KeyName, this.StartKeybind.KeyCode),
            StopKeybind = new Keybind(this.StopKeybind.KeyName, this.StopKeybind.KeyCode),
            Templates = this.Templates.Select(t => t.Clone()).ToList()
        };
    }
}
