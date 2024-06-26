using MasterApplication.Models.Structs;

namespace MasterApplication.Services.Feature.MouseClicker;

/// <summary>
/// Service to simulate mouse inputs.
/// </summary>
public interface IMouseService
{
    event EventHandler<MouseCoordinate> MouseClicked;

    /// <summary>
    /// Moves the cursor to a specific position on the screen.
    /// </summary>
    /// <param name="x">The x-coordinate of the screen.</param>
    /// <param name="y">The y-coordinate of the screen.</param>
    void MoveCursorTo(int x, int y);

    /// <summary>
    /// Simulates a left mouse click.
    /// </summary>
    void ClickLeftMouseButton();

    /// <summary>
    /// Gets the current position of the mouse cursor.
    /// </summary>
    /// <returns>The current position of the mouse cursor as a <see cref="MouseCoordinate"/>.</returns>
    MouseCoordinate GetMousePos();

    /// <summary>
    /// Intercepts the mouse clicks.
    /// </summary>
    void StartMouseHook();

    /// <summary>
    /// Stops the intercept.
    /// </summary>
    void StopMouseHook();
}
