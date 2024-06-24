using MasterApplication.Features.Productivity;

namespace MasterApplication.Models.Messages;

/// <summary>
/// Message to pass to the <see cref="DailyProductivityChunksView"/> user control.
/// </summary>
public class DailyProductivityMessage
{
    public string Content { get; }

    /// <summary>
    /// Creates and instance of an <see cref="DailyProductivityMessage"/>.
    /// </summary>
    /// <param name="message">Simple text message.</param>
    /// <summary>
    public DailyProductivityMessage(string message)
    {
        Content = message;
    }
}
