using System;
using System.Security.Authentication;
using ManyConsole;
using WebLearnCore;

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
                var statusT = Facade.Fetch(m_Previous);
                statusT.Wait();
                var status = statusT.Result;
                foreach (var lesson in status.Lessons)
                {
                    if (!lesson.HasNewAnnouncement &&
                        !lesson.HasNewDocument &&
                        !lesson.HasDeadLine)
                        continue;

                    Console.Out.WriteLine($"{(lesson.HasNewAnnouncement ? "A" : " ")} {(lesson.HasNewDocument ? "F" : " ")} {(lesson.HasDeadLine ? "D" : " ")} {lesson.Name}");
                }

                foreach (var deadLine in status.DeadLines)
                    Console.Out.WriteLine(Formatter.Format(deadLine));
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
