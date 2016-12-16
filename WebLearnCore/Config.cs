using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WebLearnCore
{
    internal sealed class Config
    {
        public List<Lesson> Lessons { get; set; }

        public Lesson GetLesson(TermInfo term, Lesson lesson)
        {
            var setting =
                Lessons.SingleOrDefault(
                                        ls =>
                                        ls.Term == term && ls.Name == lesson.Name && ls.Index == lesson.Index);
            if (setting != null)
                return setting;

            var s = new Lesson
                        {
                            Term = term,
                            Name = lesson.Name,
                            Index = lesson.Index,
                            Ignore = false,
                            Path = lesson.Name,
                            Alias = new List<string>()
                        };
            Lessons.Add(s);
            return s;
        }
    }

    internal static class ConfigManager
    {
        public static Config Config { get; set; }

        public static void Load() =>
            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(DbHelper.GetPath("config.json")));

        public static void Save() =>
            File.WriteAllText(DbHelper.GetPath("config.json"), JsonConvert.SerializeObject(Config, Formatting.Indented));
    }
}
