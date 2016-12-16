using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebLearnEntities;

namespace WebLearnOld
{
    public partial class Facade
    {
        public async Task FetchLesson(WebLearnEntities.Lesson lesson)
        {
            var l = lesson as Lesson;
            if (l == null)
                await m_NewFacade.GetAnnouncements(lesson as WebLearnNew.Lesson);
            else
            {
                var ann = GetAnnouncements(l);

                await Task.WhenAll(ann);
            }
        }

        private async Task GetAnnouncements(Lesson obj)
        {
            var req =
                Get(
                    $"http://learn.tsinghua.edu.cn/MultiLanguage/public/bbs/note_list_student.jsp?bbs_id={obj.BbsId}&course_id={obj.CourseId}");

            req.Referer =
                "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/course_locate.jsp?course_id={obj.CourseId}";
            var s = await ReadToEnd(req);

            var regex =
                new Regex(
                    @"<tr.*?>[\s\S]*?<a  href='note_reply.jsp\?bbs_type=课程公告&id=(?<id>[0-9]{1,7})&course_id=\d+'>(?<title>.*?)</a>[\s\S]*?<td.*?>(?<from>.*?)</td>\s*?<td.*?>(?<date>.*?)</td>\s*?<td.*?>(?<read>.*?)</td>\s*?</tr>");

            var lst = new List<Announcement>();

            var tasks = new List<Task>();
            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];
                var note = new Announcement
                               {
                                   Id = match.Groups["id"].Value,
                                   Title = match.Groups["title"].Value,
                                   From = match.Groups["from"].Value
                               };
                DateTime date;
                if (DateTime.TryParse(match.Groups["date"].Value, out date))
                    note.Date = date;
                lst.Add(note);
                tasks.Add(GetAnnouncement(obj, note));
            }
            await Task.WhenAll(tasks);

            obj.Announcements = lst;
        }

        private async Task GetAnnouncement(Lesson lesson, Announcement obj)
        {
            var req =
                Get(
                    $"http://learn.tsinghua.edu.cn/MultiLanguage/public/bbs/note_reply.jsp?bbs_type=课程公告&id={obj.Id}&course_id={lesson.CourseId}");

            req.Referer =
                $"http://learn.tsinghua.edu.cn/MultiLanguage/public/bbs/note_list_student.jsp?bbs_id={lesson.BbsId}&course_id={lesson.CourseId}";

            var s = await ReadToEnd(req);

            var regex =
                new Regex(
                    @"正文</td>\s*<td.*?>(?<content>[\s\S]*?)</td>\s*</tr>\s*<tr>\s*<td class=""info_b"" colspan=""4""><img src=""/img/spacer.gif"" /></td>\s*</tr>");
            obj.Content = regex.Match(s).Groups["content"].Value;
        }
    }
}
