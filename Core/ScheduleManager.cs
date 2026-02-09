using System;
using System.Timers;
using Microsoft.Win32;
using WinAutoDarkMode.Models;
using Timer = System.Timers.Timer;

namespace WinAutoDarkMode.Core
{
    /// <summary>
    /// 定时调度管理器 - 采用精确时间差值算法
    /// </summary>
    public class ScheduleManager : IDisposable
    {
        private readonly ThemeManager _themeManager;
        private readonly ConfigManager _configManager;
        private Timer? _timer;
        private AppConfig _config;

        public event EventHandler<string>? ThemeChanged;

        public ScheduleManager(ThemeManager themeManager, ConfigManager configManager)
        {
            _themeManager = themeManager;
            _configManager = configManager;
            _config = _configManager.LoadConfig();
            
            // 监听系统时间改变(例如用户调整了时钟), 自动重新对齐计划
            try {
                SystemEvents.TimeChanged += OnSystemTimeChanged;
            } catch { }
        }

        private void OnSystemTimeChanged(object? sender, EventArgs e)
        {
            ReloadPlan();
        }

        public void Start()
        {
            ReloadPlan();
        }

        public void Stop()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
        }

        public void ReloadConfig()
        {
            _config = _configManager.LoadConfig();
            ReloadPlan();
        }

        /// <summary>
        /// 核心算法：重新加载计划，计算下一次切换的精准延迟
        /// </summary>
        private void ReloadPlan()
        {
            if (!_config.AutoSwitchEnabled)
            {
                Stop();
                return;
            }

            // 1. 立即校准一次当前模式
            CheckAndSwitchTheme();

            // 2. 计算下一次切换需要的等待时间
            double delayMs = CalculateMillisecondsToNextEvent();

            if (_timer == null)
            {
                _timer = new Timer();
                _timer.Elapsed += OnTimerElapsed;
                _timer.AutoReset = false; // 每次执行完重新计算，不循环
            }
            
            _timer.Stop();
            _timer.Interval = delayMs;
            _timer.Start();

            Console.WriteLine($"[调度器] 下一次计划切换将在 {delayMs / 1000 / 60:F2} 分钟后执行");
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            ReloadPlan(); // 执行切换并安排下一次
        }

        private double CalculateMillisecondsToNextEvent()
        {
            var now = DateTime.Now;
            TimeSpan darkTime, lightTime;
            
            if (!TimeSpan.TryParse(_config.DarkModeTime, out darkTime)) darkTime = new TimeSpan(18, 0, 0);
            if (!TimeSpan.TryParse(_config.LightModeTime, out lightTime)) lightTime = new TimeSpan(7, 0, 0);

            DateTime nextDark = now.Date.Add(darkTime);
            DateTime nextLight = now.Date.Add(lightTime);

            // 如果今天的时间已经过了，设为明天
            if (nextDark <= now) nextDark = nextDark.AddDays(1);
            if (nextLight <= now) nextLight = nextLight.AddDays(1);

            // 取两个时间点中更近的一个
            DateTime nextEvent = nextDark < nextLight ? nextDark : nextLight;
            
            // 增加 2 秒缓冲时间，确保系统时间已经完全跨过临界点
            TimeSpan diff = nextEvent - now;
            double ms = diff.TotalMilliseconds + 2000; 

            // 防止负值或过小的值
            return ms > 0 ? ms : 1000;
        }

        private void CheckAndSwitchTheme()
        {
            try
            {
                var now = DateTime.Now.TimeOfDay;
                TimeSpan darkTime, lightTime;
                
                if (!TimeSpan.TryParse(_config.DarkModeTime, out darkTime)) return;
                if (!TimeSpan.TryParse(_config.LightModeTime, out lightTime)) return;

                bool shouldBeDark = ShouldUseDarkMode(now, darkTime, lightTime);
                bool isCurrentlyLight = _themeManager.IsLightMode();

                if (shouldBeDark && isCurrentlyLight)
                {
                    if (_themeManager.SwitchToDarkMode())
                        ThemeChanged?.Invoke(this, "深色模式");
                }
                else if (!shouldBeDark && !isCurrentlyLight)
                {
                    if (_themeManager.SwitchToLightMode())
                        ThemeChanged?.Invoke(this, "浅色模式");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[调度器] 自动校准失败: {ex.Message}");
            }
        }

        private bool ShouldUseDarkMode(TimeSpan now, TimeSpan darkTime, TimeSpan lightTime)
        {
            if (darkTime > lightTime)
                return now >= darkTime || now < lightTime;
            else
                return now >= darkTime && now < lightTime;
        }

        public void Dispose()
        {
            try {
                SystemEvents.TimeChanged -= OnSystemTimeChanged;
            } catch { }
            Stop();
        }
    }
}
