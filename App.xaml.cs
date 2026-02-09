using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;
using WinAutoDarkMode.Core;
using WinAutoDarkMode.UI;
using Application = System.Windows.Application;

namespace WinAutoDarkMode
{
    public partial class App : System.Windows.Application
    {
        private static Mutex? _mutex;
        private TrayIcon? _trayIcon;
        private ThemeManager? _themeManager;
        private ConfigManager? _configManager;
        private ScheduleManager? _scheduleManager;
        private StartupManager? _startupManager;
        private SettingsWindow? _settingsWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            const string mutexName = "WinAutoDarkMode_SingleInstance";
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                System.Windows.MessageBox.Show("WinAutoDarkMode 已经在运行中", "提示", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }

            InitializeComponents();
            UpdateAppTheme();
            
            SystemEvents.UserPreferenceChanged += (s, ev) => {
                if (ev.Category == UserPreferenceCategory.General) {
                    UpdateAppTheme();
                }
            };
        }

        private void UpdateAppTheme()
        {
            if (_themeManager == null) return;

            bool isLight = _themeManager.IsLightMode();
            
            if (isLight)
            {
                ApplyTheme(System.Windows.Media.Color.FromRgb(243, 243, 243), 
                           System.Windows.Media.Colors.White,
                           System.Windows.Media.Color.FromRgb(26, 26, 26),
                           System.Windows.Media.Color.FromRgb(102, 102, 102),
                           System.Windows.Media.Color.FromRgb(229, 229, 229)
                );
            }
            else
            {
                ApplyTheme(System.Windows.Media.Color.FromRgb(32, 32, 32),
                           System.Windows.Media.Color.FromRgb(45, 45, 45),
                           System.Windows.Media.Colors.White,
                           System.Windows.Media.Color.FromRgb(170, 170, 170),
                           System.Windows.Media.Color.FromRgb(60, 60, 60)
                );
            }
        }

        private void ApplyTheme(System.Windows.Media.Color winBg, System.Windows.Media.Color cardBg, System.Windows.Media.Color mainText, System.Windows.Media.Color secText, System.Windows.Media.Color border)
        {
            winBg.A = 180; 
            this.Resources["WindowBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(winBg);
            this.Resources["CardBackgroundBrush"] = new System.Windows.Media.SolidColorBrush(cardBg);
            this.Resources["MainTextBrush"] = new System.Windows.Media.SolidColorBrush(mainText);
            this.Resources["SecondaryTextBrush"] = new System.Windows.Media.SolidColorBrush(secText);
            this.Resources["BorderBrush"] = new System.Windows.Media.SolidColorBrush(border);
        }

        private void InitializeComponents()
        {
            _themeManager = new ThemeManager();
            _configManager = new ConfigManager();
            _startupManager = new StartupManager();
            _scheduleManager = new ScheduleManager(_themeManager, _configManager);

            _trayIcon = new TrayIcon(_themeManager, _scheduleManager, _startupManager);
            _trayIcon.SettingsRequested += OnSettingsRequested;
            _trayIcon.ExitRequested += OnExitRequested;

            _scheduleManager.Start();
            _scheduleManager.ThemeChanged += (s, theme) => {
                UpdateAppTheme();
                _trayIcon?.RefreshIcon();
            };

            var config = _configManager.LoadConfig();
            if (config.StartWithWindows && !_startupManager.IsStartupEnabled())
            {
                _startupManager.EnableStartup();
            }
        }

        private void OnSettingsRequested(object? sender, EventArgs e)
        {
            if (_settingsWindow == null || !_settingsWindow.IsVisible)
            {
                _settingsWindow = new SettingsWindow(_configManager!, _scheduleManager!, _startupManager!);
                _settingsWindow.Show();
            }
            else
            {
                _settingsWindow.Activate();
            }
        }

        private void OnExitRequested(object? sender, EventArgs e)
        {
            Shutdown();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _scheduleManager?.Dispose();
            _trayIcon?.Dispose();
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}
