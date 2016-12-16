using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebLearnEntities;

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
                        LessonSettings = new List<LessonSetting>()
                    };
            ConfigManager.Save();
        }

        public static async Task<Status> Fetch(bool previous)
        {
            var facade = new WebLearnOld.Facade();
            await facade.Login(CredentialManager.TryGetCredential());

            throw new NotImplementedException();

            if (previous)
            {
                var terms = await facade.FetchAllLessonList();
                foreach (var term in terms)
                {
                    Console.Out.WriteLine($"Term {term}:");

                    foreach (var lesson in term.Lessons)
                        Console.Out.WriteLine($"{lesson.Name}");

                    Console.Out.WriteLine();
                }
            }
            else
            {
                var term = await facade.FetchCurrentLessonList();
                foreach (var lesson in term.Lessons)
                    Console.Out.WriteLine($"{lesson.Name}");

                await facade.FetchLesson(term.Lessons[3]);
                foreach (var announcement in term.Lessons[3].Announcements)
                    Console.Out.WriteLine(announcement.Title);
            }
        }
    }
}
