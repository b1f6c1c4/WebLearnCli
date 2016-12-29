using System;
using System.Collections.Generic;
using System.Linq;
using WebLearnCore;

namespace WebLearnCli
{
    internal class ShowCommand : PathCommandBase
    {
        public ShowCommand() : base("show", "show lesson information.") { }

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            if (lessons.Any())
                throw new InvalidOperationException();
            return 0;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Announcement> objs)
        {
            var lst = objs.ToList();
            for (var i = 0; i < lst.Count; i++)
            {
                var obj = lst[i];
                Console.WriteLine("==================================");
                Console.Write(ext.Announcements.Count == lst.Count ? $"[{i,3}]" : "[   ]");
                Console.WriteLine($" {lesson.Term} {lesson.Name}/a/{obj.Title}");
                Console.WriteLine($"Date: {obj.Date:yyyyMMdd} From: {obj.From}");
                Console.WriteLine(obj.Content);
                Console.WriteLine();
            }
            return 0;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Document> objs)
        {
            var lst = objs.ToList();
            for (var i = 0; i < lst.Count; i++)
            {
                var obj = lst[i];
                Console.WriteLine("==================================");
                Console.Write(ext.Documents.Count == lst.Count ? $"[{i,3}]" : "[   ]");
                Console.WriteLine($" {lesson.Term} {lesson.Name}/a/{obj.Title}");
                Console.WriteLine($"Date: {obj.Date:yyyyMMdd}");
                Console.WriteLine($"File: {obj.FileSize,20:N0}B {obj.FileName}");
                Console.WriteLine(obj.Abstract);
                Console.WriteLine();
            }
            return 0;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Assignment> objs)
        {
            var lst = objs.ToList();
            for (var i = 0; i < lst.Count; i++)
            {
                var obj = lst[i];
                Console.Write("==================================");
                if (obj.IsIgnored)
                    Console.WriteLine();
                else if (!obj.IsSubmitted)
                    Console.WriteLine("[NOT SUBMITTED]");
                else
                    Console.WriteLine("[SUBMITTED]");
                Console.Write(ext.Assignments.Count == lst.Count ? $"[{i,3}]" : "[   ]");
                Console.WriteLine($" {lesson.Term} {lesson.Name}/a/{obj.Title}");
                Console.WriteLine($"Date: {obj.Date:yyyyMMdd}");
                Console.WriteLine($"Due:  {obj.DueDate:yyyyMMdd}");
                Console.WriteLine($"File: {obj.FileSize,20:N0}B {obj.FileName}");
                Console.WriteLine($"{obj.Score,10} {obj.Assess}");
                Console.WriteLine(obj.Content);
                Console.WriteLine();
            }
            return 0;
        }
    }
}
