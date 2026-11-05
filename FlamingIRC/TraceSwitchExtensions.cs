using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace FlamingIRC;

/// <summary>
/// Writes FlamingIRC trace messages through a <see cref="TraceSwitch"/>.
/// </summary>
/// <remarks>
/// Each message gets the prefix <c>[thread] Class::Method()</c>. The compiler supplies the class
/// and the method through caller-info attributes, so the tag cannot go stale when a method gets a
/// new name. The class name is the source file name, because FlamingIRC keeps one class per file.
/// <para>
/// Each method has <see cref="ConditionalAttribute"/> with <c>DEBUG</c>, the same as
/// <see cref="Debug.WriteLineIf(bool, string)"/>. A build without <c>DEBUG</c> removes the call
/// and the evaluation of its arguments.
/// </para>
/// <example>
/// <code>
/// DccUtil.DccTrace.Warning($"exception={ex}");
/// // [DccThread] DccChatSession::ReceiveMessages() exception=System.IO.IOException: ...
/// DccUtil.DccTrace.Info();
/// // [DccThread] DccChatSession::Close()
/// </code>
/// </example>
/// </remarks>
internal static class TraceSwitchExtensions
{
    [Conditional("DEBUG")]
    internal static void Error(this TraceSwitch traceSwitch, string message = "",
        [CallerFilePath] string file = "", [CallerMemberName] string member = "")
    {
        if (traceSwitch.TraceError)
            Write(message, file, member);
    }

    [Conditional("DEBUG")]
    internal static void Warning(this TraceSwitch traceSwitch, string message = "",
        [CallerFilePath] string file = "", [CallerMemberName] string member = "")
    {
        if (traceSwitch.TraceWarning)
            Write(message, file, member);
    }

    [Conditional("DEBUG")]
    internal static void Info(this TraceSwitch traceSwitch, string message = "",
        [CallerFilePath] string file = "", [CallerMemberName] string member = "")
    {
        if (traceSwitch.TraceInfo)
            Write(message, file, member);
    }

    [Conditional("DEBUG")]
    internal static void Verbose(this TraceSwitch traceSwitch, string message = "",
        [CallerFilePath] string file = "", [CallerMemberName] string member = "")
    {
        if (traceSwitch.TraceVerbose)
            Write(message, file, member);
    }

    // An empty message gives only the prefix, with no trailing space.
    internal static string Format(string threadName, string message, string file, string member)
    {
        string prefix = $"[{threadName}] {ClassNameOf(file)}::{member}()";
        return message.Length == 0 ? prefix : $"{prefix} {message}";
    }

    private static void Write(string message, string file, string member)
        => Debug.WriteLine(Format(Thread.CurrentThread.Name, message, file, member));

    // The compiler writes the path in the format of the build machine. Split on both separators,
    // because Path.GetFileNameWithoutExtension on Linux does not split on '\'.
    private static string ClassNameOf(string file)
    {
        string fileName = file[(file.LastIndexOfAny(['/', '\\']) + 1)..];
        int dot = fileName.IndexOf('.', StringComparison.Ordinal);
        return dot < 0 ? fileName : fileName[..dot];
    }
}
