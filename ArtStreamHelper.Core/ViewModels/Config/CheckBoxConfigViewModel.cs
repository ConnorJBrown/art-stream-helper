using System.ComponentModel;
using ArtStreamHelper.Core.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ArtStreamHelper.Core.ViewModels.Config;

public partial class CheckBoxConfigViewModel : ConfigViewModelBase
{
    public CheckBoxConfigViewModel(bool isChecked)
    {
        IsChecked = PendingIsChecked = isChecked;
    }

    public bool IsChecked { get; private set; }

    [ObservableProperty]
    private bool _pendingIsChecked;

    public override bool HasChanges => IsChecked != PendingIsChecked;

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(PendingIsChecked):
                OnPropertyChanged(nameof(HasChanges));
                break;
        }
    }

    public override void Save()
    {
        IsChecked = PendingIsChecked;
        OnPropertyChanged(nameof(HasChanges));
    }

    public override void DiscardChanges()
    {
        PendingIsChecked = IsChecked;
    }
}