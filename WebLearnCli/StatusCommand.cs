using System;
using System.Linq;
using ManyConsole;

namespace WebLearnCli
{
    internal class StatusCommand : ConsoleCommand
    {
        private bool m_Force;

        public StatusCommand()
        {
            IsCommand("status", "view lessons and deadlines");
            HasOption("f|force", "update status info", t => m_Force = t != null);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (m_Force)
                    Facade.GenerateStatus();

                foreach (var lesson in Status.Inst.Lessons)
                {
                    if (!lesson.HasNewAnnouncement &&
                        !lesson.HasNewDocument &&
                        !lesson.HasDeadLine)
                        continue;

                    Console.Out.WriteLine(
                                          $"{(lesson.HasNewAnnouncement ? "A" : " ")} {(lesson.HasNewDocument ? "F" : " ")} {(lesson.HasDeadLine ? "D" : " ")} {lesson.Name}");
                }

                foreach (var deadLine in Status.Inst.DeadLines.OrderBy(d => d.DueDate))
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
