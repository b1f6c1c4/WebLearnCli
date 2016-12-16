﻿using System;
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
                StatusManager.Load();

                foreach (var lesson in StatusManager.Status.Lessons)
                {
                    if (!lesson.HasNewAnnouncement &&
                        !lesson.HasNewDocument &&
                        !lesson.HasDeadLine)
                        continue;

                    Console.Out.WriteLine(
                                          $"{(lesson.HasNewAnnouncement ? "A" : " ")} {(lesson.HasNewDocument ? "F" : " ")} {(lesson.HasDeadLine ? "D" : " ")} {lesson.Name}");
                }

                foreach (var deadLine in StatusManager.Status.DeadLines)
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
