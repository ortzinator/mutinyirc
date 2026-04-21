using System.Linq;
using Avalonia.Headless.NUnit;
using NUnit.Framework;
using OrtzIRC.Avalonia.ViewModels;
using OrtzIRC.Avalonia.ViewModels.Design;

namespace MutinyIRC.Avalonia.Tests.ViewModels;

/// <summary>
/// Regression tests for the design-time DataContext namespace conflict.
///
/// The bug: using xmlns:design="clr-namespace:Avalonia.Controls" as the design alias
/// would shadow Avalonia's built-in Design attached-property class, causing
/// &lt;Design.DataContext&gt; to resolve as a user element rather than the Avalonia property.
///
/// The fix: put all design-time view models in OrtzIRC.Avalonia.ViewModels.Design
/// so that the XAML alias `design:` is distinct from `Design.` (the Avalonia property).
/// </summary>
[TestFixture]
public class DesignViewModelTests
{
    [Test]
    public void DesignMainViewModel_IsInViewModelsDesignNamespace()
    {
        // Ensures the type lives under ViewModels.Design, not Avalonia.Controls,
        // which would conflict with Design.DataContext in XAML.
        Assert.That(
            typeof(DesignMainViewModel).Namespace,
            Is.EqualTo("OrtzIRC.Avalonia.ViewModels.Design"));
    }

    [Test]
    public void DesignChannelViewModel_IsInViewModelsDesignNamespace()
    {
        Assert.That(
            typeof(DesignChannelViewModel).Namespace,
            Is.EqualTo("OrtzIRC.Avalonia.ViewModels.Design"));
    }

    [Test]
    public void DesignMainViewModel_NamespaceDoesNotContainAvaloniaControls()
    {
        // Guard: must never be placed inside the Avalonia.Controls namespace tree.
        Assert.That(
            typeof(DesignMainViewModel).Namespace,
            Does.Not.StartWith("Avalonia.Controls"));
    }

    /// <summary>
    /// DesignMainViewModel must be constructable without throwing.
    /// Outside of the XAML designer, Design.IsDesignMode is false so the
    /// ServerViewModel parameterless ctor skips its design-time branches.
    /// </summary>
    [AvaloniaTest]
    public void DesignMainViewModel_CanBeInstantiated()
    {
        DesignMainViewModel vm = null!;
        Assert.DoesNotThrow(() => vm = new DesignMainViewModel());
        Assert.That(vm, Is.Not.Null);
        Assert.That(vm.Panels, Is.Not.Null);
        Assert.That(vm.SelectedPanel, Is.Not.Null);
    }

    [AvaloniaTest]
    public void DesignMainViewModel_PanelsContainsOneServerViewModel()
    {
        var vm = new DesignMainViewModel();
        Assert.That(vm.Panels.Count, Is.EqualTo(1));
        Assert.That(vm.Panels[0], Is.InstanceOf<ServerViewModel>());
    }

    [AvaloniaTest]
    public void DesignMainViewModel_SelectedPanelMatchesPanels()
    {
        var vm = new DesignMainViewModel();
        Assert.That(vm.SelectedPanel, Is.SameAs(vm.Panels[0]));
    }

    /// <summary>
    /// DesignChannelViewModel provides realistic sample data for the channel view.
    /// Verifies it populates ChatLines and UserList without throwing.
    /// </summary>
    [AvaloniaTest]
    public void DesignChannelViewModel_CanBeInstantiated()
    {
        DesignChannelViewModel vm = null!;
        Assert.DoesNotThrow(() => vm = new DesignChannelViewModel());
        Assert.That(vm, Is.Not.Null);
    }

    [AvaloniaTest]
    public void DesignChannelViewModel_PopulatesChatLinesAndUserList()
    {
        var vm = new DesignChannelViewModel();
        Assert.That(vm.ChatLines.Count, Is.GreaterThan(0), "ChatLines should have sample messages");
        Assert.That(vm.UserList, Is.Not.Null);
        Assert.That(vm.UserList.Count, Is.GreaterThan(0), "UserList should have sample users");
    }

    [AvaloniaTest]
    public void DesignChannelViewModel_UserListContainsAllModes()
    {
        var vm = new DesignChannelViewModel();
        var modes = vm.UserList.Select(u => u.Mode).ToList();
        Assert.That(modes, Does.Contain(Mode.Op), "Should include an Op user");
        Assert.That(modes, Does.Contain(Mode.Voice), "Should include a Voice user");
        Assert.That(modes, Does.Contain(Mode.Regular), "Should include a Regular user");
    }
}
