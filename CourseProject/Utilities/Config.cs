using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace CourseProject
{
    public static class Config
    {
        private static string cfgPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        private static Dictionary<string, string> settings = null;

        private static void EnsureLoaded()
        {
            if (settings != null) return;
            if (File.Exists(cfgPath))
            {
                try { settings = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(cfgPath)); }
                catch { settings = new Dictionary<string, string>(); }
            }
            else settings = new Dictionary<string, string>();
        }

        public static string LoadSetting(string key) { EnsureLoaded(); return settings.TryGetValue(key, out var v) ? v : null; }

        public static void SaveSetting(string key, string value)
        {
            EnsureLoaded();
            settings[key] = value;
            File.WriteAllText(cfgPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}