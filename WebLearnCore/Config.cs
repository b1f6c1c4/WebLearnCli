using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WebLearnCore
{
    public sealed class Config
    {
        public static Config Inst { get; set; }

        public static void Load() =>
            Inst = JsonConvert.DeserializeObject<Config>(File.ReadAllText(DbHelper.GetPath("config.json")));

        public static void Save() =>
            File.WriteAllText(DbHelper.GetPath("config.json"), JsonConvert.SerializeObject(Inst, Formatting.Indented));

        public List<Lesson> Lessons { get; set; }

        public void Update(TermInfo term, Lesson lesson)
        {
            var setting =
                Lessons.SingleOrDefault(
                                        ls =>
                                        ls.Term == term && ls.Name == lesson.Name && ls.Index == lesson.Index);
            if (setting != null)
            {
                setting.CourseId = lesson.CourseId;
                setting.BbsId = lesson.BbsId;
                setting.Version = lesson.Version;
                return;
            }

            var s =
                new Lesson
                    {
                        Term = term,
                        Name = lesson.Name,
                        Index = lesson.Index,
                        CourseId = lesson.CourseId,
                        BbsId = lesson.BbsId,
                        Version = lesson.Version,
                        Ignore = false,
                        Path = lesson.Name,
                        Alias = new List<string>()
                    };
            Lessons.Add(s);
        }
    }
}
