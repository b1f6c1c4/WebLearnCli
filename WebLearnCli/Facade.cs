using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebLearnCli.Crawler;

namespace WebLearnCli
{
    internal static class Facade
    {
        public static async Task<CrawlerFacade> Login(bool roaming = false)
        {
            var facade = new CrawlerFacade();
            await facade.Login(CredentialManager.TryGetCredential(), roaming);
            return facade;
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
