using System.Collections.Generic;
using System.Linq;
using MutinyIRC.PluginFramework;
using MutinyIRC.PluginFramework.Tests.Fixtures;
using NUnit.Framework;

namespace MutinyIRC.PluginFramework.Tests;

[TestFixture]
public class AssemblyExaminerTests
{
    // InternalCommand is internal to the fixtures assembly, so it can't be referenced via
    // typeof here — match it by full name instead.
    private const string InternalCommandFullName =
        "MutinyIRC.PluginFramework.Tests.Fixtures.InternalCommand";

    private List<CommandInfo> _results;

    [OneTimeSetUp]
    public void ExamineFixtureAssembly()
    {
        _results = AssemblyExaminer
            .ExamineAssembly(typeof(TestCommand).Assembly)
            .ToList();
    }

    private IEnumerable<string> FullNames => _results.Select(r => r.FullName);

    private CommandInfo Command<T>() where T : ICommand
        => _results.Single(r => r.FullName == typeof(T).FullName);

    [Test]
    public void PublicAttributedPlugin_IsReturned()
    {
        Assert.That(FullNames, Does.Contain(typeof(TestCommand).FullName));
    }

    [Test]
    public void InternalType_IsExcluded()
    {
        Assert.That(FullNames, Does.Not.Contain(InternalCommandFullName));
    }

    [Test]
    public void AbstractType_IsExcluded()
    {
        Assert.That(FullNames, Does.Not.Contain(typeof(AbstractCommand).FullName));
    }

    [Test]
    public void TypeWithoutPluginAttribute_IsExcluded()
    {
        Assert.That(FullNames, Does.Not.Contain(typeof(UnattributedCommand).FullName));
    }

    [Test]
    public void TypeNotImplementingIPlugin_IsExcluded()
    {
        Assert.That(FullNames, Does.Not.Contain(typeof(OrphanAttributed).FullName));
    }

    // A plugin that implements IPlugin but not ICommand is no longer discovered.
    [Test]
    public void NonCommandPlugin_IsExcluded()
    {
        Assert.That(FullNames, Does.Not.Contain(typeof(NonCommandPlugin).FullName));
    }

    // An ICommand yields a CommandInfo with all metadata fields populated correctly.
    [Test]
    public void Command_YieldsCommandInfoWithCorrectMetadata()
    {
        var info = Command<TestCommand>();

        Assert.That(info.CommandName, Is.EqualTo("test"));
        Assert.That(info.FullName, Is.EqualTo(typeof(TestCommand).FullName));
        Assert.That(info.AssemblyPath, Is.EqualTo(typeof(TestCommand).Assembly.Location));
        Assert.That(info.Type, Is.EqualTo(typeof(ICommand)));
    }

    // [Plugin(name, description)] flows the description through to CommandInfo.Description.
    [Test]
    public void DescribedCommand_PopulatesDescription()
    {
        var info = Command<DescribedCommand>();

        Assert.That(info.CommandName, Is.EqualTo("desc"));
        Assert.That(info.Description, Is.EqualTo("Has a description"));
    }

    // The [Plugin("name")] overload leaves Description null.
    [Test]
    public void CommandWithoutDescription_HasNullDescription()
    {
        Assert.That(Command<TestCommand>().Description, Is.Null);
    }

    // Parameterless [Plugin()] leaves attr.Name null, so CommandName falls back to type.Name.
    [Test]
    public void NamelessCommand_FallsBackToTypeName()
    {
        Assert.That(Command<NamelessCommand>().CommandName, Is.EqualTo(nameof(NamelessCommand)));
    }

    // Every matching command type in the assembly is yielded, not just the first.
    [Test]
    public void AllMatchingCommands_AreYielded()
    {
        Assert.That(FullNames, Is.SupersetOf(new[]
        {
            typeof(TestCommand).FullName,
            typeof(ThrowingCommand).FullName,
            typeof(DescribedCommand).FullName,
            typeof(NamelessCommand).FullName,
        }));
    }

    // An assembly with no [Plugin] ICommand types yields an empty sequence. The
    // PluginFramework assembly itself defines the interfaces/attributes but no commands.
    [Test]
    public void AssemblyWithNoCommands_YieldsEmpty()
    {
        var results = AssemblyExaminer.ExamineAssembly(typeof(AssemblyExaminer).Assembly);

        Assert.That(results, Is.Empty);
    }
}
