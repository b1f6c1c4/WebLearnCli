﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebLearnEntities;

namespace WebLearnNew
{
    public class Facade : CrawlerBase
    {
        public async Task Login(string ticket)
        {
            var req = Get(ticket);
            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn";
            await ReadToEnd(req);
        }

        private async Task<JObject> ReadJsonToEnd(HttpWebRequest req)
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

        public async Task GetAnnouncements(Lesson lesson)
        {
            var req =
                Get(
                    $"http://learn.cic.tsinghua.edu.cn/b/myCourse/notice/listForStudent/{lesson.CourseId}?currentPage=1&pageSize=100");
            req.Accept = "application/json, text/javascript, */*; q=0.01";
            req.Referer = $"http://learn.cic.tsinghua.edu.cn/f/student/coursenotice/{lesson.CourseId}";

            var s = await ReadJsonToEnd(req);
            lesson.Announcements = s["paginationList"]["recordList"]
                .Select(
                        j =>
                        new Announcement
                            {
                                Title = j["courseNotice"]["title"].Value<string>(),
                                Date = j["courseNotice"]["regDate"].Value<DateTime>(),
                                From = j["courseNotice"]["owner"].Value<string>(),
                                Content = j["courseNotice"]["detail"].Value<string>(),
                                Id = j["courseNotice"]["id"].Value<Int64>().ToString()
                            }).ToList();
        }
    }
}
