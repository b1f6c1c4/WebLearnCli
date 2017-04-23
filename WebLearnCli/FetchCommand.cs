using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebLearnCli.Crawler;

namespace WebLearnCli
{
    internal class FetchCommand : LessonCommandBase
    {
        public FetchCommand() : base("fetch", "update information") { HasAdditionalArguments(null, "lessons"); }

        private static async Task<CrawlerFacade> FetchList(bool previous)
        {
            var facade = await Facade.Login();

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

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            async Task<int> ConcreteRunAsync()
            {
                var facade = await FetchList(Previous);
                await Task.WhenAll(lessons.Select(l => facade.FetchLesson(l).Then(e => e.Save())));

                Facade.GenerateStatus();
                return 0;
            }

            return ConcreteRunAsync().Result;
        }
    }
}
