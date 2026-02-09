using System;

namespace WinAutoDarkMode.Models
{
    /// <summary>
    /// 应用程序配置模型
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// 是否启用自动切换
        /// </summary>
        public bool AutoSwitchEnabled { get; set; } = true;

        /// <summary>
        /// 切换到深色模式的时间 (24小时制, 例如: "18:00")
        /// </summary>
        public string DarkModeTime { get; set; } = "18:00";

        /// <summary>
        /// 切换到浅色模式的时间 (24小时制, 例如: "07:00")
        /// </summary>
        public string LightModeTime { get; set; } = "07:00";

        /// <summary>
        /// 是否开机自启动
        /// </summary>
        public bool StartWithWindows { get; set; } = true;

        /// <summary>
        /// 是否最小化到系统托盘
        /// </summary>
        public bool MinimizeToTray { get; set; } = true;

        /// <summary>
        /// 验证配置有效性
        /// </summary>
        public bool Validate()
        {
            try
            {
                TimeSpan.Parse(DarkModeTime);
                TimeSpan.Parse(LightModeTime);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
