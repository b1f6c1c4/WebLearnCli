using System;
using System.Collections.Generic;
using System.Linq;
using WebLearnCore;

namespace WebLearnCli
{
    public class Filter
    {
        private readonly string m_Lesson;
        private readonly bool m_Announcement;
        private readonly bool m_Document;
        private readonly bool m_Assignment;
        private readonly string m_Title;
        private readonly int m_Index;

        public static IEnumerable<Lesson> GetLessons(bool previous = false, bool noCurrent = false) =>
            Config.Inst.Lessons
                  .Where(l => previous || l.Term == TermInfo.Current)
                  .Where(l => !noCurrent || l.Term != TermInfo.Current);

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
                    if (id >= 0 &&
                        id < lst.Count)
                        return lst[id];

                if (s == "r" ||
                    s == "R")
                    break;
            }

            // don't manually optimize this
            // ReSharper disable once TailRecursiveCall
            return GetLesson(s, raw);
        }

        public bool IsMatch(Lesson lesson)
        {
            if (m_Lesson == "*")
                return true;

            return lesson.Name == m_Lesson || lesson.Alias.Contains(m_Lesson);
        }

        public IEnumerable<Assignment> Filt(List<Assignment> objs)
        {
            if (!m_Assignment)
                return Enumerable.Empty<Assignment>();

            return Filt(objs, o => o.Title);
        }

        private IEnumerable<T> Filt<T>(IReadOnlyList<T> objs, Func<T, string> title)
        {
            if (m_Title != null)
            {
                var obj = objs.SingleOrDefault(o => title(o).Trim() == m_Title);
                yield return obj;
                yield break;
            }

            if (m_Index == -1)
            {
                yield return objs[m_Index];
                yield break;
            }

            foreach (var obj in objs)
                yield return obj;
        }

        public Filter(string path)
        {
            var t = path.Trim();
            var lid = t.IndexOf('/');
            if (lid == 0)
                // deadline shorthand
                throw new NotImplementedException();

            if (lid < 0)
            {
                m_Lesson = t;
                return;
            }

            m_Lesson = t.Substring(0, lid);
            t = t.Substring(lid + 1);
            if (string.IsNullOrWhiteSpace(t))
                throw new FormatException();

            if (t == "*")
            {
                m_Announcement = true;
                m_Document = true;
                m_Assignment = true;
                m_Title = null;
                m_Index = -1;
                return;
            }

            if (int.TryParse(t, out m_Index))
            {
                m_Announcement = false;
                m_Document = false;
                m_Assignment = true;
                m_Title = null;
                return;
            }

            if (t.StartsWith("a", StringComparison.OrdinalIgnoreCase))
            {
                m_Announcement = true;
                m_Document = false;
                m_Assignment = false;
            }
            else if (t.StartsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                m_Announcement = false;
                m_Document = true;
                m_Assignment = false;
            }
            else if (t.StartsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                m_Announcement = false;
                m_Document = false;
                m_Assignment = true;
            }
            else
                throw new FormatException(path);

            if (t[1] != '/')
                throw new FormatException(path);

            t = t.Substring(2);


            if (int.TryParse(t, out m_Index))
            {
                m_Title = null;
                return;
            }

            m_Title = t.Trim();
            m_Index = -1;
        }
    }
}
