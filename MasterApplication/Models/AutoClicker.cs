namespace MasterApplication.Models;

/// <summary>
/// Model to hold the different types of sequences it can have.
/// </summary>
public class AutoClicker
{
    public Sequence Bidding { get; set; }

    public Sequence Selling { get; set; }

    public Sequence AutoClickerLoop { get; set; }

    /// <summary>
    /// Creates an instance of a <see cref="AutoClicker"/>.
    /// </summary>
    public AutoClicker()
    {
        Selling = new();
        Bidding = new();
        AutoClickerLoop = new();
    }
}
