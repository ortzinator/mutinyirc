using System;
using Avalonia.Controls;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

/// <summary>
/// Regression tests for the StackOverflowException that occurred on window close.
///
/// The bug: ServerViewModel.Close() fires RequestClose, which triggered window.Close(),
/// which re-entered viewModel.Close() through the Closing handler, causing infinite recursion.
/// The fix in App.axaml.cs adds a `closing` flag guard before calling viewModel.Close().
/// These tests verify that guard prevents re-entrant calls.
/// </summary>
[TestFixture]
public class ViewModelCloseTests
{
    private sealed class StubViewModel : ViewModelBase { }

    [Test]
    public void Close_FiresRequestClose()
    {
        var vm = new StubViewModel();
        bool fired = false;
        vm.RequestClose += (_, _) => fired = true;

        vm.Close();

        Assert.That(fired, Is.True);
    }

    [Test]
    public void Close_FiresRequestCloseExactlyOnce()
    {
        var vm = new StubViewModel();
        int count = 0;
        vm.RequestClose += (_, _) => count++;

        vm.Close();

        Assert.That(count, Is.EqualTo(1));
    }

    [Test]
    public void CloseCommand_ExecutesClose()
    {
        var vm = new StubViewModel();
        bool fired = false;
        vm.RequestClose += (_, _) => fired = true;

        vm.CloseCommand.Execute(null);

        Assert.That(fired, Is.True);
    }

    /// <summary>
    /// Simulates the App.axaml.cs closing-guard pattern that prevented the SOE regression.
    /// Without the `closing` flag, the call chain:
    ///   window.Closing → viewModel.Close() → RequestClose → window.Close()
    ///   → window.Closing → viewModel.Close() → ... (SOE)
    /// would cause infinite recursion.  The guard makes it a one-shot.
    /// </summary>
    [Test]
    public void ClosingGuardPattern_PreventsReentrantViewModelClose()
    {
        var vm = new StubViewModel();
        bool closing = false;
        int viewModelCloseCount = 0;

        Action simulatedClosingHandler = () =>
        {
            if (closing) return;
            closing = true;
            viewModelCloseCount++;
            vm.Close();
        };

        // RequestClose handler mirrors: viewModel.RequestClose += (_, _) => window.Close();
        vm.RequestClose += (_, _) => simulatedClosingHandler();

        Assert.DoesNotThrow(() => simulatedClosingHandler());
        Assert.That(viewModelCloseCount, Is.EqualTo(1),
            "viewModel.Close() must be called exactly once despite re-entrant RequestClose");
    }

    [Test]
    public void Close_CalledTwice_DoesNotThrow()
    {
        var vm = new StubViewModel();
        vm.Close();
        Assert.DoesNotThrow(() => vm.Close());
    }

    /// <summary>
    /// Verifies the ChannelViewModel close lifecycle: RequestClose must fire when a
    /// ChannelViewModel is closed, and must not fire again after unsubscription (mimicking
    /// the Chan_RequestClose handler in MainViewModel).
    /// </summary>
    [AvaloniaTest]
    public void IrcViewModel_Close_FiresRequestClose()
    {
        var vm = new DesignChannelViewModelStub();
        bool fired = false;
        vm.RequestClose += (_, _) => fired = true;

        vm.Close();

        Assert.That(fired, Is.True);
    }

    [AvaloniaTest]
    public void IrcViewModel_AfterUnsubscribe_CloseDoesNotFireHandler()
    {
        var vm = new DesignChannelViewModelStub();
        int count = 0;
        EventHandler handler = (_, _) => count++;
        vm.RequestClose += handler;

        vm.Close();
        vm.RequestClose -= handler;
        vm.Close();

        Assert.That(count, Is.EqualTo(1),
            "Handler removed before second Close must not fire a second time");
    }

    /// <summary>
    /// Headless window test: full window close lifecycle with guard must not throw.
    /// Mirrors how App.axaml.cs wires viewModel.RequestClose → window.Close().
    /// </summary>
    [AvaloniaTest]
    public void WindowClose_WithClosingGuard_CallsViewModelCloseOnce()
    {
        var window = new Window();
        var vm = new StubViewModel();

        bool closing = false;
        int viewModelCloseCalls = 0;

        window.Closing += (_, _) =>
        {
            if (closing) return;
            closing = true;
            viewModelCloseCalls++;
            vm.Close();
        };

        vm.RequestClose += (_, _) => window.Close();

        window.Show();
        Assert.DoesNotThrow(() => window.Close());
        Assert.That(viewModelCloseCalls, Is.EqualTo(1));
    }

    // Minimal IrcViewModel stub — no real Channel/Server dependencies.
    private sealed class DesignChannelViewModelStub : IrcViewModel
    {
        public override Common.Server? OwningServer => null;
        public override void Dispose() { }
    }
}
