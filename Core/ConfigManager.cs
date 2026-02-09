using System;
using System.IO;
using System.Text.Json;
using WinAutoDarkMode.Models;

namespace WinAutoDarkMode.Core
{
    /// <summary>
    /// 配置文件管理器
    /// </summary>
    public class ConfigManager
    {
        private readonly string _configPath;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ConfigManager()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "WinAutoDarkMode");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            _configPath = Path.Combine(appFolder, "config.json");
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        public AppConfig LoadConfig()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    var defaultConfig = new AppConfig();
                    SaveConfig(defaultConfig);
                    return defaultConfig;
                }

                string json = File.ReadAllText(_configPath);
                var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
                
                if (config == null || !config.Validate())
                {
                    Console.WriteLine("配置文件无效,使用默认配置");
                    return new AppConfig();
                }

                return config;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载配置失败: {ex.Message}");
                return new AppConfig();
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public bool SaveConfig(AppConfig config)
        {
            try
            {
                if (!config.Validate())
                {
                    Console.WriteLine("配置验证失败");
                    return false;
                }

                string json = JsonSerializer.Serialize(config, JsonOptions);
                File.WriteAllText(_configPath, json);
                Console.WriteLine($"配置已保存到: {_configPath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存配置失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取配置文件路径
        /// </summary>
        public string GetConfigPath() => _configPath;
    }
}
