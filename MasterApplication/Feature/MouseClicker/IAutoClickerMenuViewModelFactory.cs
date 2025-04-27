using MasterApplication.Models;

namespace MasterApplication.Feature.MouseClicker;

public interface IAutoClickerMenuViewModelFactory
{
    /// <summary>
    /// Creates an <see cref="AutoClickerMenuViewModel"/>.
    /// </summary>
    /// <param name="sequence"><see cref="AutoClickerSequence"/> to know what sequences to execute.</param>
    /// <returns>The <see cref="AutoClickerMenuViewModel"/> created.</returns>
    AutoClickerMenuViewModel Create(AutoClickerSequence sequence);
}
