using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

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

        private static async Task<CrawlerOld> FetchList(bool previous)
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
                    ConfigManager.Config.Update(term.Info, lesson);

            ConfigManager.Save();

            return facade;
        }

        public static async Task Fetch(bool previous)
        {
            var facade = await FetchList(previous);

            await Task.WhenAll(ConfigManager.Config.Lessons.Select(l => facade.FetchLesson(l).ContinueWith(t => SaveExtension(l, t.Result))));

            GenerateStatus();
        }

        public static async Task Fetch(bool previous, IEnumerable<string> args)
        {
            var facade = await FetchList(previous);

            var lessons = new List<Lesson>();
            foreach (var arg in args)
            {
                var l = AbbrExpand.GetLesson(arg);
                if (l == null)
                    throw new ApplicationException($"Lesson \"{arg}\" not found.");
                lessons.Add(l);
            }

            await Task.WhenAll(lessons.Select(l => facade.FetchLesson(l).ContinueWith(t => SaveExtension(l, t.Result))));

            GenerateStatus();
        }

        private static void SaveExtension(Lesson lesson, LessonExtension ext)
        {
            Directory.CreateDirectory(DbHelper.GetPath($"lessons/{lesson}/"));
            File.WriteAllText(
                              DbHelper.GetPath($"lessons/{lesson}/announcements.json"),
                              JsonConvert.SerializeObject(ext.Announcements, Formatting.Indented));
            File.WriteAllText(
                              DbHelper.GetPath($"lessons /{lesson}/documents.json"),
                              JsonConvert.SerializeObject(ext.Documents, Formatting.Indented));
            File.WriteAllText(
                              DbHelper.GetPath($"lessons/{lesson}/assignments.json"),
                              JsonConvert.SerializeObject(ext.Assignments, Formatting.Indented));
        }

        private static void GenerateStatus() { throw new NotImplementedException(); }
    }
}
