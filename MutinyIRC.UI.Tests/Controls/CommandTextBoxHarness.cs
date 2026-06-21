using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Input;
using MutinyIRC.Common;
using MutinyIRC.UI.Controls;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.Controls;

/// <summary>
/// Shared scaffolding for the <see cref="CommandTextBox"/> test fixtures: hosts the control in a
/// headless window, raises key presses through the real routed-event path, and supplies a minimal
/// <see cref="IrcViewModel"/> stub. Imported via <c>using static</c> so call sites read unqualified.
/// </summary>
internal static class CommandTextBoxHarness
{
    /// <summary>
    /// Minimal <see cref="IrcViewModel"/> for the control: no owning server, and an optional fixed
    /// candidate list for nick completion (empty when none are supplied).
    /// </summary>
    public sealed class StubViewModel : IrcViewModel
    {
        private readonly IReadOnlyList<string> _candidates;
        public StubViewModel(params string[] candidates) => _candidates = candidates;
        public override Server? OwningServer => null;
        public override IReadOnlyList<string> CompletionCandidates => _candidates;
        public override void Dispose() { }
    }

    /// <summary>Hosts a fresh <see cref="CommandTextBox"/> in a shown window, wired to <paramref name="vm"/>.</summary>
    public static (CommandTextBox box, Window window) Host(IrcViewModel? vm = null)
    {
        var window = new Window { Width = 400, Height = 200 };
        var box = new CommandTextBox();
        window.Content = box;
        window.Show();
        box.DataContext = vm ?? new StubViewModel();
        global::Avalonia.Threading.Dispatcher.UIThread.RunJobs();
        return (box, window);
    }

    /// <summary>Raises a KeyDown for <paramref name="key"/> and returns whether the control handled it.</summary>
    public static bool PressKey(CommandTextBox box, Key key)
    {
        var args = new KeyEventArgs { Key = key, RoutedEvent = InputElement.KeyDownEvent };
        box.RaiseEvent(args);
        return args.Handled;
    }
}
