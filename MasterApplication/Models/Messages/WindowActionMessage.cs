using MasterApplication.Models.Enums;

namespace MasterApplication.Models.Messages;

internal class WindowActionMessage
{
    public WindowAction WindowAction { get; private set; }

    public string Message { get; private set; }

    /// <summary>
    /// Message model class with a <see cref="WindowAction"/>.
    /// </summary>
    /// <param name="windowAction"><see cref="Enum"/> of the different states of the window.</param>
    /// <param name="message">Additional information.</param>
    public WindowActionMessage(WindowAction windowAction, string message = "")
    {
        WindowAction = windowAction;
        Message = message;
    }
}
