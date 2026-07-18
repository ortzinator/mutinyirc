using MutinyIRC.PluginFramework;

namespace MutinyIRC.PluginFramework.Tests.Fixtures;

// Fixtures for AssemblyExaminer discovery-filter tests. Each type fails at least one
// filter clause in AssemblyExaminer.ExamineAssembly, so it must NOT appear in the
// examined results. (The happy-path "is returned" case is covered by the existing
// public TestCommand fixture.)

// Abstract classes are excluded even when otherwise valid.
[Plugin("abstract")]
public abstract class AbstractCommand : ICommand
{
}

// Non-public (internal) types are excluded.
[Plugin("internal")]
internal class InternalCommand : ICommand
{
}

// Types without [Plugin] are excluded.
public class UnattributedCommand : ICommand
{
}

// Attributed types that implement no interfaces fail the ICommand filter. (Together with
// NonCommandPlugin this covers both ways a [Plugin] type can fail to be a command.)
[Plugin("orphan")]
public class OrphanAttributed
{
}

// An IPlugin that is not an ICommand also fails the ICommand filter — only commands are
// discovered.
[Plugin("plugin")]
public class NonCommandPlugin : IPlugin
{
}
