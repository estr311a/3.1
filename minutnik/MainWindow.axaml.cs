using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace minutnik;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
    private int _workMinutes = 25;
    private int _breakMinutes = 5;
    private int _remainingSeconds = 25 * 60;
    private int _cycles = 0;
    private bool _isWorkSession = true;
    private bool _isRunning = false;
    private bool _isPaused = false;

    public MainWindow()
    {
        InitializeComponent();

        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;

        UpdateTimerDisplay();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        if (_remainingSeconds > 0)
        {
            _remainingSeconds--;
            UpdateTimerDisplay();
        }

        if (_remainingSeconds <= 0)
        {
            if (_isWorkSession)
            {
                // Automatyczne rozpoczęcie przerwy po pracy
                _isWorkSession = false;
                _remainingSeconds = _breakMinutes * 60;
                StatusTextBlock.Text = "Stan: Przerwa";
                UpdateTimerDisplay();
            }
            else
            {
                // Ukończenie przerwy i pełnego cyklu
                _cycles++;
                CyclesTextBlock.Text = $"Ukończone cykle: {_cycles}";

                // Przejście do kolejnego cyklu pracy
                _isWorkSession = true;
                _remainingSeconds = _workMinutes * 60;
                StatusTextBlock.Text = "Stan: Praca";
                UpdateTimerDisplay();
            }
        }
    }

    private void StartPauseButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isRunning)
        {
            PauseTimer();
        }
        else
        {
            StartTimer();
        }
    }

    private void StartTimer()
    {
        if (!_isPaused)
        {
            if (!int.TryParse(WorkMinutesTextBox.Text?.Trim(), out int work) || work <= 0)
            {
                StatusTextBlock.Text = "Błąd: Nieprawidłowy czas pracy!";
                return;
            }

            if (!int.TryParse(BreakMinutesTextBox.Text?.Trim(), out int breakTime) || breakTime <= 0)
            {
                StatusTextBlock.Text = "Błąd: Nieprawidłowy czas przerwy!";
                return;
            }

            _workMinutes = work;
            _breakMinutes = breakTime;
            _remainingSeconds = (_isWorkSession ? _workMinutes : _breakMinutes) * 60;
        }

        _isRunning = true;
        _isPaused = false;
        _timer.Start();

        StartPauseButton.Content = "Pauza";
        WorkMinutesTextBox.IsEnabled = false;
        BreakMinutesTextBox.IsEnabled = false;
        StatusTextBlock.Text = _isWorkSession ? "Stan: Praca" : "Stan: Przerwa";
        UpdateTimerDisplay();
    }

    private void PauseTimer()
    {
        _isRunning = false;
        _isPaused = true;
        _timer.Stop();

        StartPauseButton.Content = "Start";
        StatusTextBlock.Text = (_isWorkSession ? "Stan: Praca" : "Stan: Przerwa") + " (Wstrzymano)";
    }

    private void ResetButton_Click(object? sender, RoutedEventArgs e)
    {
        ResetTimer();
    }

    private void ResetTimer()
    {
        _timer.Stop();
        _isRunning = false;
        _isPaused = false;
        _isWorkSession = true;

        if (int.TryParse(WorkMinutesTextBox.Text?.Trim(), out int work) && work > 0)
        {
            _workMinutes = work;
        }
        else
        {
            _workMinutes = 25;
            WorkMinutesTextBox.Text = "25";
        }

        if (int.TryParse(BreakMinutesTextBox.Text?.Trim(), out int breakTime) && breakTime > 0)
        {
            _breakMinutes = breakTime;
        }
        else
        {
            _breakMinutes = 5;
            BreakMinutesTextBox.Text = "5";
        }

        _remainingSeconds = _workMinutes * 60;

        StartPauseButton.Content = "Start";
        WorkMinutesTextBox.IsEnabled = true;
        BreakMinutesTextBox.IsEnabled = true;
        StatusTextBlock.Text = "Stan: Praca";
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        int minutes = _remainingSeconds / 60;
        int seconds = _remainingSeconds % 60;
        TimerTextBlock.Text = $"{minutes:D2}:{seconds:D2}";
    }
}
