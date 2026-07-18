namespace MutinyIRC.PluginFramework;

/// <summary>
/// The outcome of running a command. A <see cref="Result.Fail"/> result has its
/// <see cref="Message"/> rendered as an error line in the chat pane; a
/// <see cref="Result.Success"/> result (or a <c>null</c>/void return) produces no UI output.
/// </summary>
public class CommandResultInfo
{
    public Result Result { get; set; }
    public string Message { get; set; }

    public static CommandResultInfo Fail(string message)
    {
        return new CommandResultInfo
        {
            Message = message,
            Result = Result.Fail
        };
    }

    public static CommandResultInfo Success(string message)
    {
        return new CommandResultInfo
        {
            Message = message,
            Result = Result.Success
        };
    }
}
