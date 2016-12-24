using System;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal class StatusCommand : ConsoleCommand
    {
        public StatusCommand() { IsCommand("status", "view lessons and deadlines"); }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Status.Load();

                foreach (var lesson in Status.Inst.Lessons)
                {
                    if (!lesson.HasNewAnnouncement &&
                        !lesson.HasNewDocument &&
                        !lesson.HasDeadLine)
                        continue;

                    Console.Out.WriteLine(
                                          $"{(lesson.HasNewAnnouncement ? "A" : " ")} {(lesson.HasNewDocument ? "F" : " ")} {(lesson.HasDeadLine ? "D" : " ")} {lesson.Name}");
                }

                foreach (var deadLine in Status.Inst.DeadLines)
                    Console.Out.WriteLine(Formatter.Format(deadLine));
                return 0;
            }
            catch (ApplicationException e)
            {
                Console.Error.WriteLine(e.Message);
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
