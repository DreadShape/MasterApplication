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

    public IList<AutoClickerTemplate> Templates { get; set; } = new List<AutoClickerTemplate>();

    /// <summary>
    /// Creates an instance of a <see cref="AutoClickerSequence"/>.
    /// </summary>
    public AutoClickerSequence()
    {
    }
}
