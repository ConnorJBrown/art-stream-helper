using CommunityToolkit.Mvvm.ComponentModel;

namespace ArtStreamHelper.Core.ViewModels.Settings;

public abstract partial class ConfigViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _name = "Test";

    public abstract bool HasChanges { get; }

    public abstract void Save();

    public abstract void DiscardChanges();
}