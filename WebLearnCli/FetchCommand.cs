using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using ManyConsole;
using WebLearnEntities;
using WebLearnOld;

namespace WebLearnCli
{
    internal class FetchCommand : ConsoleCommand
    {
        private bool m_Previous;

        public FetchCommand()
        {
            IsCommand("fetch", "update information");
            HasOption("previous", "fetch old lessons.", t => m_Previous = t != null);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Run().Wait();
                return 0;
            }
            catch (AuthenticationException)
            {
                Console.Error.WriteLine("Invalid credential.");
                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unknown error: {e.Message}");
                return 1;
            }
        }

        private async Task Run()
        {
            var facade = new Facade();
            await facade.Login(CredentialManager.TryGetCredential());
            if (m_Previous)
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

                await facade.FetchLesson(term.Lessons[0]);
                foreach (var announcement in term.Lessons[0].Announcements)
                {
                    Console.Out.WriteLine(announcement.Title);
                }
            }
        }
    }
}
