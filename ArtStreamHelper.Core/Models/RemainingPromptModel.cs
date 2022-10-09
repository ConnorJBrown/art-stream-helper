using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ArtStreamHelper.Core.Models;

public partial class RemainingPromptModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isNextRequested;

    public Func<RemainingPromptModel, Task> DeleteFunc { get; set; }
    public Func<RemainingPromptModel, Task> SetAsNextFunc { get; set; }

    [RelayCommand]
    private Task Delete()
    {
        return DeleteFunc(this);
    }

    [RelayCommand]
    private Task OnNextRequestedChanged()
    {
        return SetAsNextFunc(this);
    }
}