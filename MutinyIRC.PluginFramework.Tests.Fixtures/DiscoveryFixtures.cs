namespace MutinyIRC.PluginFramework.Tests.Fixtures
{
    using MutinyIRC.PluginFramework;

    // Fixtures for AssemblyExaminer Tier 1 discovery-filter tests. Each type is shaped to
    // be rejected by exactly one of the .Where clauses in AssemblyExaminer.ExamineAssembly,
    // so it must NOT appear in the examined results. (The happy-path "is returned" case is
    // covered by the existing public TestCommand fixture.)

    // #3 — abstract classes are excluded even when otherwise valid.
    [Plugin("abstract")]
    public abstract class AbstractCommand : ICommand
    {
    }

    // #2 — non-public (internal) types are excluded.
    [Plugin("internal")]
    internal class InternalCommand : ICommand
    {
    }

    // #4 — types without [Plugin] are excluded.
    public class UnattributedCommand : ICommand
    {
    }

    // #5 — types carrying [Plugin] but not implementing IPlugin are excluded.
    [Plugin("orphan")]
    public class OrphanAttributed
    {
    }

    // #6 — an IPlugin that is not an ICommand is excluded (only commands are discovered).
    [Plugin("plugin")]
    public class NonCommandPlugin : IPlugin
    {
    }
}