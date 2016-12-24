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

namespace WebLearnCore.Crawler
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
                                Size = Convert.ToDouble(j["resourcesMappingByFileId"]["fileSize"].Value<string>()),
                                Url =
                                    $"http://learn.cic.tsinghua.edu.cn/b/resource/downloadFile/{j["resourcesMappingByFileId"]["fileId"].Value<string>()}"
                            }).ToList();
        }

        public async Task<List<Assignment>> GetAssignments(Lesson lesson)
        {
            var req =
                Get(
                    $"http://learn.cic.tsinghua.edu.cn/b/myCourse/homework/list4Student/{lesson.CourseId}/0");
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/homework/{lesson.CourseId}";

            var s = await ReadJsonToEnd(req);
            return s["resultList"]
                .Children()
                .Select(
                        j =>
                        new Assignment
                            {
                                Id = j["courseHomeworkInfo"]["homewkId"].Value<int>().ToString(),
                                Title = j["courseHomeworkInfo"]["title"].Value<string>(),
                                Content = j["courseHomeworkInfo"]["detail"].Value<string>(),
                                StartDate = FromUnix(j["courseHomeworkInfo"]["beginDate"].Value<long>()),
                                DueDate = FromUnix(j["courseHomeworkInfo"]["endDate"].Value<long>()),
                                IsSubmitted = j["courseHomeworkRecord"]["status"].Value<string>() != "0",
                                Size =
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

        public async Task DownloadDocument(Lesson lesson, Document obj)
        {
            if (Check(lesson, obj))
                return;

            var req0 = Post(obj.Url, "");
            req0.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/courseware/{lesson.CourseId}";
            var j = await ReadJsonToEnd(req0);

            var req = Get($"http://learn.cic.tsinghua.edu.cn{j["result"].Value<string>()}");
            await DownloadDocument(lesson, obj, req);
        }

        public async Task DownloadAssignment(Lesson lesson, Assignment obj)
        {
            if (Check(lesson, obj))
                return;

            var req0 = Post(obj.FileUrl, "");
            req0.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/courseware/{lesson.CourseId}";
            var j = await ReadJsonToEnd(req0);

            var req = Get($"http://learn.cic.tsinghua.edu.cn{j["result"].Value<string>()}");
            await DownloadAssignment(lesson, obj, req);
        }

        public async Task SubmitAssignment(Lesson lesson, Assignment obj) { throw new NotImplementedException(); }

        private static DateTime FromUnix(long val) =>
            new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(val).ToLocalTime();
    }
}