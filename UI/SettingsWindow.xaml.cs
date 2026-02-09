using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WinAutoDarkMode.Core;
using WinAutoDarkMode.Models;

namespace WinAutoDarkMode.UI
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigManager _configManager;
        private readonly ScheduleManager _scheduleManager;
        private readonly StartupManager _startupManager;
        private AppConfig _config;

        public SettingsWindow(ConfigManager configManager, ScheduleManager scheduleManager, StartupManager startupManager)
        {
            InitializeComponent();
            
            _configManager = configManager;
            _scheduleManager = scheduleManager;
            _startupManager = startupManager;
            _config = _configManager.LoadConfig();

            this.Loaded += (s, e) => {
                UpdateBlur();
            };

            LoadSettings();
            BindTimeEvents();
        }

        private void UpdateBlur()
        {
            bool isActuallyDark = ((App)System.Windows.Application.Current).Resources["MainTextBrush"].ToString().Contains("#FFFFFFFF");
            WindowHelper.EnableBlur(this, isActuallyDark);
        }

        private void BindTimeEvents()
        {
            DarkModeTimeTextBox.TextChanged += TimeTextBox_TextChanged;
            LightModeTimeTextBox.TextChanged += TimeTextBox_TextChanged;
        }

        private void TimeTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox textBox)
            {
                textBox.TextChanged -= TimeTextBox_TextChanged;

                string raw = new string(System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Where(textBox.Text, c => char.IsDigit(c))));
                if (raw.Length > 4) raw = raw.Substring(0, 4);

                string formatted = raw;
                if (raw.Length >= 3)
                {
                    formatted = raw.Insert(raw.Length - 2, ":");
                }

                if (textBox.Text != formatted)
                {
                    int selectionStart = textBox.SelectionStart;
                    int oldLength = textBox.Text.Length;
                    textBox.Text = formatted;
                    
                    int newPos = selectionStart;
                    if (oldLength < formatted.Length) newPos++;
                    else if (oldLength > formatted.Length) newPos--;
                    
                    textBox.SelectionStart = Math.Max(0, Math.Min(formatted.Length, newPos));
                }

                textBox.TextChanged += TimeTextBox_TextChanged;
            }
        }

        private void LoadSettings()
        {
            AutoSwitchCheckBox.IsChecked = _config.AutoSwitchEnabled;
            DarkModeTimeTextBox.Text = _config.DarkModeTime;
            LightModeTimeTextBox.Text = _config.LightModeTime;
            StartWithWindowsCheckBox.IsChecked = _config.StartWithWindows;
            MinimizeToTrayCheckBox.IsChecked = _config.MinimizeToTray;
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (_config.MinimizeToTray)
                this.Hide();
            else
                this.Close();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!TimeSpan.TryParse(DarkModeTimeTextBox.Text, out _))
                {
                    ModernMessageBox.Show(this, "深色模式时间格式无效，请使用 HH:mm 格式（例如：18:00）");
                    return;
                }

                if (!TimeSpan.TryParse(LightModeTimeTextBox.Text, out _))
                {
                    ModernMessageBox.Show(this, "浅色模式时间格式无效，请使用 HH:mm 格式（例如：07:00）");
                    return;
                }

                _config.AutoSwitchEnabled = AutoSwitchCheckBox.IsChecked ?? true;
                _config.DarkModeTime = DarkModeTimeTextBox.Text;
                _config.LightModeTime = LightModeTimeTextBox.Text;
                _config.StartWithWindows = StartWithWindowsCheckBox.IsChecked ?? true;
                _config.MinimizeToTray = MinimizeToTrayCheckBox.IsChecked ?? true;

                if (!_configManager.SaveConfig(_config))
                {
                    ModernMessageBox.Show(this, "保存配置失败");
                    return;
                }

                if (_config.StartWithWindows && !_startupManager.IsStartupEnabled())
                {
                    _startupManager.EnableStartup();
                }
                else if (!_config.StartWithWindows && _startupManager.IsStartupEnabled())
                {
                    _startupManager.DisableStartup();
                }

                _scheduleManager.ReloadConfig();

                ModernMessageBox.Show(this, "设置已成功保存 ✨");
                Close();
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(this, $"保存设置时出错: {ex.Message}");
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_config.MinimizeToTray)
            {
                e.Cancel = true;
                Hide();
            }
            base.OnClosing(e);
        }
    }
}
