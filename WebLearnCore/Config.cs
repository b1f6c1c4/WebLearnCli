using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WebLearnCore
{
    internal sealed class Config
    {
        public List<LessonSetting> LessonSettings { get; set; }
    }

    internal sealed class LessonSetting
    {
        public string Term { get; set; }
        public string Name { get; set; }
        public string Index { get; set; }

        public bool Ignore { get; set; }
        public string Path { get; set; }
        public List<string> Alias { get; set; }
    }

    internal static class ConfigManager
    {
        public static Config Config { get; set; }

        public static void Load() =>
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(DbHelper.GetPath("config")));

        public static void Save() =>
            File.WriteAllText(DbHelper.GetPath("config"), JsonConvert.SerializeObject(Config, Formatting.Indented));
    }
}
