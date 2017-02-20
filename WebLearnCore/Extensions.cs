using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            Announcements = $"lessons/{Lesson}/announcements.json".InDb().LoadJson<List<Announcement>>();
            Documents = $"lessons/{Lesson}/documents.json".InDb().LoadJson<List<Document>>();
            Assignments = $"lessons/{Lesson}/assignments.json".InDb().LoadJson<List<Assignment>>();
        }

        public void Save()
        {
            Directory.CreateDirectory($"lessons/{Lesson}/".InDb());
            $"lessons/{Lesson}/announcements.json".InDb().SaveJson(Announcements);
            $"lessons/{Lesson}/documents.json".InDb().SaveJson(Documents);
            $"lessons/{Lesson}/assignments.json".InDb().SaveJson(Assignments);
        }

        public void Merge(IEnumerable<Announcement> objs) =>
            Announcements = Announcements == null
                ? objs.ToList()
                : objs.GroupJoin(Announcements, o => o.Id, o => o.Id, Merge).ToList();

        public void Merge(IEnumerable<Document> objs) =>
            Documents = Documents == null
                ? objs.ToList()
                : objs.GroupJoin(Documents, o => o.Id, o => o.Id, Merge).ToList();

        public void Merge(IEnumerable<Assignment> objs) =>
            Assignments = Assignments == null
                ? objs.ToList()
                : objs.GroupJoin(Assignments, o => o.Id, o => o.Id, Merge).ToList();

        private static T Merge<T>(T newObj, IEnumerable<T> oldObj)
            where T:Extension
        {
            var obj = oldObj.SingleOrDefault();
            if (obj != null)
                newObj.IsIgnored = obj.IsIgnored;
            return newObj;
        }
    }
}
