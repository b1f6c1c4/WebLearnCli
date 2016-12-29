using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal abstract class CommandBase : ConsoleCommand
    {
        protected bool Confirm;

        protected CommandBase(string command, string oneLineDescription = "")
        {
            IsCommand(command, oneLineDescription);
            HasOption("confirm", "ask before proceed", t => Confirm = t != null);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                return ConcreteRun(remainingArguments);
            }
            catch (AuthenticationException)
            {
                Console.Error.WriteLine("Invalid credential.");
                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unknown error: {e}");
                return 1;
            }
        }

        protected abstract int ConcreteRun(string[] remainingArguments);

        protected static void AskForProceed()
        {
            Console.WriteLine("Proceed? Y/n");
            while (true)
            {
                var s = Console.ReadLine();
                if (string.IsNullOrEmpty(s))
                    break;
                if ("n".Equals(s, StringComparison.OrdinalIgnoreCase))
                    throw new OperationCanceledException();
                if ("no".Equals(s, StringComparison.OrdinalIgnoreCase))
                    throw new OperationCanceledException();
                if ("y".Equals(s, StringComparison.OrdinalIgnoreCase))
                    break;
                if ("yes".Equals(s, StringComparison.OrdinalIgnoreCase))
                    break;
            }
        }
    }

    internal abstract class SelectionCommandBase : CommandBase
    {
        protected bool Previous;
        protected bool NoCurrent;

        protected SelectionCommandBase(string command, string oneLineDescription = "")
            : base(command, oneLineDescription)
        {
            HasOption("previous", "include old lessons.", t => Previous = t != null);
            HasOption("no-current", "exclude current lessons.", t => NoCurrent = t != null);
        }
    }

    internal abstract class LessonCommandBase : SelectionCommandBase
    {
        protected LessonCommandBase(string command, string oneLineDescription = "")
            : base(command, oneLineDescription)
        {
            HasAdditionalArguments(null, "lessons");
        }

        protected override sealed int ConcreteRun(string[] remainingArguments)
        {
            var lst = Filter.GetLessons(Previous, NoCurrent).ToList();
            var lst2 = remainingArguments.Length != 0
                           ? remainingArguments.Select(arg => Filter.GetLesson(arg, lst)).ToList()
                           : lst;

            Console.WriteLine("To process:");
            foreach (var lesson in lst2)
                Console.WriteLine($"{lesson.Term} {lesson.Name}");
            if (Confirm)
                AskForProceed();

            Console.WriteLine("Processing...");
            return ConcreteRun(lst2);
        }

        protected abstract int ConcreteRun(IEnumerable<Lesson> lessons);
    }

    internal abstract class PathCommandBase : SelectionCommandBase
    {
        protected PathCommandBase(string command, string oneLineDescription = "") : base(command, oneLineDescription)
        {
            HasAdditionalArguments(null, "paths");
            HasLongDescription(@"Path syntax:
*            any lesson
<L>          the lesson
<L>/*        everything of the lesson
<L>/a        every announcement of the lesson
<L>/f        every file of the lesson
<L>/d        every deadline of the lesson
<L>/<T>/<t>  select by title, <T> is a/f/d
<L>/<T>//<n> select by index (from 0)
<L>/<n>      select deadline by index
/<n>         select deadline by (0 is the most urgent)");
        }

        protected override sealed int ConcreteRun(string[] remainingArguments)
        {
            var lst = Filter.GetLessons(Previous, NoCurrent);
            var filters = remainingArguments.Select(arg => new Filter(arg)).ToList();

            var lst2 = lst.Where(l => filters.Any(f => f.IsMatch(l))).ToList();
            var exts = lst2.Select(Facade.LoadExtension).ToList();
            var lstSelf = lst2.Where(l => filters.Any(f => f.IsSelfMatch(l))).ToList();

            var lstAnn = new List<List<Announcement>>();
            var lstDoc = new List<List<Document>>();
            var lstAss = new List<List<Assignment>>();

            foreach (var ext in exts)
            {
                lstAnn.Add(
                           filters.Select(f => f.Filt(ext.Announcements))
                                  .Aggregate(Enumerable.Empty<Announcement>(), Enumerable.Union)
                                  .ToList());
                lstDoc.Add(
                           filters.Select(f => f.Filt(ext.Documents))
                                  .Aggregate(Enumerable.Empty<Document>(), Enumerable.Union)
                                  .ToList());
                lstAss.Add(
                           filters.Select(f => f.Filt(ext.Assignments))
                                  .Aggregate(Enumerable.Empty<Assignment>(), Enumerable.Union)
                                  .ToList());
            }

            Console.WriteLine("To process:");
            foreach (var lesson in lstSelf)
                Console.WriteLine($"{lesson.Term} {lesson.Name}");
            for (var i = 0; i < lst2.Count; i++)
            {
                var lesson = lst2[i];
                foreach (var obj in lstAnn[i])
                    Console.WriteLine($"{lesson.Term} {lesson.Name}/a/{obj.Title}");
                foreach (var obj in lstDoc[i])
                    Console.WriteLine($"{lesson.Term} {lesson.Name}/f/{obj.Title}");
                foreach (var obj in lstAss[i])
                    Console.WriteLine($"{lesson.Term} {lesson.Name}/d/{obj.Title}");
            }
            if (Confirm)
                AskForProceed();

            if (lstSelf.Count > 0)
            {
                Console.WriteLine("Processing lessons...");
                var ret = ConcreteRun(lstSelf);
                if (ret != 0)
                    return ret;
            }

            if (lstAnn.Any(l => l.Count > 0))
            {
                Console.WriteLine("Processing announcements...");
                for (var i = 0; i < lst2.Count; i++)
                {
                    var ret = ConcreteRun(lst2[i], exts[i], lstAnn[i]);
                    if (ret != 0)
                        return ret;
                }
            }

            if (lstDoc.Any(l => l.Count > 0))
            {
                Console.WriteLine("Processing files...");
                for (var i = 0; i < lst2.Count; i++)
                {
                    var ret = ConcreteRun(lst2[i], exts[i], lstDoc[i]);
                    if (ret != 0)
                        return ret;
                }
            }

            if (lstAss.Any(l => l.Count > 0))
            {
                Console.WriteLine("Processing deadlines...");
                for (var i = 0; i < lst2.Count; i++)
                {
                    var ret = ConcreteRun(lst2[i], exts[i], lstAss[i]);
                    if (ret != 0)
                        return ret;
                }
            }

            for (var i = 0; i < lst2.Count; i++)
                if (lstAnn[i].Count > 0 ||
                    lstDoc[i].Count > 0 ||
                    lstAss[i].Count > 0)
                    Facade.SaveExtension(lst2[i], exts[i]);

            Config.Save();
            Facade.GenerateStatus();

            return 0;
        }

        protected abstract int ConcreteRun(IEnumerable<Lesson> lessons);

        protected abstract int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Announcement> objs);

        protected abstract int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Document> objs);

        protected abstract int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Assignment> objs);
    }

    internal abstract class PathExtCommandBase : PathCommandBase
    {
        protected PathExtCommandBase(string command, string oneLineDescription = "") : base(command, oneLineDescription) { }

        protected override sealed int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Announcement> objs) =>
            ConcreteRun(lesson, objs);

        protected override sealed int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Document> objs) =>
            ConcreteRun(lesson, objs);

        protected override sealed int ConcreteRun(Lesson lesson, LessonExtension ext, IEnumerable<Assignment> objs) =>
            ConcreteRun(lesson, objs);

        protected abstract int ConcreteRun(Lesson lesson, IEnumerable<Extension> objs);
    }
}
