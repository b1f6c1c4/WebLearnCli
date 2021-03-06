﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebLearnCli.Crawler
{
    internal class CrawlerNew : CrawlerBase, ILessonExtensionCrawler
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
            try
            {
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
            catch (Exception)
            {
                return new List<Announcement>();
            }
        }

        public async Task<List<Document>> GetDocuments(Lesson lesson)
        {
            var req =
                Get(
                    $"http://learn.cic.tsinghua.edu.cn/b/myCourse/tree/getCoursewareTreeData/{lesson.CourseId}/0");
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/coursenotice/{lesson.CourseId}";

            var s = await ReadJsonToEnd(req);
            try
            {
                return ((JProperty)s["resultList"].Children().First()).Value["childMapData"]
                    .Where(j => j.HasValues)
                    .SelectMany(j => ((JProperty)j).Value["courseCoursewareList"])
                    .Select(
                        j =>
                            new Document
                                {
                                    Id = j["resourcesMappingByFileId"]["fileId"].Value<string>(),
                                    Title = j["title"].Value<string>(),
                                    Abstract = j["detail"].Value<string>(),
                                    Date = FromUnix(j["resourcesMappingByFileId"]["regDate"].Value<long>()),
                                    FileName = j["resourcesMappingByFileId"]["fileName"].Value<string>(),
                                    IsIgnored = true, // TODO
                                    FileSize = Convert.ToDouble(j["resourcesMappingByFileId"]["fileSize"].Value<string>()),
                                    FileUrl =
                                        $"http://learn.cic.tsinghua.edu.cn/b/resource/downloadFile/{j["resourcesMappingByFileId"]["fileId"].Value<string>()}"
                                }).ToList();

            }
            catch (Exception)
            {
                return new List<Document>();
            }
        }

        public async Task<List<Assignment>> GetAssignments(Lesson lesson)
        {
            var req =
                Get(
                    $"http://learn.cic.tsinghua.edu.cn/b/myCourse/homework/list4Student/{lesson.CourseId}/0");
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/homework/{lesson.CourseId}";

            var s = await ReadJsonToEnd(req);
            try
            {
                return s["resultList"]
                    .Children()
                    .Select(
                        j =>
                            new Assignment
                                {
                                    Id = j["courseHomeworkInfo"]["homewkId"].Value<int>().ToString(),
                                    Title = j["courseHomeworkInfo"]["title"].Value<string>(),
                                    Content = j["courseHomeworkInfo"]["detail"].Value<string>(),
                                    Date = FromUnix(j["courseHomeworkInfo"]["beginDate"].Value<long>()),
                                    DueDate = FromUnix(j["courseHomeworkInfo"]["endDate"].Value<long>()),
                                    IsSubmitted = j["courseHomeworkRecord"]["status"].Value<string>() != "0",
                                    FileSize =
                                        j["courseHomeworkRecord"]["resourcesMappingByHomewkAffix"] is JValue
                                            ? 0
                                            : Convert.ToDouble(
                                                j["courseHomeworkRecord"]
                                                    ["resourcesMappingByHomewkAffix"]["fileSize"]
                                                    .Value<string>()),
                                    FileUrl =
                                        j["courseHomeworkRecord"]["resourcesMappingByHomewkAffix"] is JValue
                                            ? null
                                            : $"http://learn.cic.tsinghua.edu.cn/b/resource/downloadFile/{j["courseHomeworkInfo"]["homewkAffix"]}",
                                    FileName =
                                        j["courseHomeworkRecord"]["resourcesMappingByHomewkAffix"] is JValue
                                            ? null
                                            : j["courseHomeworkInfo"]["homewkAffixFilename"].Value<string>(),
                                    Score = j["courseHomeworkRecord"]["mark"].Value<double?>()
                                        ?.ToString(CultureInfo.InvariantCulture),
                                    Assess = j["courseHomeworkRecord"]["replyDetail"].Value<string>()
                                }).ToList();

            }
            catch (Exception)
            {
                return new List<Assignment>();
            }
        }

        public async Task DownloadFile(Lesson lesson, ExtensionWithFile obj)
        {
            if (Check(lesson, obj))
                return;

            var req0 = Post(obj.FileUrl, "");
            req0.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/courseware/{lesson.CourseId}";
            var j = await ReadJsonToEnd(req0);

            var req = Get($"http://learn.cic.tsinghua.edu.cn{j["result"].Value<string>()}");
            req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/courseware/{lesson.CourseId}";
            req.UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/56.0.2924.87 Safari/537.36";
            req.Headers["Accept-Encoding"] = "gzip, deflate, sdch";
            req.Headers["Accept-Language"] = "zh-CN,zh;q=0.8,en;q=0.6";
            await DownloadFile(lesson, obj, req);
        }

        public async Task SubmitAssignment(Lesson lesson, Assignment obj) => throw new NotImplementedException();

        private static DateTime FromUnix(long val) =>
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(val).ToLocalTime();
    }
}
