using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace WebLearnCore
{
    public sealed class LessonExtension
    {
        private static readonly Dictionary<Lesson, LessonExtension> Dic = new Dictionary<Lesson, LessonExtension>();

        public static LessonExtension From(Lesson lesson)
        {
            LessonExtension ext;
            if (Dic.TryGetValue(lesson, out ext))
                return ext;

            ext = new LessonExtension(lesson);
            try
            {
                ext.Load();
            }
            catch (Exception)
            {
                // ignore
            }
            Dic.Add(lesson, ext);
            return ext;
        }

        public Lesson Lesson { get; }

        public List<Announcement> Announcements { get; private set; }

        public List<Document> Documents { get; private set; }

        public List<Assignment> Assignments { get; private set; }

        private LessonExtension(Lesson lesson) { Lesson = lesson; }

        private void Load()
        {
            Announcements =
                JsonConvert
                    .DeserializeObject<List<Announcement>>
                    (File.ReadAllText(DbHelper.GetPath($"lessons/{Lesson}/announcements.json")));
            Documents =
                JsonConvert
                    .DeserializeObject<List<Document>>
                    (File.ReadAllText(DbHelper.GetPath($"lessons/{Lesson}/documents.json")));
            Assignments =
                JsonConvert
                    .DeserializeObject<List<Assignment>>
                    (File.ReadAllText(DbHelper.GetPath($"lessons/{Lesson}/assignments.json")));
        }

        public void Save()
        {
            Directory.CreateDirectory(DbHelper.GetPath($"lessons/{Lesson}/"));
            File.WriteAllText(
                              DbHelper.GetPath($"lessons/{Lesson}/announcements.json"),
                              JsonConvert.SerializeObject(Announcements, Formatting.Indented));
            File.WriteAllText(
                              DbHelper.GetPath($"lessons /{Lesson}/documents.json"),
                              JsonConvert.SerializeObject(Documents, Formatting.Indented));
            File.WriteAllText(
                              DbHelper.GetPath($"lessons/{Lesson}/assignments.json"),
                              JsonConvert.SerializeObject(Assignments, Formatting.Indented));
        }

        public void Merge(IEnumerable<Announcement> objs) =>
            Announcements = objs.Join(Announcements, o => o.Id, o => o.Id, (o1, o2) => o2).ToList();

        public void Merge(IEnumerable<Document> objs) =>
            Documents = objs.Join(Documents, o => o.Id, o => o.Id, (o1, o2) => o2).ToList();

        public void Merge(IEnumerable<Assignment> objs) =>
            Assignments = objs.Join(Assignments, o => o.Id, o => o.Id, (o1, o2) => o2).ToList();
    }
}
