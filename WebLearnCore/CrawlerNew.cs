using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebLearnCore
{
    public class CrawlerNew : CrawlerBase
    {
        public async Task Login(string ticket)
        {
            var req = Get(ticket);
            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn";
            await ReadToEnd(req);
        }

        private static async Task<JObject> ReadJsonToEnd(WebRequest req)
        {
            var res = await req.GetResponseAsync();
            try
            {
                using (var stream = res.GetResponseStream())
                {
                    if (stream == null)
                        throw new NetworkInformationException();
                    using (var rd = new JsonTextReader(new StreamReader(stream)))
                        return JObject.Load(rd);
                }
            }
            finally
            {
                res.Close();
                req.Abort();
            }
        }

        public async Task<List<Announcement>> GetAnnouncements(Lesson lesson)
        {
            var req =
                Get(
                    $"http://learn.cic.tsinghua.edu.cn/b/myCourse/notice/listForStudent/{lesson.CourseId}?currentPage=1&pageSize=100");
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/coursenotice/{lesson.CourseId}";

            var s = await ReadJsonToEnd(req);
            return s["paginationList"]["recordList"]
                .Select(
                        j =>
                        new Announcement
                            {
                                Title = j["courseNotice"]["title"].Value<string>(),
                                Date = j["courseNotice"]["regDate"].Value<DateTime>(),
                                From = j["courseNotice"]["owner"].Value<string>(),
                                Content = j["courseNotice"]["detail"].Value<string>(),
                                Id = j["courseNotice"]["id"].Value<long>().ToString()
                            }).ToList();
        }

        public async Task<List<Document>> GetDocuments(Lesson lesson)
        {
            var req =
                Get(
                    $"http://learn.cic.tsinghua.edu.cn/b/myCourse/tree/getCoursewareTreeData/{lesson.CourseId}/0");
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/coursenotice/{lesson.CourseId}";

            var s = await ReadJsonToEnd(req);
            return ((JProperty)s["resultList"].Children().First()).Value["childMapData"].Where(j => j.HasValues)
                .SelectMany(j => ((JProperty)j).Value["courseCoursewareList"])
                .Select(
                        j =>
                        new Document
                        {
                                Id = j["resourcesMappingByFileId"]["fileId"].Value<string>(),
                                Title = j["title"].Value<string>(),
                                Abstract = j["detail"].Value<string>(),
                                Date =
                                    new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                                    .AddMilliseconds(j["resourcesMappingByFileId"]["regDate"].Value<long>())
                                    .ToLocalTime(),
                                FileName = j["resourcesMappingByFileId"]["fileName"].Value<string>(),
                                IsRead = true, // TODO
                                Size = Convert.ToDouble(j["resourcesMappingByFileId"]["fileSize"].Value<string>()),
                                Url =
                                    $"http://learn.cic.tsinghua.edu.cn/b/resource/downloadFile/{j["resourcesMappingByFileId"]["fileId"].Value<string>()}"
                            }).ToList();
        }
    }
}
