using System;
using MutinyIRC.PluginFramework;
using MutinyIRC.PluginFramework.Tests.Fixtures;
using NUnit.Framework;

namespace MutinyIRC.PluginFramework.Tests;

[TestFixture]
public class ParseCommandTests
{
    private PluginManager _manager;
    private TestMessageContext _context;

    [SetUp]
    public void SetUp()
    {
        _manager = new PluginManager();
        _context = new TestMessageContext();
    }

    [Test]
    public void SlashPrefix_ParsesCommandName()
    {
        var result = _manager.ParseCommand(_context, "/join");

        Assert.That(result.Name, Is.EqualTo("join"));
        Assert.That(result.ParameterList, Is.Empty);
    }

    [Test]
    public void SlashPrefix_SplitsArgsOnSpaces()
    {
        var result = _manager.ParseCommand(_context, "/say hello world");

        Assert.That(result.Name, Is.EqualTo("say"));
        Assert.That(result.ParameterList, Is.EqualTo(new object[] { "hello", "world" }));
    }

    [Test]
    public void NoSlashPrefix_DefaultsToSayCommand()
    {
        var result = _manager.ParseCommand(_context, "hello there");

        Assert.That(result.Name, Is.EqualTo("say"));
        Assert.That(result.ParameterList, Is.EqualTo(new object[] { "hello", "there" }));
    }

    [Test]
    public void NoSlashPrefix_SingleWord_IsSayWithOneArg()
    {
        var result = _manager.ParseCommand(_context, "hello");

        Assert.That(result.Name, Is.EqualTo("say"));
        Assert.That(result.ParameterList, Is.EqualTo(new object[] { "hello" }));
    }

    [Test]
    public void PassesContextThrough()
    {
        var result = _manager.ParseCommand(_context, "/anything");

        Assert.That(result.Context, Is.SameAs(_context));
    }

    [Test]
    public void NullLine_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _manager.ParseCommand(_context, null));
    }
}
