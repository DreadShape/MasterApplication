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

    /// <summary>
    /// Returns the name of the key based on it's code.
    /// </summary>
    /// <param name="keyCode">Code of the key.</param>
    /// <returns>The name of the key pressed.</returns>
    string GetKeyByCode(int keyCode);

    /// <summary>
    /// Tells us if the keyboard hook is listening to key presses or not.
    /// </summary>
    /// <returns></returns>
    bool IsKeyboardHookAttached();
}
