using Microsoft.Win32;
using System;
using System.Reflection;

namespace WinAutoDarkMode.Core
{
    /// <summary>
    /// 开机自启动管理器
    /// </summary>
    public class StartupManager
    {
        private const string AppName = "WinAutoDarkMode";
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// 检查是否已设置开机自启动
        /// </summary>
        public bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false);
                return key?.GetValue(AppName) != null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查启动项失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 启用开机自启动
        /// </summary>
        public bool EnableStartup()
        {
            try
            {
                string exePath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    Console.WriteLine("无法打开注册表启动项");
                    return false;
                }

                key.SetValue(AppName, $"\"{exePath}\"");
                Console.WriteLine("开机自启动已启用");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启用开机自启动失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 禁用开机自启动
        /// </summary>
        public bool DisableStartup()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true);
                if (key == null)
                {
                    Console.WriteLine("无法打开注册表启动项");
                    return false;
                }

                if (key.GetValue(AppName) != null)
                {
                    key.DeleteValue(AppName);
                    Console.WriteLine("开机自启动已禁用");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"禁用开机自启动失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 切换开机自启动状态
        /// </summary>
        public bool ToggleStartup()
        {
            return IsStartupEnabled() ? DisableStartup() : EnableStartup();
        }
    }
}
