using MasterApplication.Models.Structs;

namespace MasterApplication.Models;

/// <summary>
/// A list of <see cref="AutoClickerSequence"/>.
/// </summary>
public class Sequence
{
    public string StartKeyBind { get; set; }
    public string StopKeyBind { get; set; }

    public IList<AutoClickerSequence> Steps { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="Sequence"/>.
    /// </summary>
    public Sequence()
    {
        StartKeyBind = string.Empty;
        StopKeyBind = string.Empty;
        Steps = new List<AutoClickerSequence>();
    }
}
