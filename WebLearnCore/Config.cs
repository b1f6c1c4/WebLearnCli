using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using WebLearnEntities;

namespace WebLearnCore
{
    internal sealed class Config
    {
        public List<LessonSetting> LessonSettings { get; set; }

        public LessonSetting GetLessonSetting(TermInfo term, Lesson lesson)
        {
            var setting =
                LessonSettings.SingleOrDefault(
                                               ls =>
                                               ls.Term == term && ls.Name == lesson.Name && ls.Index == lesson.Index);
            if (setting != null)
                return setting;

            var s = new LessonSetting
                        {
                            Term = term,
                            Name = lesson.Name,
                            Index = lesson.Index,
                            Ignore = false,
                            Path = lesson.Name,
                            Alias = new List<string>()
                        };
            LessonSettings.Add(s);
            return s;
        }
    }

    internal sealed class LessonSetting
    {
        public TermInfo Term { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }

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
