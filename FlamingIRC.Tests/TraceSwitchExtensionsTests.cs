using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace FlamingIRC.Tests;

/// <summary>
/// The trace helpers write <c>[thread] Class::Method() message</c> only when the switch level
/// lets the message through.
/// </summary>
[TestFixture]
public class TraceSwitchExtensionsTests
{
    private StringWriter _output;
    private TextWriterTraceListener _listener;
    private TraceSwitch _traceSwitch;

    [SetUp]
    public void SetUp()
    {
        _output = new StringWriter();
        _listener = new TextWriterTraceListener(_output);
        Trace.Listeners.Add(_listener);
        _traceSwitch = new TraceSwitch("TraceSwitchExtensionsTests", "Test switch.");
    }

    [TearDown]
    public void TearDown()
    {
        Trace.Listeners.Remove(_listener);
        _listener.Dispose();
        _output.Dispose();
    }

    // Other code can write trace output at the same time, so find the lines by a unique marker.
    private string[] LinesWith(string marker)
    {
        _listener.Flush();
        return _output.ToString()
            .Split('\n')
            .Select(line => line.TrimEnd('\r'))
            .Where(line => line.Contains(marker, StringComparison.Ordinal))
            .ToArray();
    }

    [TestCase("/home/dev/mutinyirc/FlamingIRC/Dcc/DccChatSession.cs")]
    [TestCase(@"C:\src\mutinyirc\FlamingIRC\Dcc\DccChatSession.cs")]
    [TestCase("DccChatSession.cs")]
    public void Format_TakesTheClassNameFromTheFileName(string file)
    {
        string line = TraceSwitchExtensions.Format("DccThread", "exception=x", file, "Listen");

        Assert.That(line, Is.EqualTo("[DccThread] DccChatSession::Listen() exception=x"));
    }

    [Test]
    public void Format_WritesEmptyBracketsForAThreadWithNoName()
    {
        string line = TraceSwitchExtensions.Format(null, "text", "Identd.cs", "Run");

        Assert.That(line, Is.EqualTo("[] Identd::Run() text"));
    }

    [Test]
    public void Format_WritesNoTrailingSpaceForAnEmptyMessage()
    {
        string line = TraceSwitchExtensions.Format("t", string.Empty, "Identd.cs", "Stop");

        Assert.That(line, Is.EqualTo("[t] Identd::Stop()"));
    }

    [Test]
    public void Warning_SuppliesThisClassAndMethodFromTheCaller()
    {
        _traceSwitch.Level = TraceLevel.Verbose;
        string marker = Guid.NewGuid().ToString();

        _traceSwitch.Warning(marker);

        string expected = $"[{Thread.CurrentThread.Name}] TraceSwitchExtensionsTests::"
            + $"{nameof(Warning_SuppliesThisClassAndMethodFromTheCaller)}() {marker}";
        Assert.That(LinesWith(marker), Is.EqualTo(new[] { expected }));
    }

    [TestCase(TraceLevel.Off, new string[0])]
    [TestCase(TraceLevel.Error, new[] { "error" })]
    [TestCase(TraceLevel.Warning, new[] { "error", "warning" })]
    [TestCase(TraceLevel.Info, new[] { "error", "warning", "info" })]
    [TestCase(TraceLevel.Verbose, new[] { "error", "warning", "info", "verbose" })]
    public void EachMethod_WritesOnlyWhenTheSwitchLevelAllowsIt(TraceLevel level, string[] expected)
    {
        _traceSwitch.Level = level;
        string marker = Guid.NewGuid().ToString();

        _traceSwitch.Error($"{marker} error");
        _traceSwitch.Warning($"{marker} warning");
        _traceSwitch.Info($"{marker} info");
        _traceSwitch.Verbose($"{marker} verbose");

        string[] written = LinesWith(marker)
            .Select(line => line[(line.LastIndexOf(' ') + 1)..])
            .ToArray();
        Assert.That(written, Is.EqualTo(expected));
    }
}
