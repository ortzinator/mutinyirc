namespace MutinyIRC.PluginFramework.Tests.Fixtures
{
    using MutinyIRC.PluginFramework;

    // Fixtures for AssemblyExaminer Tier 2 branching/metadata tests. Unlike the discovery
    // fixtures, these all pass the filters and are expected to appear in the results — the
    // tests assert on what kind of PluginInfo is produced and how its fields are populated.

    // #8 — [Plugin(name, description)] populates CommandInfo.Description.
    [Plugin("desc", "Has a description")]
    public class DescribedCommand : ICommand
    {
    }

    // #10 — parameterless [Plugin()] leaves attr.Name null, so CommandName falls back to type.Name.
    [Plugin]
    public class NamelessCommand : ICommand
    {
    }
}
