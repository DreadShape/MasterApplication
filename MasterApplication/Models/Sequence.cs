using MasterApplication.Models.Structs;

namespace MasterApplication.Models;

/// <summary>
/// A list of <see cref="AutoClickerSequence"/>.
/// </summary>
public class Sequence
{
    public int Loops { get; set; }
    public Keybind StartKeybind { get; set; }
    public Keybind StopKeybind { get; set; }

    public IList<AutoClickerSequence> Steps { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="Sequence"/>.
    /// </summary>
    public Sequence()
    {
        StartKeybind = new();
        StopKeybind = new();
        Steps = new List<AutoClickerSequence>();
    }
}
