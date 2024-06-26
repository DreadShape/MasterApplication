namespace MasterApplication.Services.Feature.MouseClicker;

/// <summary>
/// Service to intercept keyboard presses.
/// </summary>
public interface IKeyboardService
{
    event EventHandler<int> KeyPressed;

    /// <summary>
    /// Hooks into the keyboard inputs to intercept them.
    /// </summary>
    void StartKeyboardHook();

    /// <summary>
    /// Unhooks from the keyboard inputs.
    /// </summary>
    void StopKeyboardHook();
}
