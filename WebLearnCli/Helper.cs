using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebLearnCli
{
    public static class Helper
    {
        public static LessonExtension Extension(this Lesson l) => LessonExtension.From(l);

        public static Task Then<T>(this Task<T> t0, Action<T> a) => t0.ContinueWith(t => a(t.Result));

        public static string InDb(this string val) => Path.Combine(".weblearn/", val);

        public static T LoadJson<T>(this string file) =>
            JsonConvert.DeserializeObject<T>(File.ReadAllText(file));

        public static void SaveJson<T>(this string file, T obj) =>
            File.WriteAllText(file, JsonConvert.SerializeObject(obj, Formatting.Indented));
    }
}
