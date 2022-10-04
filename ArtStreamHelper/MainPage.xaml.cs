using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArtStreamHelper
{
    public sealed partial class MainPage : Page
    {
        private static readonly TimeSpan PromptCooldown = TimeSpan.FromSeconds(10);
        private static readonly TimeSpan PromptTime = TimeSpan.FromMinutes(30);

        private readonly DispatcherTimer _promptCooldownTimer;
        private readonly DispatcherTimer _promptDrawingTimer;
        private readonly Random _random;

        private string _lastPrompt;
        private List<string> _promptList;
        private int _drawingSecondsRemaining;
        private int _cooldownSecondsRemaining;
        private bool _started;

        public MainPage()
        {
            InitializeComponent();

            _promptCooldownTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _promptCooldownTimer.Tick += (sender, o) =>
            {
                _cooldownSecondsRemaining--;

                if (_cooldownSecondsRemaining == 0)
                {
                    _promptCooldownTimer.Stop();
                    _promptDrawingTimer.Start();
                }
            };

            _promptDrawingTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _promptDrawingTimer.Tick += (sender, args) =>
            {
                SetSecondsRemaining(_drawingSecondsRemaining - 1);
                if (_drawingSecondsRemaining == 0)
                {
                    _promptDrawingTimer.Stop();
                    CyclePrompt();
                    StartCooldownTimer();
                }
            };

            _promptList = new List<string>
            {
                "Animal", "Clothing Item", "Color", "Adjective", "Object"
            };
            _random = new Random();

            UpdatePromptMaxWidth();

            DataContext = this;
        }

        private void UpdatePromptMaxWidth()
        {
            double maxWidth = GetMaxTextWidth();
            PromptText.Width = maxWidth;
        }

        private double GetMaxTextWidth()
        {
            double maxWidth = 0;

            if (Resources.TryGetValue("MyOutlinedTextBlockStyle", out object value) && value is Style style)
            {
                foreach (var prompt in _promptList)
                {
                    OutlinedTextBlock text = new OutlinedTextBlock()
                    {
                        Style = style,
                        Text = $"Prompt: {prompt}"
                    };

                    text.Measure(new Size(4000, 4000));
                    if (maxWidth < text.DesiredSize.Width)
                    {
                        maxWidth = text.DesiredSize.Width;
                    }
                }
            }

            return maxWidth;
        }

        private void OnToggleClicked(object sender, RoutedEventArgs e)
        {
            Toggle();
        }

        private void Toggle()
        {
            _started = !_started;
            ToggleButtonText.Text = _started ? "Stop" : "Start";

            if (_started)
            {
                CyclePrompt();
                StartCooldownTimer();
            }
            else
            {
                PromptText.Text = " ";
                TimeText.Text = " ";
                _promptDrawingTimer.Stop();
                _promptCooldownTimer.Stop();
            }
        }

        private void OnCycleClicked(object sender, RoutedEventArgs e)
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
            PromptText.Text = $"Prompt: {_lastPrompt}";
            PromptText.InvalidateMeasure();

            SetSecondsRemaining((int)PromptTime.TotalSeconds);
        }

        private void SetSecondsRemaining(int totalSeconds)
        {
            _drawingSecondsRemaining = totalSeconds;
            int minutes = _drawingSecondsRemaining / 60;
            int seconds = _drawingSecondsRemaining % 60;
            TimeText.Text = $"Time: {minutes}:{seconds:00}";
            TimeText.InvalidateMeasure();
        }

        private async void PickPromptFile(object sender, RoutedEventArgs e)
        {
            if (_started)
            {
                Toggle();
            }

            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".txt");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                string text = await FileIO.ReadTextAsync(file);
                _promptList = text.Split(Environment.NewLine).ToList();
                UpdatePromptMaxWidth();
            }
        }
    }
}
