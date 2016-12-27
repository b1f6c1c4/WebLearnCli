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
            if (Confirm)
            {
                Console.WriteLine("To process:");
                foreach (var lesson in lst2)
                    Console.WriteLine($"{lesson.Term} {lesson.Name}");
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
    }
}
