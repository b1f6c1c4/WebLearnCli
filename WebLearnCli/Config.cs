using System.Collections.Generic;
using System.Linq;

namespace WebLearnCli
{
    public sealed class Config
    {
        private static Config m_Inst;

        public static Config Inst
        {
            get
            {
                if (m_Inst == null)
                    Load();
                return m_Inst;
            }
            set => m_Inst = value;
        }

        public static void Load() => Inst = "config.json".InDb().LoadJson<Config>();

        public static void Save() => "config.json".InDb().SaveJson(Inst);

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
                        Path = lesson.Name.Replace(':', '-'),
                        Alias = new List<string>()
                    };
            Lessons.Add(s);
        }
    }
}
