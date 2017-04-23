using System;
using System.Collections.Generic;
using System.Linq;

namespace WebLearnCli
{
    internal class ShowCommand : PathCommandBase
    {
        private bool m_Ignore;

        public ShowCommand() : base("show", "show lesson information.")
        {
            HasOption("i|ignore", "ask to ignore", t => m_Ignore = t != null);
        }

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            if (lessons.Any())
                throw new InvalidOperationException();

            return 0;
        }

        private static void Show(Lesson lesson, LessonExtension ext, IList<Announcement> lst, int i)
        {
            var obj = lst[i];
            Console.WriteLine("==================================");
            Console.Write(ext.Announcements.Count == lst.Count ? $"[{i,3}]" : "[   ]");
            Console.WriteLine($" {lesson.Term} {lesson.Name}/a/{obj.Title}");
            Console.WriteLine($"Date: {obj.Date:yyyyMMdd} From: {obj.From}");
            Console.WriteLine(obj.Content);
            Console.WriteLine();
        }

        private static void Show(Lesson lesson, LessonExtension ext, IList<Document> lst, int i)
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

        private static void Show(Lesson lesson, LessonExtension ext, IList<Assignment> lst, int i)
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

        private void PromptForIgnore(Extension ext, bool defualt)
        {
            if (!m_Ignore)
                return;

            Console.Write(defualt ? "Ignore? Y/n" : "Ignore? y/N");
            while (true)
            {
                var s = Console.ReadLine();
                if (string.IsNullOrEmpty(s))
                    if (defualt)
                        break;
                    else
                        return;

                if ("y".Equals(s, StringComparison.OrdinalIgnoreCase) ||
                    "yes".Equals(s, StringComparison.OrdinalIgnoreCase))
                    break;

                if ("n".Equals(s, StringComparison.OrdinalIgnoreCase) ||
                    "no".Equals(s, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            ext.IsIgnored = true;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Announcement> objs)
        {
            var lst = objs.ToList();
            for (var i = 0; i < lst.Count; i++)
            {
                Show(lesson, ext, lst, i);
                PromptForIgnore(lst[i], true);
            }

            return 0;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Document> objs)
        {
            var lst = objs.ToList();
            for (var i = 0; i < lst.Count; i++)
            {
                Show(lesson, ext, lst, i);
                PromptForIgnore(lst[i], true);
            }

            return 0;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Assignment> objs)
        {
            var lst = objs.ToList();
            for (var i = 0; i < lst.Count; i++)
            {
                Show(lesson, ext, lst, i);
                PromptForIgnore(lst[i], false);
            }

            return 0;
        }
    }
}
