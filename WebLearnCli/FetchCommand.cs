using System;
using System.Linq;
using System.Security.Authentication;
using ManyConsole;
using WebLearnEntities;
using WebLearnOld;

namespace WebLearnCli
{
    internal class FetchCommand : ConsoleCommand
    {
        public FetchCommand() { IsCommand("fetch", "update information"); }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var facade = new Facade();
                facade.Login(CredentialManager.TryGetCredential());
                var termT = facade.FetchCurrentLessonList();
                termT.Wait();
                var term = termT.Result;
                foreach (var lesson in term.Lessons.Cast<WebLearnOld.Lesson>())
                {
                    Console.Out.WriteLine($"{lesson.Name} {lesson.BbsId} {lesson.CourseId}");
                }
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
    }
}
