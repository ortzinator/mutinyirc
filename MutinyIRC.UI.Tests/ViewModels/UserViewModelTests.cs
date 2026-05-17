using FlamingIRC;
using NUnit.Framework;
using MutinyIRC.UI.ViewModels;

namespace MutinyIRC.UI.Tests.ViewModels;

[TestFixture]
public class UserViewModelTests
{
    [Test]
    public void UserViewModel_AssignsOpMode_ForAtPrefix()
    {
        var user = User.FromNames("@Alice");
        var vm = new UserViewModel(user);

        Assert.That(vm.Mode, Is.EqualTo(Mode.Op));
    }

    [Test]
    public void UserViewModel_AssignsVoiceMode_ForPlusPrefix()
    {
        var user = User.FromNames("+Bob");
        var vm = new UserViewModel(user);

        Assert.That(vm.Mode, Is.EqualTo(Mode.Voice));
    }

    [Test]
    public void UserViewModel_AssignsRegularMode_ForNoPrefix()
    {
        var user = User.FromNames("Charlie");
        var vm = new UserViewModel(user);

        Assert.That(vm.Mode, Is.EqualTo(Mode.Regular));
    }
}
