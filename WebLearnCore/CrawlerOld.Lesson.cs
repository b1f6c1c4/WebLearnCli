using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebLearnCore
{
    public partial class CrawlerOld
    {
        public async Task<LessonExtension> FetchLesson(Lesson lesson)
        {
            if (lesson.Version)
            {
                var ann = m_NewFacade.GetAnnouncements(lesson);

                await Task.WhenAll(ann);

                return
                    new LessonExtension
                        {
                            Announcements = ann.Result
                        };
            }
            else
            {
                var ann = GetAnnouncements(lesson);
                var doc = GetDocuments(lesson);

                await Task.WhenAll(ann, doc);

                return
                    new LessonExtension
                        {
                            Announcements = ann.Result,
                            Documents = doc.Result
                        };
            }
        }

        private async Task<List<Announcement>> GetAnnouncements(Lesson obj)
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

            return lst;
        }

        private async Task<List<Document>> GetDocuments(Lesson obj)
        {
            var regex =
                new Regex(
                    @"&getfilelink=(?<filename>[^&]*)&id=(?<id>[0-9]+)""[\s\S]*?<a.*?href=""(?<url>/uploadFile/downloadFile_student\.jsp\?[^""]*&file_id=\k<id>)""\s*>\s*(?<title>.*?)\s*</a>\s*</td>\s*<td[^>]*>(?<abstract>[^<]*)</td>\s*<td[^>]*>(?<size>[0-9]*\.?[0-9]*[BKMG])</td>\s*<td[^>]*>(?<date>\d{4}-\d{2}-\d{2})</td>\s*<td[^>]*>\s*(?<state>.*?)\s*</td>");
            var req =
                Get($"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/download.jsp?course_id={obj.CourseId}");
            req.Referer =
                $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/course_locate.jsp?course_id={obj.CourseId}";

            var s = await ReadToEnd(req);

            var lst = new List<Document>();
            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];
                var doc =
                    new Document
                        {
                            Id = match.Groups["id"].Value,
                            Title = match.Groups["title"].Value,
                            Abstract = match.Groups["abstract"].Value,
                            Date = Convert.ToDateTime(match.Groups["date"].Value),
                            Size = SizeParser.Parse(match.Groups["size"].Value),
                            IsRead = match.Groups["state"].Length == 0,
                            FileName = match.Groups["filename"].Value,
                            Url = $"http://learn.tsinghua.edu.cn/{match.Groups["url"].Value}"
                        };
                lst.Add(doc);
            }

            return lst;
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

    internal static class SizeParser
    {
        public static double Parse(string val)
        {
            if (val.EndsWith("B", StringComparison.OrdinalIgnoreCase))
                return Convert.ToDouble(val.Substring(0, val.Length - 1));
            if (val.EndsWith("K", StringComparison.OrdinalIgnoreCase))
                return Convert.ToDouble(val.Substring(0, val.Length - 1)) * 1024;
            if (val.EndsWith("M", StringComparison.OrdinalIgnoreCase))
                return Convert.ToDouble(val.Substring(0, val.Length - 1)) * 1024 * 1024;
            if (val.EndsWith("G", StringComparison.OrdinalIgnoreCase))
                return Convert.ToDouble(val.Substring(0, val.Length - 1)) * 1024 * 1024 * 1024;
            return Convert.ToDouble(val);
        }
    }
}
