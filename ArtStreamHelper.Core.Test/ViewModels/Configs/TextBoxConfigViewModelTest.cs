using ArtStreamHelper.Core.ViewModels.Config;

namespace ArtStreamHelper.Core.Test.ViewModels.Configs;

public class TextBoxConfigViewModelTest
{
    [Test]
    public void ShouldInitCorrectly()
    {
        var originalText = "test";
        var vm = new TextBoxConfigViewModel(originalText);

        Assert.Multiple(() =>
        {
            Assert.That(vm.Text, Is.EqualTo(originalText));
            Assert.That(vm.PendingText, Is.EqualTo(originalText));
            Assert.That(vm.HasChanges, Is.EqualTo(false));
        });
    }

    [Test]
    public void ShouldHaveChangesAfterSettingText()
    {
        var originalText = "test";
        var newText = "new text";
        var vm = new TextBoxConfigViewModel(originalText);

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TextBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.PendingText = newText;

        Assert.Multiple(() =>
        {
            Assert.That(vm.PendingText, Is.EqualTo(newText));
            Assert.That(vm.Text, Is.EqualTo(originalText));
            Assert.That(vm.HasChanges, Is.EqualTo(true));
            Assert.That(hasChangesEventRaised, Is.EqualTo(true));
        });
    }

    [Test]
    public void ShouldNotHaveChangesAfterSaving()
    {
        var originalText = "test";
        var newText = "new text";
        var vm = new TextBoxConfigViewModel(originalText);

        vm.PendingText = newText;

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TextBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.Save();

        Assert.Multiple(() =>
        {
            Assert.That(vm.PendingText, Is.EqualTo(newText));
            Assert.That(vm.Text, Is.EqualTo(newText));
            Assert.That(vm.HasChanges, Is.EqualTo(false));
            Assert.That(hasChangesEventRaised, Is.EqualTo(true));
        });
    }

    [Test]
    public void ShouldNotHaveChangesAfterDiscarding()
    {
        var originalText = "test";
        var newText = "new text";
        var vm = new TextBoxConfigViewModel(originalText);

        vm.PendingText = newText;

        bool hasChangesEventRaised = false;

        vm.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(TextBoxConfigViewModel.HasChanges))
            {
                hasChangesEventRaised = true;
            }
        };

        vm.DiscardChanges();

        Assert.Multiple(() =>
        {
            Assert.That(vm.PendingText, Is.EqualTo(originalText));
            Assert.That(vm.Text, Is.EqualTo(originalText));
            Assert.That(vm.HasChanges, Is.EqualTo(false));
            Assert.That(hasChangesEventRaised, Is.EqualTo(true));
        });
    }
}