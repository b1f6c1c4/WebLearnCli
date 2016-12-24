using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebLearnCore.Crawler
{
    internal partial class CrawlerOld : CrawlerBase
    {
        private string m_MyCourse;

        public async Task Login(WebLearnCredential cred)
        {
            var req = Post(
                           "https://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp",
                           $"userid={cred.Username}&userpass={cred.Password}&submit1=%E7%99%BB%E5%BD%95");

            req.Accept = "text/html, application/xhtml+xml, */*";
            req.Referer = "http://learn.tsinghua.edu.cn/index.jsp";

            var s = await ReadToEnd(req);
            if (s.IndexOf("用户名或密码错误，登录失败！", StringComparison.Ordinal) >= 0)
                throw new AuthenticationException();
        }

        public async Task<string> FetchRoamingTicket()
        {
            var req = Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn");
            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/mainstudent.jsp";

            m_MyCourse = await ReadToEnd(req);

            var regex2 =
                new Regex(
                    @"(?<ticket>http://learn\.cic\.tsinghua\.edu\.cn/j_spring_security_thauth_roaming_entry.*renewSession)");
            return regex2.Match(m_MyCourse).Groups["ticket"].Value;
        }

        public async Task<Term> FetchCurrentLessonList()
        {
            var regex =
                new Regex(
                    @"(?:<a.*?course_locate\.jsp\?course_id=(?<id>[0-9a-zA-Z-]+).*?>|<a.*?coursehome/(?<idx>[0-9a-zA-Z-]+).*?>)\s*(?<name>.*?)\((?<index>[0-9]+)\)\((?<term>[0-9]{4}-[0-9]{4}\S{4})\)</a>[\s\S]*?span.*?>(?<homework>[0-9]+)</span>[\s\S]*?<span.*?>(?<note>[0-9]+)</span>[\s\S]*?<span.*?>(?<file>[0-9]+)</span>.*?</td>");

            var terms = await ParseLessonList(regex, m_MyCourse);

            return terms.SingleOrDefault();
        }

        public async Task<List<Term>> FetchPreviousLessonList()
        {
            var regex =
                new Regex(
                    @"(?:<a.*?course_locate\.jsp\?course_id=(?<id>[0-9a-zA-Z-]+).*?>|<a.*?coursehome/(?<idx>[0-9a-zA-Z-]+).*?>)\s*(?<name>.*?)\((?<index>[0-9]+)\)\((?<term>[0-9]{4}-[0-9]{4}\S{4})\)</a>");

            var req = Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?typepage=2");
            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn";

            return await ParseLessonList(regex, await ReadToEnd(req));
        }

        private async Task<List<Term>> ParseLessonList(Regex regex, string s)
        {
            var objs = new List<Term>();
            var rawObjs = new Dictionary<string, List<Lesson>>();
            var tasks = new List<Task>();

            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];

                List<Lesson> lst;
                TermInfo termInfo = match.Groups["term"].Value;
                if (!rawObjs.TryGetValue(match.Groups["term"].Value, out lst))
                {
                    lst = new List<Lesson>();
                    rawObjs.Add(match.Groups["term"].Value, lst);
                    objs.Add(new Term { Info = termInfo, Lessons = lst });
                }

                if (string.IsNullOrEmpty(match.Groups["idx"].Value))
                {
                    var obj = new Lesson
                                  {
                                      Term = termInfo,
                                      CourseId = match.Groups["id"].Value,
                                      Version = false,
                                      Name = match.Groups["name"].Value,
                                      Index = Convert.ToInt32(match.Groups["index"].Value)
                                  };
                    tasks.Add(GetBbsId(obj));
                    lst.Add(obj);
                }
                else
                    lst.Add(
                            new Lesson
                                {
                                    Term = termInfo,
                                    CourseId = match.Groups["idx"].Value,
                                    Version = true,
                                    Name = match.Groups["name"].Value,
                                    Index = Convert.ToInt32(match.Groups["index"].Value)
                                });
            }

            await Task.WhenAll(tasks);
            return objs;
        }

        private async Task GetBbsId(Lesson obj)
        {
            var req =
                Head(
                     $"http://learn.tsinghua.edu.cn/MultiLanguage/public/bbs/getnoteid_student.jsp?course_id={obj.CourseId}");

            req.AllowAutoRedirect = false;
            req.Referer =
                $"http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/course_locate.jsp?course_id={obj.CourseId}";

            var res = await req.GetResponseAsync();
            try
            {
                var url = res.Headers["Location"];
                var regex = new Regex(@"bbs_id=(?<bbs>[0-9].+?)&");
                obj.BbsId = regex.Match(url).Groups["bbs"].Value;
            }
            finally
            {
                res.Close();
                req.Abort();
            }
        }

        public async Task DownloadDocument(Lesson lesson, Document obj)
        {
            if (Check(lesson, obj))
                return;

            await DownloadDocument(lesson, obj, Get(obj.Url));
        }

        public async Task DownloadAssignment(Lesson lesson, Assignment obj)
        {
            if (Check(lesson, obj))
                return;

            await DownloadAssignment(lesson, obj, Get(obj.FileUrl));
        }

        public async Task SubmitAssignment(Lesson lesson, Assignment obj) { throw new NotImplementedException(); }
    }
}
