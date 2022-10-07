using ArtStreamHelper.Core.ViewModels.Settings;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace ArtStreamHelper.Core.ViewModels.Config;

public partial class ButtonConfigViewModel : ConfigViewModelBase
{
    private bool _hasChanges;

    public Func<Task> ClickFunc { get; set; }

    [RelayCommand]
    public Task Click()
    {
        return ClickFunc();
    }

    public override bool HasChanges => _hasChanges;

    public void SetHasChanges()
    {
        _hasChanges = true;
        OnPropertyChanged(nameof(HasChanges));
    }

    public override bool IsValid => true;

    public override async Task Save()
    {
        if (HasChanges && IsValid)
        {
            _hasChanges = false;
            OnPropertyChanged(nameof(HasChanges));

            await base.Save();
        }
    }

    public override void DiscardChanges()
    {
        _hasChanges = false;
        OnPropertyChanged(nameof(HasChanges));
    }
}