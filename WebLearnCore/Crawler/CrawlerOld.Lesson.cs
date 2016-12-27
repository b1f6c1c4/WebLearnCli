using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace WebLearnCore.Crawler
{
    internal partial class CrawlerOld : ILessonExtensionCrawler
    {
        public async Task<List<Announcement>> GetAnnouncements(Lesson obj)
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
                                   From = match.Groups["from"].Value,
                                   Date = Convert.ToDateTime(match.Groups["date"].Value)
                               };
                lst.Add(note);
                tasks.Add(GetAnnouncement(obj, note));
            }
            await Task.WhenAll(tasks);

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
                    @"正文</td>\s*<td.*?>(?<content>[\s\S]*?)</td>\s*</tr>\s*<tr>\s*<td class=""info_b"" colspan=""4""><img src=""/img/spacer\.gif"" /></td>\s*</tr>");
            obj.Content = regex.Match(s).Groups["content"].Value;
        }

        public async Task<List<Document>> GetDocuments(Lesson obj)
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
                            FileSize = ParseSize(match.Groups["size"].Value),
                            IsIgnored = match.Groups["state"].Length == 0,
                            FileName = match.Groups["filename"].Value,
                            FileUrl = $"http://learn.tsinghua.edu.cn/{match.Groups["url"].Value}"
                        };
                lst.Add(doc);
            }

            return lst;
        }

        public async Task<List<Assignment>> GetAssignments(Lesson obj)
        {
            var regex =
                new Regex(
                    @"<tr[^>]*>\s*<td[^>]*><a\s+href=""hom_wk_detail\.jsp\?id=(?<id>\d+)&course_id=\d+&rec_id=(?<rec>null|\d+)"">(?<title>[^<]*)</a></td>\s*<td[^>]*>(?<start>\d{4}-\d{2}-\d{2})</td>\s*<td[^>]*>(?<due>\d{4}-\d{2}-\d{2})</td>\s*<td[^>]*>\s*(?<state>.*)\s*</td>\s*<td[^>]*>(?<size>[0-9]*\.?[0-9]*[KMG]?B)\s*</td>\s*<td[^>]*>[\s\S]*?查看批阅""\s*(?<scored>disabled=""true"")?\s*type=""button""");
            var req =
                Get(
                    $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_brw.jsp?course_id={obj.CourseId}");
            req.Referer =
                $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/course_locate.jsp?course_id={obj.CourseId}";

            var s = await ReadToEnd(req);

            var lst = new List<Assignment>();

            var tasks = new List<Task>();
            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];
                var ass =
                    new Assignment
                        {
                            Id = match.Groups["id"].Value,
                            Title = match.Groups["title"].Value,
                            FileSize = ParseSize(match.Groups["size"].Value),
                            Date = Convert.ToDateTime(match.Groups["start"].Value),
                            DueDate = Convert.ToDateTime(match.Groups["due"].Value).AddDays(1).AddSeconds(-1),
                            RecId = match.Groups["rec"].Value,
                            IsSubmitted = Purify(match.Groups["state"].Value) == "已经提交"
                        };
                tasks.Add(GetAssignment(obj, ass));
                if (match.Groups["scored"].Length == 0)
                    tasks.Add(GetAssignmentScore(obj, ass));
                lst.Add(ass);
            }
            await Task.WhenAll(tasks);

            return lst;
        }

        private async Task GetAssignment(Lesson lesson, Assignment obj)
        {
            var req =
                Get(
                    $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_detail.jsp?id={obj.Id}&course_id={lesson.CourseId}&rec_id={obj.RecId}");

            req.Referer =
                $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_brw.jsp?course_id={lesson.CourseId}";

            var s = await ReadToEnd(req);

            var regex =
                new Regex(
                    @"<textarea[^>]*>(?<content>[\s\S]*?)</textarea>[\s\S]*?作业附件</td>\s*<td[^>]*>(?<supp>[\s\S]*?)</td>");
            var match = regex.Match(s);
            obj.Content = match.Groups["content"].Value;

            var regex2 =
                new Regex(
                    @"<a[^>]*href=""(?<url>/uploadFile/downloadFile\.jsp\?[^""]*)"">(?<file>[^<]*)</a>");
            var match2 = regex2.Match(match.Groups["supp"].Value);
            if (!match2.Success)
                return;

            obj.FileUrl = $"http://learn.tsinghua.edu.cn/{match2.Groups["url"].Value}";
            obj.FileName = Purify(match2.Groups["file"].Value);
        }

        private async Task GetAssignmentScore(Lesson lesson, Assignment obj)
        {
            var req =
                Get(
                    $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_view.jsp?id={obj.Id}&course_id={lesson.CourseId}");

            req.Referer =
                $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/hom_wk_brw.jsp?course_id={lesson.CourseId}";

            var s = await ReadToEnd(req);

            var regex =
                new Regex(
                    @"得分</td>\s*<td[^>]*>\s*(?<score>[\s\S]*?)\s*</td>[\s\S]*?<textarea[^>]*>(?<assess>[\s\S]*?)</textarea>");
            var match = regex.Match(s);
            obj.Score = Purify(match.Groups["score"].Value);
            obj.Assess = Purify(match.Groups["assess"].Value);
        }

        private static string Purify(string val) => HttpUtility.HtmlDecode(val).Trim();

        private static double ParseSize(string val)
        {
            if (val.EndsWith("B", StringComparison.OrdinalIgnoreCase))
                val = val.Substring(0, val.Length - 1);
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
