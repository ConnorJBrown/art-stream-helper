using ArtStreamHelper.Core.ViewModels.Config;

namespace ArtStreamHelper.Core.Test.ViewModels.Configs;

public class ButtonConfigViewModelTest
{
    [Test]
    public void ShouldItCorrectly()
    {
        var vm = new ButtonConfigViewModel
        {
            ClickFunc = () =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            }
        };

        Assert.That(vm.HasChanges, Is.EqualTo(false));
    }

    [Test]
    public async Task ShouldExecuteActionWhenClickCommandExecuted()
    {
        bool hasClicked = false;

        var vm = new ButtonConfigViewModel
        {
            ClickFunc = () =>
            {
                hasClicked = true;
                return Task.CompletedTask;

            }
        };

        await vm.ClickCommand.ExecuteAsync(null);

        Assert.That(hasClicked, Is.EqualTo(true));
    }

    [Test]
    public void ShouldExecuteActionWhenClicked()
    {
        bool hasClicked = false;

        var vm = new ButtonConfigViewModel
        {
            ClickFunc = () =>
            {
                hasClicked = true;
                return Task.CompletedTask;
            }
        };

        vm.Click();

        Assert.That(hasClicked, Is.EqualTo(true));
    }

    [Test]
    public void ShouldHaveChangesWhenCallingSetHasChanges()
    {
        var vm = new ButtonConfigViewModel
        {
            ClickFunc = () =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            }
        };

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CheckBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.SetHasChanges();

        Assert.That(vm.HasChanges, Is.EqualTo(true));
        Assert.That(hasChangesEventRaised, Is.EqualTo(true));
    }

    [Test]
    public async Task ShouldNotHaveChangesAfterSaving()
    {
        var vm = new ButtonConfigViewModel
        {
            ClickFunc = () =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            }
        };

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CheckBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.SetHasChanges();
        await vm.Save();

        Assert.That(vm.HasChanges, Is.EqualTo(false));
        Assert.That(hasChangesEventRaised, Is.EqualTo(true));
    }

    [Test]
    public void ShouldNotHaveChangesAfterDiscarding()
    {
        var vm = new ButtonConfigViewModel
        {
            ClickFunc = () =>
            {
                Assert.Fail();
                return Task.CompletedTask;
            }
        };

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CheckBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.SetHasChanges();
        vm.DiscardChanges();

        Assert.That(vm.HasChanges, Is.EqualTo(false));
        Assert.That(hasChangesEventRaised, Is.EqualTo(true));
    }
}