﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebLearnCore.Crawler;

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
            await Task.WhenAll(lessons.Select(l => facade.FetchLesson(l).ContinueWith(t => SaveExtension(l, t.Result))));

            GenerateStatus();
        }

        public static async Task Checkout(IEnumerable<Lesson> lessons)
        {
            var facade = await Login(true);
            await Task.WhenAll(lessons.Select(l => facade.CheckoutLesson(l, LoadExtension(l))));

            GenerateStatus();
        }

        public static void SaveExtension(Lesson lesson, LessonExtension ext)
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

        public static LessonExtension LoadExtension(Lesson lesson) =>
            new LessonExtension
                {
                    Announcements =
                        JsonConvert
                        .DeserializeObject<List<Announcement>>
                        (File.ReadAllText(DbHelper.GetPath($"lessons/{lesson}/announcements.json"))),
                    Documents =
                        JsonConvert
                        .DeserializeObject<List<Document>>
                        (File.ReadAllText(DbHelper.GetPath($"lessons/{lesson}/documents.json"))),
                    Assignments =
                        JsonConvert
                        .DeserializeObject<List<Assignment>>
                        (File.ReadAllText(DbHelper.GetPath($"lessons/{lesson}/assignments.json")))
                };

        public static void GenerateStatus()
        {
            var ddls = new List<DeadLine>();
            var lsts = new List<LessonStatus>();

            foreach (var lesson in Config.Inst.Lessons)
            {
                if (lesson.Ignore)
                    continue;

                var ext = LoadExtension(lesson);
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
