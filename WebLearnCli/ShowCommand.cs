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
            if (ext.Announcements.Count == lst.Count)
                for (var i = 0; i < lst.Count; i++)
                {
                    var obj = lst[i];
                    Console.WriteLine("==================================");
                    Console.WriteLine($"[{i,3}] {lesson.Term} {lesson.Name}/a/{obj.Title}");
                    Console.WriteLine($"Date: {obj.Date:yyyyMMdd} From: {obj.From}");
                    Console.WriteLine(obj.Content);
                    Console.WriteLine();
                }
            else
            // ReSharper disable once ForCanBeConvertedToForeach
                for (var i = 0; i < lst.Count; i++)
                {
                    var obj = lst[i];
                    Console.WriteLine("==================================");
                    Console.WriteLine($"[   ] {lesson.Term} {lesson.Name}/a/{obj.Title}");
                    Console.WriteLine($"Date: {obj.Date:yyyyMMdd} From: {obj.From}");
                    Console.WriteLine(obj.Content);
                    Console.WriteLine();
                }
            return 1;
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Document> objs)
        {
            throw new NotImplementedException();
        }

        protected override int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Assignment> objs)
        {
            throw new NotImplementedException();
        }
    }
}
