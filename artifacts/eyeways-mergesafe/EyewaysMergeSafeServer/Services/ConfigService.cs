using System.Text.Json;

namespace EyewaysMergeSafeServer.Services;

public class ConfigService
{
    private readonly IConfigurationRoot? _cfgRoot;
    private readonly string _keyFilePath;

    public ConfigService(IConfiguration cfg)
    {
        _cfgRoot    = cfg as IConfigurationRoot;
        _keyFilePath = Path.Combine(AppContext.BaseDirectory, "tomtomkey.json");
    }

    public string? GetTomTomKey() => _cfgRoot?["TomTomApiKey"];

    public void SaveTomTomKey(string apiKey)
    {
        var doc = new Dictionary<string, string> { { "TomTomApiKey", apiKey } };
        File.WriteAllText(_keyFilePath, JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));
        _cfgRoot?.Reload();
    }

    public void ClearTomTomKey()
    {
        if (File.Exists(_keyFilePath)) File.Delete(_keyFilePath);
        _cfgRoot?.Reload();
    }
}
