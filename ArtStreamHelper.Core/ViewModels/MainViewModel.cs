using ArtStreamHelper.Core.Models;
using ArtStreamHelper.Core.Services;
using ArtStreamHelper.Core.ViewModels.Config;
using ArtStreamHelper.Core.ViewModels.Settings;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ArtStreamHelper.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private const int DefaultPromptTimeMinutes = 10;
    private const int DefaultPromptCooldownSeconds = 10;

    private static readonly TimeSpan PromptCooldown = TimeSpan.FromSeconds(DefaultPromptCooldownSeconds);

    private readonly IFileService _fileService;
    private readonly IPlatformServices _platformServices;

    private readonly Timer _promptCooldownTimer;
    private readonly Timer _promptDrawingTimer;
    private readonly Random _random;

    private string _previousPrompt;
    private int _drawingSecondsRemaining;
    private int _cooldownSecondsRemaining;
    private bool _allowRepeats;
    private bool _doNotRandomize;
    private List<string> _unsavedPromptList;
    private TimeSpan _promptTime = TimeSpan.FromMinutes(DefaultPromptTimeMinutes);

    [ObservableProperty] private string _promptPrefix = string.Empty;
    [ObservableProperty] private IReadOnlyCollection<string> _originalPromptList;
    [ObservableProperty] private ObservableCollection<RemainingPromptModel> _promptList;
    [ObservableProperty] private string _promptText;
    [ObservableProperty] private string _timeText;
    [ObservableProperty] private List<ConfigViewModelBase> _configs;
    [ObservableProperty] private bool _started;

    public MainViewModel(IFileService fileService, IPlatformServices platformServices)
    {
        _fileService = fileService;
        _platformServices = platformServices;

        _promptCooldownTimer = new Timer
        {
            Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
        };
        _promptDrawingTimer = new Timer
        {
            Interval = TimeSpan.FromSeconds(1).TotalMilliseconds
        };

        _promptCooldownTimer.Elapsed += (sender, o) =>
        {
            _cooldownSecondsRemaining--;

            if (_cooldownSecondsRemaining == 0)
            {
                _promptCooldownTimer.Stop();
                _promptDrawingTimer.Start();
            }
        };
        _promptDrawingTimer.Elapsed += (sender, args) =>
        {
            _platformServices.BeginInvokeOnMainThread(() =>
            {
                SetSecondsRemaining(_drawingSecondsRemaining - 1);
                if (_drawingSecondsRemaining == 0)
                {
                    _promptDrawingTimer.Stop();
                    CyclePrompt();
                }
            });
        };

        OriginalPromptList = new List<string>
        {
            "Animal", "Clothing Item", "Color", "Adjective", "Object"
        };
        ResetPromptList();
        _random = new Random();

        SetUpSettings();
    }
    
    private void SetUpSettings()
    {
        var pickPromptsBtn = new ButtonConfigViewModel
        {
            Name = "Pick prompt file",
            OnSaved = () =>
            {
                if (Started)
                {
                    Started = false;
                }
                OriginalPromptList = _unsavedPromptList;
                ResetPromptList();
                return Task.CompletedTask;
            }
        };
        pickPromptsBtn.ClickFunc = async () =>
        {
            await PickPromptFileAsync();
            if (OriginalPromptList != _unsavedPromptList)
            {
                pickPromptsBtn.SetHasChanges();
            }
        };

        var allowRepeatsToggle = new CheckBoxConfigViewModel(_allowRepeats)
        {
            Name = "Allow repeats"
        };
        allowRepeatsToggle.OnSaved = () =>
        {
            _allowRepeats = allowRepeatsToggle.IsChecked;
            return Task.CompletedTask;
        };

        var minutesText = new TextBoxConfigViewModel(DefaultPromptTimeMinutes.ToString())
        {
            Name = "Prompt time in mins",
            ValidTextRegex = @"\d+"
        };
        minutesText.OnSaved = () =>
        {
            _promptTime = TimeSpan.FromMinutes(double.Parse(minutesText.Text));
            return Task.CompletedTask;
        };

        var promptPrefix = new TextBoxConfigViewModel(PromptPrefix)
        {
            Name = "Prompt prefix"
        };
        promptPrefix.OnSaved = () =>
        {
            PromptPrefix = promptPrefix.Text;
            return Task.CompletedTask;
        };

        var doNotRandomize = new CheckBoxConfigViewModel(_allowRepeats)
        {
            Name = "Do not randomize"
        };
        doNotRandomize.OnSaved = () =>
        {
            _doNotRandomize = doNotRandomize.IsChecked;
            return Task.CompletedTask;
        };


        Configs = new List<ConfigViewModelBase>
        {
            pickPromptsBtn, allowRepeatsToggle, doNotRandomize, minutesText, promptPrefix
        };

        Configs.ForEach(config => config.PropertyChanged += (sender, args) =>
        {
            switch (args.PropertyName)
            {
                case nameof(ConfigViewModelBase.IsValid):
                case nameof(ConfigViewModelBase.HasChanges):
                    SaveSettingsCommand.NotifyCanExecuteChanged();
                    DiscardSettingsCommand.NotifyCanExecuteChanged();
                    break;
            }
        });
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        switch (e.PropertyName)
        {
            case nameof(Started):
                if (Started)
                {
                    CyclePrompt();
                }
                else
                {
                    StopAndClearAll();
                }
                CycleClickedCommand.NotifyCanExecuteChanged();
                break;
        }
    }

    [RelayCommand]
    private async Task PickPromptFileAsync()
    {
        var stream = await _fileService.PickFileAsync(new List<string> { ".txt" });
        if (stream != null)
        {
            using var streamReader = new StreamReader(stream);
            var text = await streamReader.ReadToEndAsync();
            _unsavedPromptList = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        }
    }

    [RelayCommand]
    private void Toggle()
    {
        Started = !Started;
    }

    private void StopAndClearAll()
    {
        ResetPromptList();
        PromptText = " ";
        TimeText = " ";
        _promptDrawingTimer.Stop();
        _promptCooldownTimer.Stop();
    }

    private Task DeletePrompt(RemainingPromptModel selectedPrompt)
    {
        PromptList.Remove(selectedPrompt);
        return Task.CompletedTask;
    }

    private Task SetPromptAsNext(RemainingPromptModel selectedPrompt)
    {
        foreach (var prompt in PromptList)
        {
            if (prompt != selectedPrompt)
            {
                prompt.IsNextRequested = false;
            }
        }

        return Task.CompletedTask;
    }

    private void ResetPromptList()
    {
        PromptList = new ObservableCollection<RemainingPromptModel>(OriginalPromptList.Select(p => new RemainingPromptModel
        {
            Name = p,
            DeleteFunc = DeletePrompt,
            SetAsNextFunc = SetPromptAsNext
        }));
    }

    [RelayCommand(CanExecute = nameof(Started))]
    private void OnCycleClicked()
    {
        _promptDrawingTimer.Stop();
        _promptCooldownTimer.Stop();
        
        CyclePrompt();
    }

    [RelayCommand(CanExecute = nameof(CanSaveSettings))]
    private async Task SaveSettings()
    {
        foreach (var config in Configs)
        {
            await config.Save();
        }

        if (Started)
        {
            Started = false;
        }

        DiscardSettingsCommand.NotifyCanExecuteChanged();
    }

    private bool CanSaveSettings()
    {
        return Configs.Any(config => config.HasChanges && config.IsValid);
    }

    [RelayCommand(CanExecute = nameof(HasSettingsChanges))]
    private void DiscardSettings()
    {
        foreach (var config in Configs.Where(config => config.HasChanges))
        {
            config.DiscardChanges();
        }


        SaveSettingsCommand.NotifyCanExecuteChanged();
    }

    private bool HasSettingsChanges()
    {
        return Configs.Any(config => config.HasChanges);
    }

    private void StartCooldownTimer()
    {
        _cooldownSecondsRemaining = (int)PromptCooldown.TotalSeconds;
        SetSecondsRemaining((int)_promptTime.TotalSeconds);
        _promptCooldownTimer.Start();
    }

    private void CyclePrompt()
    {
        string nextPrompt = null;
        if (PromptList.Count > 0)
        {
            var nextPromptModel = PromptList.FirstOrDefault(p => p.IsNextRequested);
            if (nextPromptModel != null)
            {
                nextPrompt = nextPromptModel.Name;
                nextPromptModel.IsNextRequested = false;
                if (!_allowRepeats)
                {
                    PromptList.Remove(nextPromptModel);
                }
            }
            else if (_allowRepeats)
            {
                if (PromptList.Count == 1)
                {
                    nextPrompt = PromptList[0].Name;
                }
                else
                {
                    if (_doNotRandomize)
                    {
                        var nextIndex = PromptList.ToList().FindIndex(item => item.Name == _previousPrompt) + 1;
                        if (nextIndex >= PromptList.Count)
                        {
                            nextIndex = 0;
                        }
                        
                        nextPrompt = PromptList[nextIndex].Name;
                    }
                    else
                    {
                        var availablePromptsExcludingPrevious = PromptList.Where(prompt => prompt.Name != _previousPrompt).ToList();
                        var nextIndex = _random.Next(availablePromptsExcludingPrevious.Count);
                        nextPrompt = availablePromptsExcludingPrevious[nextIndex].Name;
                    }
                }
            }
            else
            {
                if (_doNotRandomize)
                {
                    var nextIndex = PromptList.ToList().FindIndex(item => item.Name == _previousPrompt) + 1;
                    if (nextIndex >= PromptList.Count)
                    {
                        nextIndex = 0;
                    }

                    nextPrompt = PromptList[nextIndex].Name;
                }
                else
                {
                    nextPromptModel = PromptList[_random.Next(PromptList.Count)];
                    nextPrompt = nextPromptModel.Name;
                    PromptList.Remove(nextPromptModel);
                }
            }
        }

        if (nextPrompt == null)
        {
            Started = false;
            PromptText = "No More.";
        }
        else
        {
            PromptText = string.IsNullOrEmpty(PromptPrefix) ? nextPrompt : $"{PromptPrefix}: {nextPrompt}";

            SetSecondsRemaining((int)_promptTime.TotalSeconds);
            StartCooldownTimer();
        }

        _previousPrompt = nextPrompt;
    }

    private void SetSecondsRemaining(int totalSeconds)
    {
        _drawingSecondsRemaining = totalSeconds;
        int minutes = _drawingSecondsRemaining / 60;
        int seconds = _drawingSecondsRemaining % 60;
        TimeText = $"Time: {minutes}:{seconds:00}";
    }
}