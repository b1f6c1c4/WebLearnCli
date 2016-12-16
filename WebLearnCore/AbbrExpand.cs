using System;
using System.Collections.Generic;
using System.Linq;

namespace WebLearnCore
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
        public IComparer<TOut> RawComparer;

        public int Compare(T x, T y) =>
            (RawComparer ?? Comparer<TOut>.Default).Compare(Functor(x), Functor(y));
    }

    internal static class AbbrExpand
    {
        public static Lesson GetLesson(string str)
        {
            ConfigManager.Load();

            var lst = ConfigManager.Config.Lessons.Where(l => l.Name == str || l.Alias.Contains(str)).ToList();
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
    }
}
