using System.IO;
using System.Text.Json;
using StudentAdWindowsApp.Models;

namespace StudentAdWindowsApp.Services
{
    public class ConfigService
    {
        private readonly string _path =
            Path.Combine(AppContext.BaseDirectory, "config.json");

        public void Save(AdConfig config)
        {
            var json = JsonSerializer.Serialize(
                config,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(_path, json);
        }

        public AdConfig Load()
        {
            if (!File.Exists(_path))
                return new AdConfig();

            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<AdConfig>(json)
                   ?? new AdConfig();
        }
    }
}
