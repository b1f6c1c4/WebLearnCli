using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WebLearnCore
{
    public static class Facade
    {
        public static void Init()
        {
            if (Directory.Exists(DbHelper.GetPath("")))
                throw new ApplicationException("WebLearn folder already exists.");

            Directory.CreateDirectory(DbHelper.GetPath(""));
            Directory.CreateDirectory(DbHelper.GetPath("lessons/"));
            ConfigManager.Config =
                new Config
                    {
                        Lessons = new List<Lesson>()
                    };
            ConfigManager.Save();
        }

        public static async Task Fetch(bool previous, IEnumerable<string> args)
        {
            var facade = new CrawlerOld();
            await facade.Login(CredentialManager.TryGetCredential());

            List<Term> terms;
            if (previous)
                terms = await facade.FetchAllLessonList();
            else
                terms = new List<Term> { await facade.FetchCurrentLessonList() };

            ConfigManager.Load();

            foreach (var term in terms)
                foreach (var lesson in term.Lessons)
                {
                    var settings = ConfigManager.Config.GetLesson(term.Info, lesson);
                    if (settings.Ignore)
                        continue;
                }

            ConfigManager.Save();

            var lessons = new List<Lesson>();
            foreach (var arg in args)
            {
                var l = AbbrExpand.GetLesson(arg);
                if (l == null)
                    throw new ApplicationException($"Lesson \"{arg}\" not found.");
                lessons.Add(l);
            }

            throw new NotImplementedException();
        }
    }
}
