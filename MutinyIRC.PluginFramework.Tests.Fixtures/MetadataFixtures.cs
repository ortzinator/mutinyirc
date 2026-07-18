using MutinyIRC.PluginFramework;

namespace MutinyIRC.PluginFramework.Tests.Fixtures;

// Fixtures for AssemblyExaminer branching/metadata tests. Unlike the discovery
// fixtures, these all pass the filters and are expected to appear in the results — the
// tests assert on what kind of PluginInfo is produced and how its fields are populated.

// [Plugin(name, description)] populates CommandInfo.Description.
[Plugin("desc", "Has a description")]
public class DescribedCommand : ICommand
{
}

// Parameterless [Plugin()] leaves attr.Name null, so CommandName falls back to type.Name.
[Plugin]
public class NamelessCommand : ICommand
{
}
