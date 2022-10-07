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
    private const int DefaultPromptTimeMinutes = 30;
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
    private List<string> _unsavedPromptList;
    private TimeSpan _promptTime = TimeSpan.FromMinutes(DefaultPromptTimeMinutes);

    [ObservableProperty]
    private IReadOnlyCollection<string> _originalPromptList;
    [ObservableProperty]
    private ObservableCollection<string> _promptList;

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

        _originalPromptList = new List<string>
        {
            "Animal", "Clothing Item", "Color", "Adjective", "Object"
        };
        PromptList = new ObservableCollection<string>(_originalPromptList);
        _random = new Random();

        var btn = new ButtonConfigViewModel
        {
            Name = "Pick prompt file",
            OnSaved = () =>
            {
                if (Started)
                {
                    Started = false;
                }
                _originalPromptList = _unsavedPromptList;
                PromptList = new ObservableCollection<string>(_originalPromptList);
                return Task.CompletedTask;
            }
        };
        btn.ClickFunc = async () =>
        {
            await PickPromptFileAsync();
            btn.SetHasChanges();
        };

        var checkBox = new CheckBoxConfigViewModel(_allowRepeats)
        {
            Name = "Allow repeats"
        };
        checkBox.OnSaved = () =>
        {
            _allowRepeats = checkBox.IsChecked;
            return Task.CompletedTask;
        };

        var textBox = new TextBoxConfigViewModel(DefaultPromptTimeMinutes.ToString())
        {
            Name = "Prompt time in mins",
            ValidTextRegex = @"\d+"
        };
        textBox.OnSaved = () =>
        {
            _promptTime = TimeSpan.FromMinutes(double.Parse(textBox.Text));
            return Task.CompletedTask;
        };

        Configs = new List<ConfigViewModelBase>
        {
            btn, checkBox, textBox
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

    [ObservableProperty]
    private string _promptText;
    [ObservableProperty]
    private string _timeText;
    [ObservableProperty]
    private List<ConfigViewModelBase> _configs;
    [ObservableProperty]
    private bool _started;

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
            _unsavedPromptList = text.Split(Environment.NewLine).ToList();
        }
    }

    [RelayCommand]
    private void Toggle()
    {
        Started = !Started;
    }

    private void StopAndClearAll()
    {
        PromptList = new ObservableCollection<string>(_originalPromptList);
        PromptText = " ";
        TimeText = " ";
        _promptDrawingTimer.Stop();
        _promptCooldownTimer.Stop();
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
            if (_allowRepeats)
            {
                if (PromptList.Count == 1)
                {
                    nextPrompt = PromptList[0];
                }
                else
                {
                    var availablePromptsExcludingPrevious = PromptList.Where(prompt => prompt != _previousPrompt).ToList();
                    nextPrompt = availablePromptsExcludingPrevious[_random.Next(availablePromptsExcludingPrevious.Count)];
                }
            }
            else
            {
                nextPrompt = PromptList[_random.Next(PromptList.Count)];
                PromptList.Remove(nextPrompt);
            }
        }

        if (nextPrompt == null)
        {
            Started = false;
            PromptText = "No More Prompts.";
        }
        else
        {
            PromptText = $"Prompt: {nextPrompt}";

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