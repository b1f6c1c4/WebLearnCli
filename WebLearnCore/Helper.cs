using System;
using System.Threading.Tasks;

namespace WebLearnCore
{
    public static class Helper
    {
        public static LessonExtension Extension(this Lesson l) => LessonExtension.From(l);

        public static Task Then<T>(this Task<T> t0, Action<T> a) => t0.ContinueWith(t => a(t.Result));
    }
}
