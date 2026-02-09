using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace WinAutoDarkMode.Core
{
    /// <summary>
    /// Windows 主题管理器
    /// </summary>
    public class ThemeManager
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string AppsUseLightTheme = "AppsUseLightTheme";
        private const string SystemUsesLightTheme = "SystemUsesLightTheme";

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd, 
            uint Msg, 
            IntPtr wParam, 
            string lParam, 
            uint fuFlags, 
            uint uTimeout, 
            out IntPtr lpdwResult);

        private const uint WM_SETTINGCHANGE = 0x001A;
        private const uint SMTO_ABORTIFHUNG = 0x0002;
        private static readonly IntPtr HWND_BROADCAST = new IntPtr(0xffff);

        /// <summary>
        /// 获取当前主题模式
        /// </summary>
        /// <returns>true=浅色模式, false=深色模式</returns>
        public bool IsLightMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
                if (key == null) return true; // 默认浅色

                var value = key.GetValue(AppsUseLightTheme);
                return value != null && (int)value == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取主题状态失败: {ex.Message}");
                return true;
            }
        }

        public bool SwitchToDarkMode()
        {
            return SetTheme(isDark: true);
        }

        public bool SwitchToLightMode()
        {
            return SetTheme(isDark: false);
        }

        private bool SetTheme(bool isDark)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
                if (key == null)
                {
                    Console.WriteLine("无法打开注册表项");
                    return false;
                }

                int value = isDark ? 0 : 1;
                key.SetValue(AppsUseLightTheme, value, RegistryValueKind.DWord);
                key.SetValue(SystemUsesLightTheme, value, RegistryValueKind.DWord);

                // 发送系统广播消息,强制刷新 UI
                BroadcastThemeChange();

                Console.WriteLine($"主题已切换到: {(isDark ? "深色" : "浅色")}模式");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"切换主题失败: {ex.Message}");
                return false;
            }
        }

        private void BroadcastThemeChange()
        {
            // 向所有窗口广播设置已更改的消息
            // "ImmersiveColorSet" 是 Windows 用于主题颜色通知的关键标识
            SendMessageTimeout(
                HWND_BROADCAST, 
                WM_SETTINGCHANGE, 
                IntPtr.Zero, 
                "ImmersiveColorSet", 
                SMTO_ABORTIFHUNG, 
                500, 
                out _);
        }

        public bool ToggleTheme()
        {
            bool isCurrentlyLight = IsLightMode();
            return SetTheme(isDark: isCurrentlyLight);
        }
    }
}
