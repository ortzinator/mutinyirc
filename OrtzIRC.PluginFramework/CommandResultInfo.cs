using System;

namespace OrtzIRC.PluginFramework
{
    /// <summary>
    /// Not implemented. Possibly to provide commands with a way to send messages back to the app.
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
}
