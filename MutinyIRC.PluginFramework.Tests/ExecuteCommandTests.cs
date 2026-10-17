using System.Collections.Generic;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;
using MutinyIRC.PluginFramework.Tests.Fixtures;
using MutinyIRC.TestShared;
using NUnit.Framework;

namespace MutinyIRC.PluginFramework.Tests;

[TestFixture]
public class ExecuteCommandTests
{
    private CommandHarness _harness;

    [SetUp]
    public void SetUp()
    {
        _harness = new CommandHarness()
            .Register<TestCommand>("test")
            .Register<ThrowingCommand>("throws")
            .Register<VoidCommand>("void");
    }

    private static CommandExecutionInfo Build(string name, MessageContext context, params object[] args)
        => new CommandExecutionInfo
        {
            Name = name,
            Context = context,
            ParameterList = new List<object>(args),
        };

    [Test]
    public void UnknownCommand_ReturnsFailResult()
    {
        var result = _harness.Execute(Build("does-not-exist", new TestMessageContext()));

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        Assert.That(result.Message, Does.Contain("invalid command"));
    }

    [Test]
    public void NoArgs_DispatchesToParameterlessOverload()
    {
        var result = _harness.Execute(Build("test", new TestMessageContext()));

        Assert.That(result.Message, Is.EqualTo("noargs"));
    }

    [Test]
    public void SingleStringArg_DispatchesToStringOverload()
    {
        var result = _harness.Execute(Build("test", new TestMessageContext(), "hello"));

        Assert.That(result.Message, Is.EqualTo("string:hello"));
    }

    [Test]
    public void TwoStringArgs_DispatchToMostSpecificOverload()
    {
        var result = _harness.Execute(Build("test", new TestMessageContext(), "a", "b"));

        // Most-specific overload wins — (TestMessageContext, string, string) matches exactly, no coalescing
        Assert.That(result.Message, Is.EqualTo("string,string:a|b"));
    }

    [Test]
    public void ExtraStringArgs_CoalesceIntoFinalStringParameter()
    {
        // 3 args, most-specific (TestMessageContext, string, string) overload accepts 2 strings;
        // trailing extras fold into the final string slot
        var result = _harness.Execute(Build("test", new TestMessageContext(), "a", "b", "c"));

        Assert.That(result.Message, Is.EqualTo("string,string:a|b c"));
    }

    [Test]
    public void OpenEndedCoalescing_AcrossAllArgs()
    {
        // OtherTestMessageContext has only (ctx, string); 3 string args all collapse into the one slot
        var result = _harness.Execute(Build("test", new OtherTestMessageContext(), "x", "y", "z"));

        Assert.That(result.Message, Is.EqualTo("other-context:x y z"));
    }

    [Test]
    public void ChannelNameArg_IsConvertedToChannelInfo()
    {
        var result = _harness.Execute(Build("test", new TestMessageContext(), "#room", "hi there"));

        Assert.That(result.Message, Is.EqualTo("channel,string:#room|hi there"));
    }

    [Test]
    public void SwitchArg_IsConvertedToCharArray()
    {
        var result = _harness.Execute(Build("test", new TestMessageContext(), "-abc"));

        Assert.That(result.Message, Is.EqualTo("switch:abc"));
    }

    [Test]
    public void ContextType_FiltersOverloads()
    {
        var result = _harness.Execute(Build("test", new OtherTestMessageContext(), "x"));

        Assert.That(result.Message, Is.EqualTo("other-context:x"));
    }

    [Test]
    public void NoMatchingOverload_ReturnsNull()
    {
        // OtherTestMessageContext only has a (string) overload; passing zero args has no match.
        var result = _harness.Execute(Build("test", new OtherTestMessageContext()));

        Assert.That(result, Is.Null);
    }

    [Test]
    public void ExecuteThrows_ReturnsFailResult()
    {
        var result = _harness.Execute(Build("throws", new TestMessageContext()));

        Assert.That(result.Result, Is.EqualTo(Result.Fail));
        Assert.That(result.Message, Does.Contain("failed with an error"));
    }

    [Test]
    public void VoidOverload_RunsOnlyTheMatchingOverload()
    {
        // A void overload returns null from Invoke. That null must not read as "no match",
        // or the dispatcher also runs the shorter overload with the arguments joined.
        VoidCommand.Calls.Clear();

        var result = _harness.Execute(Build("void", new TestMessageContext(), "x", "y"));

        Assert.That(VoidCommand.Calls, Is.EqualTo(new[] { "two:x|y" }));
        Assert.That(result.Result, Is.EqualTo(Result.Success));
    }

    [Test]
    public void CommandLookup_IsCaseInsensitive()
    {
        var result = _harness.Execute(Build("TEST", new TestMessageContext()));

        Assert.That(result.Message, Is.EqualTo("noargs"));
    }

    [Test]
    public void ParameterlessExecuteOverload_IsSkippedWithoutThrowing()
    {
        // TestCommand declares an Execute() with no parameters. Overload discovery filters
        // on the first parameter's type, so it must guard against the empty parameter list
        // rather than throw IndexOutOfRangeException.
        Assert.DoesNotThrow(() => _harness.Execute(Build("test", new TestMessageContext())));
    }

    [Test]
    public void ChannelCoercion_DoesNotMutateInputParameterList()
    {
        // Covers in-place channel coercion + context prepend.
        var input = Build("test", new TestMessageContext(), "#room", "hi there");

        _harness.Execute(input);

        Assert.That(input.ParameterList, Is.EqualTo(new object[] { "#room", "hi there" }));
    }

    [Test]
    public void SwitchCoercion_DoesNotMutateInputParameterList()
    {
        // Covers in-place switch (string -> char[]) coercion + context prepend.
        var input = Build("test", new TestMessageContext(), "-abc");

        _harness.Execute(input);

        Assert.That(input.ParameterList, Is.EqualTo(new object[] { "-abc" }));
    }

    [Test]
    public void OpenEndedCollapsing_DoesNotMutateInputParameterList()
    {
        // Covers RemoveRange/Add collapse + context prepend.
        var input = Build("test", new TestMessageContext(), "a", "b", "c");

        _harness.Execute(input);

        Assert.That(input.ParameterList, Is.EqualTo(new object[] { "a", "b", "c" }));
    }
}
