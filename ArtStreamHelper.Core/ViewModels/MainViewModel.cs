using ArtStreamHelper.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace ArtStreamHelper.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IFileService _fileService;
    private readonly IPlatformServices _platformServices;

    private static readonly TimeSpan PromptCooldown = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan PromptTime = TimeSpan.FromMinutes(30);

    private readonly Timer _promptCooldownTimer;
    private readonly Timer _promptDrawingTimer;
    private readonly Random _random;

    private string _lastPrompt;
    private List<string> _promptList;
    private int _drawingSecondsRemaining;
    private int _cooldownSecondsRemaining;
    private bool _started;

    public IReadOnlyCollection<string> PromptList => _promptList;

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
                    StartCooldownTimer();
                }
            });
        };

        _promptList = new List<string>
        {
            "Animal", "Clothing Item", "Color", "Adjective", "Object"
        };
        _random = new Random();
    }

    [ObservableProperty]
    private string _promptText;
    [ObservableProperty]
    private string _timeText;
    [ObservableProperty]
    private string _toggleButtonText = "Start";

    [RelayCommand]
    private async Task PickPromptFileAsync()
    {
        if (_started)
        {
            Toggle();
        }

        var stream = await _fileService.PickFileAsync(new List<string> { ".txt" });
        if (stream != null)
        {
            using var streamReader = new StreamReader(stream);
            var text = await streamReader.ReadToEndAsync();
            _promptList = text.Split(Environment.NewLine).ToList();
        }
    }

    [RelayCommand]
    private void Toggle()
    {
        _started = !_started;
        ToggleButtonText = _started ? "Stop" : "Start";

        if (_started)
        {
            CyclePrompt();
            StartCooldownTimer();
        }
        else
        {
            PromptText = " ";
            TimeText = " ";
            _promptDrawingTimer.Stop();
            _promptCooldownTimer.Stop();
        }
    }

    [RelayCommand]
    private void OnCycleClicked()
    {
        _promptDrawingTimer.Stop();
        _promptCooldownTimer.Stop();
        
        CyclePrompt();
        StartCooldownTimer();
    }

    private void StartCooldownTimer()
    {
        _cooldownSecondsRemaining = (int)PromptCooldown.TotalSeconds;
        SetSecondsRemaining((int)PromptTime.TotalSeconds);
        _promptCooldownTimer.Start();
    }

    private void CyclePrompt()
    {
        List<string> availablePrompts = _promptList.Where(prompt => prompt != _lastPrompt).ToList();
        _lastPrompt = availablePrompts[_random.Next(availablePrompts.Count)];
        PromptText = $"Prompt: {_lastPrompt}";

        SetSecondsRemaining((int)PromptTime.TotalSeconds);
    }

    private void SetSecondsRemaining(int totalSeconds)
    {
        _drawingSecondsRemaining = totalSeconds;
        int minutes = _drawingSecondsRemaining / 60;
        int seconds = _drawingSecondsRemaining % 60;
        TimeText = $"Time: {minutes}:{seconds:00}";
    }
}