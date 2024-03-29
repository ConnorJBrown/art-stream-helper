﻿using ArtStreamHelper.Core.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ArtStreamHelper.Core.ViewModels.Config;

public partial class CheckBoxConfigViewModel : ConfigViewModelBase
{
    [ObservableProperty]
    private bool _pendingIsChecked;

    public CheckBoxConfigViewModel(bool isChecked)
    {
        IsChecked = PendingIsChecked = isChecked;
    }

    public bool IsChecked { get; private set; }

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

    public override bool IsValid => true;

    public override async Task Save()
    {
        if (HasChanges && IsValid)
        {
            IsChecked = PendingIsChecked;
            OnPropertyChanged(nameof(HasChanges));

            await base.Save();
        }
    }

    public override void DiscardChanges()
    {
        PendingIsChecked = IsChecked;
    }
}