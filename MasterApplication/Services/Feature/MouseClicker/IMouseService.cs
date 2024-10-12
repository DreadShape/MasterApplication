using MasterApplication.Models;
using MasterApplication.Models.Structs;

namespace MasterApplication.Services.Feature.MouseClicker;

/// <summary>
/// Service to simulate mouse inputs.
/// </summary>
public interface IMouseService
{
    event EventHandler<AutoClickerMouseButtonEventArgs> MouseButtonClicked;

    /// <summary>
    /// Moves the mouse cursor to the specified coordinates.
    /// </summary>
    /// <param name="x">Horizontal coordinate.</param>
    /// <param name="y">Vertical coordinate.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the one or both of the mouse coordinates are negative values.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the mouse cannot move to the specified coordinates.</exception>
    void MoveCursorTo(int x, int y);

    /// <summary>
    /// Clicks the left click mouse button on the current cursor's position.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the mouse cannot click the button on the current position.</exception>
    void ClickLeftMouseButton();

    /// <summary>
    /// Gets the current mouse position as a <see cref="MouseCoordinate"/> struct.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Thrown when it fails to get the mouse position.</exception>
    MouseCoordinate GetMousePos();

    /// <summary>
    /// Hooks to the mouse to be able to use the click buttons.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call the method again while already being hooked.</exception>
    void StartMouseHook();

    /// <summary>
    /// Unhooks from the mouse.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if you call this method while not being hooked.</exception>
    void StopMouseHook();

    /// <summary>
    /// Tells us if the mouse is hooked.
    /// </summary>
    /// <returns></returns>
    public bool IsMouseHookAttached();
}
