using ArtStreamHelper.Core.ViewModels.Config;

namespace ArtStreamHelper.Core.Test.ViewModels.Configs;

public class CheckBoxConfigViewModelTest
{
    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void ShouldInitCorrectly(bool initialCheckedValue)
    {
        var vm = new CheckBoxConfigViewModel(initialCheckedValue);

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsChecked, Is.EqualTo(initialCheckedValue));
            Assert.That(vm.PendingIsChecked, Is.EqualTo(initialCheckedValue));
        });
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void ShouldHaveChangesAfterSettingPendingChecked(bool initialCheckedValue, bool newCheckedValue)
    {
        var vm = new CheckBoxConfigViewModel(initialCheckedValue);

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CheckBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.PendingIsChecked = newCheckedValue;

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsChecked, Is.EqualTo(initialCheckedValue));
            Assert.That(vm.PendingIsChecked, Is.EqualTo(newCheckedValue));
            Assert.That(vm.HasChanges, Is.EqualTo(true));
            Assert.That(hasChangesEventRaised, Is.EqualTo(true));
        });
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void ShouldNotHaveChangesAfterSaving(bool initialCheckedValue, bool newCheckedValue)
    {
        var vm = new CheckBoxConfigViewModel(initialCheckedValue)
        {
            PendingIsChecked = newCheckedValue
        };

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CheckBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.Save();

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsChecked, Is.EqualTo(newCheckedValue));
            Assert.That(vm.PendingIsChecked, Is.EqualTo(newCheckedValue));
            Assert.That(vm.HasChanges, Is.EqualTo(false));
            Assert.That(hasChangesEventRaised, Is.EqualTo(true));
        });
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, true)]
    public void ShouldNotHaveChangesAfterDiscarding(bool initialCheckedValue, bool newCheckedValue)
    {
        var vm = new CheckBoxConfigViewModel(initialCheckedValue)
        {
            PendingIsChecked = newCheckedValue
        };

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CheckBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.DiscardChanges();

        Assert.Multiple(() =>
        {
            Assert.That(vm.IsChecked, Is.EqualTo(initialCheckedValue));
            Assert.That(vm.PendingIsChecked, Is.EqualTo(initialCheckedValue));
            Assert.That(vm.HasChanges, Is.EqualTo(false));
            Assert.That(hasChangesEventRaised, Is.EqualTo(true));
        });
    }
}