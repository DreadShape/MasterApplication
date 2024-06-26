using MasterApplication.Models.Structs;

namespace MasterApplication.Models;

/// <summary>
/// A list of <see cref="AutoClickerSequence"/>.
/// </summary>
public class Sequence
{
    public IList<AutoClickerSequence> Steps { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="Sequence"/>.
    /// </summary>
    public Sequence()
    {
        Steps = new List<AutoClickerSequence>();
    }
}
