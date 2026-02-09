using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WinAutoDarkMode.Core;

namespace WinAutoDarkMode.UI
{
    /// <summary>
    /// 系统托盘图标管理器
    /// </summary>
    public class TrayIcon : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ThemeManager _themeManager;
        private readonly ScheduleManager _scheduleManager;
        private readonly StartupManager _startupManager;
        private readonly ContextMenuStrip _contextMenu;

        private class DarkThemeRenderer : ToolStripProfessionalRenderer
        {
            private bool _isDark;
            public DarkThemeRenderer(bool isDark) : base(new DarkThemeColorTable(isDark)) { _isDark = isDark; }

            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                e.TextColor = _isDark ? Color.White : Color.Black;
                var rect = e.TextRectangle;
                rect.Offset(10, 0);
                e.TextRectangle = rect;
                base.OnRenderItemText(e);
            }

            protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
            {
                if (e.Item.Selected)
                {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    var rect = new Rectangle(4, 2, e.Item.Width - 8, e.Item.Height - 4);
                    using var brush = new SolidBrush(_isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(230, 230, 230));
                    FillRoundedRectangle(g, brush, rect, 4);
                }
            }

            private void FillRoundedRectangle(Graphics g, Brush brush, Rectangle rect, int radius)
            {
                using var path = new System.Drawing.Drawing2D.GraphicsPath();
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                g.FillPath(brush, path);
            }

            protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
            {
                var g = e.Graphics;
                var color = _isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(225, 225, 225);
                using var pen = new Pen(color);
                g.DrawLine(pen, 10, e.Item.Height / 2, e.Vertical ? 10 : e.Item.Width - 10, e.Item.Height / 2);
            }

            protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
            {
            }
        }

        private class DarkThemeColorTable : ProfessionalColorTable
        {
            private bool _isDark;
            public DarkThemeColorTable(bool isDark) { _isDark = isDark; }
            public override Color ToolStripDropDownBackground => _isDark ? Color.FromArgb(32, 32, 32) : Color.White;
            public override Color MenuBorder => _isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(200, 200, 200);
            public override Color MenuItemSelected => Color.Transparent;
            public override Color MenuItemBorder => Color.Transparent;
            public override Color SeparatorDark => _isDark ? Color.FromArgb(60, 60, 60) : Color.FromArgb(225, 225, 225);
        }

        public event EventHandler? SettingsRequested;
        public event EventHandler? ExitRequested;

        public TrayIcon(ThemeManager themeManager, ScheduleManager scheduleManager, StartupManager startupManager)
        {
            _themeManager = themeManager;
            _scheduleManager = scheduleManager;
            _startupManager = startupManager;

            _contextMenu = CreateContextMenu();
            
            _notifyIcon = new NotifyIcon
            {
                Text = "WinAutoDarkMode - 自动深色模式",
                Visible = true,
                ContextMenuStrip = _contextMenu
            };

            RefreshIcon(); // 初始设置图标

            _notifyIcon.DoubleClick += OnTrayIconDoubleClick;
            _scheduleManager.ThemeChanged += (s, theme) => RefreshIcon();
        }

        public void RefreshIcon()
        {
            try
            {
                bool isDark = !_themeManager.IsLightMode();
                string iconName = isDark ? "moon.png" : "sun.png";
                var uri = new Uri($"pack://application:,,,/{iconName}");
                var resourceInfo = System.Windows.Application.GetResourceStream(uri);

                if (resourceInfo != null)
                {
                    using (var stream = resourceInfo.Stream)
                    {
                        using var bitmap = new Bitmap(stream);
                        _notifyIcon.Icon = Icon.FromHandle(bitmap.GetHicon());
                    }
                }
            }
            catch (Exception ex)
            {
                _notifyIcon.Icon = SystemIcons.Application;
                Console.WriteLine($"更新图标失败: {ex.Message}");
            }
        }

        private ContextMenuStrip CreateContextMenu()
        {
            var menu = new ContextMenuStrip
            {
                Padding = new Padding(4),
                Font = new Font("Segoe UI", 9.5f),
                ShowImageMargin = false,
                ShowCheckMargin = false
            };

            var toggleItem = new ToolStripMenuItem("切换主题", null, OnToggleTheme);
            menu.Items.Add(toggleItem);

            menu.Items.Add(new ToolStripSeparator());

            var statusItem = new ToolStripMenuItem
            {
                Text = GetCurrentStatusText(),
                Enabled = false
            };
            menu.Items.Add(statusItem);

            menu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("设置", null, OnSettings);
            menu.Items.Add(settingsItem);

            var startupItem = new ToolStripMenuItem("开机自启动", null, OnToggleStartup)
            {
                Checked = _startupManager.IsStartupEnabled()
            };
            menu.Items.Add(startupItem);

            menu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("退出", null, OnExit);
            menu.Items.Add(exitItem);

            menu.Opening += (s, e) =>
            {
                statusItem.Text = GetCurrentStatusText();
                startupItem.Checked = _startupManager.IsStartupEnabled();
                bool isLight = _themeManager.IsLightMode();
                _contextMenu.Renderer = new DarkThemeRenderer(!isLight);
            };

            return menu;
        }

        private string GetCurrentStatusText()
        {
            string mode = _themeManager.IsLightMode() ? "浅色" : "深色";
            return $"当前模式: {mode}";
        }

        private void OnToggleTheme(object? sender, EventArgs e)
        {
            _themeManager.ToggleTheme();
            RefreshIcon();
            ShowNotification("主题已切换", GetCurrentStatusText());
        }

        private void OnSettings(object? sender, EventArgs e)
        {
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnToggleStartup(object? sender, EventArgs e)
        {
            _startupManager.ToggleStartup();
            string status = _startupManager.IsStartupEnabled() ? "已启用" : "已禁用";
            ShowNotification("开机自启动", status);
        }

        private void OnExit(object? sender, EventArgs e)
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OnTrayIconDoubleClick(object? sender, EventArgs e)
        {
            SettingsRequested?.Invoke(this, EventArgs.Empty);
        }

        public void ShowNotification(string title, string message)
        {
            _notifyIcon.ShowBalloonTip(3000, title, message, ToolTipIcon.Info);
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}
