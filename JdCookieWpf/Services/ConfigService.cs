using System.IO;
using System.Text.Json;
using JdCookieWpf.Models;

namespace JdCookieWpf.Services;

public static class ConfigService
{
    private static readonly string ConfigPath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(JdCookieWpf),
            "config.json");

    // 加载配置文件
    public static AppConfig? LoadConfig()
    {
        if (!File.Exists(ConfigPath))
        {
            return new AppConfig
            {
                Url = "",
                Cid = "",
                Secret = ""
            };
        }

        var json = File.ReadAllText(ConfigPath);
        return JsonSerializer.Deserialize<AppConfig>(json);
    }

    // 保存配置文件
    public static async Task SaveConfig(AppConfig config)
    {
        var dir = Path.GetDirectoryName(ConfigPath);
        if (!Directory.Exists(dir))
        {
            if (dir != null) Directory.CreateDirectory(dir);
        }

        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(config, options);
        await File.WriteAllTextAsync(ConfigPath, json);
    }
}