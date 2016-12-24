using System;
using System.Collections.Generic;
using System.Linq;
using WebLearnCore;

namespace WebLearnCli
{
    internal sealed class ChainedComparer<T> : IComparer<T>
    {
        public IComparer<T> FirstComparer;
        public IComparer<T> SecondComparer;

        public int Compare(T x, T y)
        {
            var c = FirstComparer.Compare(x, y);
            return c != 0 ? c : (SecondComparer.Compare(x, y));
        }
    }

    internal sealed class FunctorComparer<T, TOut> : IComparer<T>
    {
        public Func<T, TOut> Functor;
        public IComparer<TOut> RawComparer = Comparer<TOut>.Default;

        public int Compare(T x, T y) =>
            RawComparer.Compare(Functor(x), Functor(y));
    }

    public static class AbbrExpand
    {
        public static Lesson GetLesson(string str, bool previous = false, bool noCurrent = false)
        {
            Config.Load();

            var lst = GetLessons(previous, noCurrent)
                .Where(l => l.Name == str || l.Alias.Contains(str))
                .ToList();
            lst.Sort(
                     new ChainedComparer<Lesson>
                         {
                             FirstComparer =
                                 new FunctorComparer<Lesson, TermInfo>
                                     {
                                         Functor = l => l.Term
                                     },
                             SecondComparer =
                                 new FunctorComparer<Lesson, int>
                                     {
                                         Functor = l => l.Ignore ? 1 : 0
                                     }
                         });
            return lst.FirstOrDefault();
        }

        public static IEnumerable<Lesson> GetLessons(bool previous = false, bool noCurrent = false) =>
            Config.Inst.Lessons
                  .Where(l => previous || l.Term == TermInfo.Current)
                  .Where(l => !noCurrent || l.Term != TermInfo.Current);

        public static IEnumerable<Lesson> GetLessons(IEnumerable<string> args, bool previous = false,
                                                     bool noCurrent = false)
        {
            foreach (var arg in args)
            {
                var l = GetLesson(arg, previous, noCurrent);
                if (l == null)
                    throw new ApplicationException($"Lesson \"{arg}\" not found.");
                yield return l;
            }
        }
    }
}
