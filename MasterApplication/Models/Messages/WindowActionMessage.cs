using MasterApplication.Models.Enums;

namespace MasterApplication.Models.Messages;

internal class WindowActionMessage
{
    public WindowAction WindowAction { get; private set; }

    /// <summary>
    /// Message model class with a <see cref="WindowAction"/>.
    /// </summary>
    /// <param name="windowAction"></param>
    public WindowActionMessage(WindowAction windowAction)
    {
        WindowAction = windowAction;
    }
}
