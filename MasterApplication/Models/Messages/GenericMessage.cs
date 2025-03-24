namespace MasterApplication.Models.Messages;

internal class GenericMessage
{
    public string Message { get; private set; }

    /// <summary>
    /// Generic message to send.
    /// </summary>
    /// <param name="message">Information for the generic message.</param>
    public GenericMessage(string message = "")
    {
        Message = message;
    }
}
