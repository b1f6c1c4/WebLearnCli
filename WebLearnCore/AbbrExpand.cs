using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebLearnEntities;

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

    internal sealed class FunctorComparer<T,TOut> : IComparer<T>
    {
        public Func<T, TOut> Functor;
        public IComparer<TOut> RawComparer;

        public int Compare(T x, T y) =>
            (RawComparer ?? Comparer<TOut>.Default).Compare(Functor(x), Functor(y));
    }

    internal static class AbbrExpand
    {
        public static LessonSetting GetLesson(string str)
        {
            ConfigManager.Load();

            var lst = ConfigManager.Config.LessonSettings.Where(l => l.Name == str || l.Alias.Contains(str)).ToList();
            lst.Sort(
                     new ChainedComparer<LessonSetting>
                         {
                             FirstComparer =
                                 new FunctorComparer<LessonSetting, TermInfo>
                                     {
                                         Functor = l => l.Term
                                     },
                             SecondComparer =
                                 new FunctorComparer<LessonSetting, int>
                                     {
                                         Functor = l => l.Ignore ? 1 : 0
                                     }
                         });
            return lst.FirstOrDefault();
        }
    }
}
