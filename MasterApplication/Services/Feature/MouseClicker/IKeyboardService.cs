namespace MasterApplication.Services.Feature.MouseClicker;

/// <summary>
/// Service to intercept keyboard presses.
/// </summary>
public interface IKeyboardService
{
    event EventHandler<int> KeyPressed;

    /// <summary>
    /// Hooks to the keyboard to be able to use the keys.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call the method again while already being hooked.</exception>
    void StartKeyboardHook();

    /// <summary>
    /// Hooks from the keyboard.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call this method while not being hooked.</exception>
    void StopKeyboardHook();

    /// <summary>
    /// Returns the name of the key based on it's code.
    /// </summary>
    /// <param name="keyCode">Code of the key.</param>
    /// <returns>The name of the key pressed.</returns>
    string GetKeyByCode(int keyCode);

    /// <summary>
    /// Returns if the keyboard hook is listening to key presses or not.
    /// </summary>
    /// <returns></returns>
    bool IsKeyboardHookAttached();
}
