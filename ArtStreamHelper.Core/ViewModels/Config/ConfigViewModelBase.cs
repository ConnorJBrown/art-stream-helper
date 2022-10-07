using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace ArtStreamHelper.Core.ViewModels.Settings;

public abstract partial class ConfigViewModelBase : ObservableObject
{
    [ObservableProperty]
    private string _name;

    public Func<Task> OnSaved { get; set; }

    public abstract bool HasChanges { get; }

    public abstract bool IsValid { get; }

    public virtual async Task Save()
    {
        if (OnSaved != null)
        {
            await OnSaved();
        }
    }

    public abstract void DiscardChanges();
}