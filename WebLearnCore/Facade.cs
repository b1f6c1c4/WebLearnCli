using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebLearnCore.Crawler;

namespace WebLearnCore
{
    public static class Facade
    {
        public static void Init()
        {
            if (Directory.Exists("".InDb()))
                throw new ApplicationException("WebLearn folder already exists.");

            Directory.CreateDirectory("".InDb());
            Directory.CreateDirectory("lessons/".InDb());
            Config.Inst =
                new Config
                    {
                        Lessons = new List<Lesson>()
                    };
            Config.Save();
        }

        private static async Task<CrawlerFacade> Login(bool roaming = false)
        {
            var facade = new CrawlerFacade();
            await facade.Login(CredentialManager.TryGetCredential(), roaming);
            return facade;
        }

        private static async Task<CrawlerFacade> FetchList(bool previous)
        {
            var facade = await Login();

            List<Term> terms;
            if (previous)
                terms = await facade.FetchAllLessonList();
            else
                terms = new List<Term> { await facade.FetchCurrentLessonList() };

            Config.Load();

            foreach (var term in terms)
                foreach (var lesson in term.Lessons)
                    Config.Inst.Update(term.Info, lesson);

            Config.Save();

            return facade;
        }

        public static async Task Fetch(bool previous, IEnumerable<Lesson> lessons)
        {
            var facade = await FetchList(previous);
            await Task.WhenAll(lessons.Select(l => facade.FetchLesson(l).Then(e => e.Save())));

            GenerateStatus();
        }

        public static async Task Checkout(IEnumerable<Lesson> lessons)
        {
            var facade = await Login(true);
            await Task.WhenAll(lessons.Select(l => facade.CheckoutLesson(l.Extension())));

            GenerateStatus();
        }

        public static void GenerateStatus()
        {
            var ddls = new List<DeadLine>();
            var lsts = new List<LessonStatus>();

            foreach (var lesson in Config.Inst.Lessons)
            {
                if (lesson.Ignore)
                    continue;

                var ext = lesson.Extension();
                var flag = false;
                foreach (var assignment in ext.Assignments)
                {
                    if (assignment.IsIgnored)
                        continue;
                    flag = true;
                    ddls.Add(
                             new DeadLine
                                 {
                                     DueDate = assignment.DueDate,
                                     Title = assignment.Title,
                                     Name = lesson.Name
                                 });
                }
                lsts.Add(
                         new LessonStatus
                             {
                                 Name = lesson.Name,
                                 HasNewAnnouncement = ext.Announcements.Any(o => !o.IsIgnored),
                                 HasNewDocument = ext.Documents.Any(o => !o.IsIgnored),
                                 HasDeadLine = flag
                             });
            }

            Status.Inst =
                new Status
                    {
                        Lessons = lsts,
                        DeadLines = ddls
                    };
            Status.Save();
        }
    }
}
