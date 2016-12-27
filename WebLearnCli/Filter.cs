using System;
using System.Collections.Generic;
using System.Linq;
using WebLearnCore;

namespace WebLearnCli
{
    public static class Filter
    {
        public static Lesson GetLesson(string str, IReadOnlyCollection<Lesson> raw)
        {
            var lst = raw
                .Where(l => l.Name == str || l.Alias.Contains(str))
                .ToList();
            if (lst.Count == 0)
                throw new ApplicationException($"Lesson \"{str}\" not found.");
            if (lst.Count == 1)
                return lst.SingleOrDefault();

            Console.WriteLine("Multiple lessons match:");
            for (var i = 0; i < lst.Count; i++)
            {
                var lesson = lst[i];
                Console.WriteLine($"[{i}] {lesson.Term} {lesson.Name}");
            }

            Console.WriteLine("Type the number to select, or 'r'/'R' to enter query again:");
            Console.Write(">");
            string s;
            while (true)
            {
                s = Console.ReadLine();
                int id;
                if (int.TryParse(s, out id))
                    return lst[id];

                if (s == "r" ||
                    s == "R")
                    break;
            }

            // don't manually optimize this
            // ReSharper disable once TailRecursiveCall
            return GetLesson(s, raw);
        }

        public static IEnumerable<Lesson> GetLessons(bool previous = false, bool noCurrent = false) =>
            Config.Inst.Lessons
                  .Where(l => previous || l.Term == TermInfo.Current)
                  .Where(l => !noCurrent || l.Term != TermInfo.Current);
    }
}
