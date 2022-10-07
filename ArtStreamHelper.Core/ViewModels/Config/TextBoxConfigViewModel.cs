using ArtStreamHelper.Core.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArtStreamHelper.Core.ViewModels.Config;

public partial class TextBoxConfigViewModel : ConfigViewModelBase
{
    [ObservableProperty]
    private string _pendingText;

    public TextBoxConfigViewModel(string initialValue)
    {
        Text = PendingText = initialValue;
    }

    public string ValidTextRegex { get; set; }

    public string Text { get; set; }

    public override bool HasChanges => PendingText != Text;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(PendingText):
                OnPropertyChanged(nameof(IsValid));
                OnPropertyChanged(nameof(HasChanges));
                break;
        }
    }

    public override bool IsValid =>
        string.IsNullOrEmpty(ValidTextRegex) || new Regex(ValidTextRegex).Match(PendingText).Success;

    public override async Task Save()
    {
        if (HasChanges && IsValid)
        {
            Text = PendingText;
            OnPropertyChanged(nameof(HasChanges));

            await base.Save();
        }
    }

    public override void DiscardChanges()
    {
        PendingText = Text;
    }
}