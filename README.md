# WinAutoDarkMode

Windows 自动深色模式切换工具 - 根据时间自动切换系统深色/浅色模式。

## 功能特性

- ⏰ **定时自动切换** - 根据设定的时间自动切换深色/浅色模式
- 🖱️ **系统托盘** - 最小化到托盘,不占用任务栏
- ⚡ **快捷切换** - 右键托盘图标可手动切换主题
- 🚀 **开机自启** - 支持开机自动运行
- ⚙️ **可视化配置** - 友好的设置界面

## 系统要求

- Windows 10 / 11
- .NET 8.0 Runtime

## 安装

### 从源码构建

```bash
# 克隆仓库
git clone https://github.com/yourusername/WinAutoDarkMode.git
cd WinAutoDarkMode

# 构建项目
dotnet build -c Release

# 运行
dotnet run
```

### 发布独立可执行文件

```bash
dotnet publish -c Release -r win-x64 --self-contained true
```

## 使用说明

1. 运行程序后,图标会显示在系统托盘
2. **双击托盘图标** - 打开设置窗口
3. **右键托盘图标** - 显示快捷菜单
   - 切换主题 - 立即切换深色/浅色模式
   - 设置 - 打开配置界面
   - 开机自启动 - 开关自启动功能
   - 退出 - 关闭程序

## 配置说明

配置文件位置: `%APPDATA%\WinAutoDarkMode\config.json`

```json
{
  "autoSwitchEnabled": true,
  "darkModeTime": "18:00",
  "lightModeTime": "07:00",
  "startWithWindows": true,
  "minimizeToTray": true,
  "checkIntervalMinutes": 1
}
```

| 配置项 | 说明 | 默认值 |
|--------|------|--------|
| autoSwitchEnabled | 启用自动切换 | true |
| darkModeTime | 深色模式开始时间 | 18:00 |
| lightModeTime | 浅色模式开始时间 | 07:00 |
| startWithWindows | 开机自启动 | true |
| minimizeToTray | 最小化到托盘 | true |
| checkIntervalMinutes | 检查间隔(分钟) | 1 |

## 工作原理

程序通过修改 Windows 注册表来切换主题:

```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize
- AppsUseLightTheme: 0=深色, 1=浅色
- SystemUsesLightTheme: 0=深色, 1=浅色
```

## 开发

```bash
# 还原依赖
dotnet restore

# 开发模式运行
dotnet run

# 运行测试
dotnet test
```

## License

MIT License
