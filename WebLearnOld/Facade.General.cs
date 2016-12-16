using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebLearnEntities;

namespace WebLearnOld
{
    public partial class Facade : CrawlerBase
    {
        private readonly WebLearnNew.Facade m_NewFacade = new WebLearnNew.Facade();

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

        public async Task<Term> FetchCurrentLessonList()
        {
            var regex =
                new Regex(
                    @"(?:<a.*?course_locate\.jsp\?course_id=(?<id>[0-9]+).*?>|<a.*?coursehome/(?<idx>[0-9-]+).*?>)\s*(?<name>.*?)\((?<index>[0-9]+)\)\((?<term>[0-9]{4}-[0-9]{4}\S{4})\)</a>[\s\S]*?span.*?>(?<homework>[0-9]+)</span>[\s\S]*?<span.*?>(?<note>[0-9]+)</span>[\s\S]*?<span.*?>(?<file>[0-9]+)</span>.*?</td>");

            var req = Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn");

            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/mainstudent.jsp";

            var s = await ReadToEnd(req);

            var termS = "";
            var objs = new List<WebLearnEntities.Lesson>();
            var tasks = new List<Task>();

            var regex2 =
                new Regex(
                    @"(?<ticket>http://learn\.cic\.tsinghua\.edu\.cn/j_spring_security_thauth_roaming_entry.*renewSession)");

            tasks.Add(m_NewFacade.Login(regex2.Match(s).Groups["ticket"].Value));

            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];
                termS = match.Groups["term"].Value;
                if (string.IsNullOrEmpty(match.Groups["idx"].Value))
                {
                    var obj = new Lesson
                                  {
                                      CourseId = match.Groups["id"].Value,
                                      Name = match.Groups["name"].Value,
                                      Index = Convert.ToInt32(match.Groups["index"].Value)
                                  };
                    tasks.Add(GetBbsId(obj));
                    objs.Add(obj);
                }
                else
                    objs.Add(
                             new WebLearnNew.Lesson
                                 {
                                     CourseId = match.Groups["idx"].Value,
                                     Name = match.Groups["name"].Value,
                                     Index = Convert.ToInt32(match.Groups["index"].Value)
                                 });
            }

            await Task.WhenAll(tasks);

            return new Term(termS) { Lessons = objs };
        }

        private async Task<List<Term>> FetchPreviousLessonList()
        {
            var regex =
                new Regex(
                    @"(?:<a.*?course_locate\.jsp\?course_id=(?<id>[0-9]+).*?>|<a.*?coursehome/(?<idx>[0-9-]+).*?>)\s*(?<name>.*?)\((?<index>[0-9]+)\)\((?<term>[0-9]{4}-[0-9]{4}\S{4})\)</a>");

            var req = Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?typepage=2");

            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn";

            var s = await ReadToEnd(req);

            var objs = new List<Term>();
            var rawObjs = new Dictionary<string, List<WebLearnEntities.Lesson>>();
            var tasks = new List<Task>();

            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];

                List<WebLearnEntities.Lesson> lst;
                if (!rawObjs.TryGetValue(match.Groups["term"].Value, out lst))
                {
                    lst = new List<WebLearnEntities.Lesson>();
                    rawObjs.Add(match.Groups["term"].Value, lst);
                    objs.Add(new Term(match.Groups["term"].Value) { Lessons = lst });
                }

                if (string.IsNullOrEmpty(match.Groups["idx"].Value))
                {
                    var obj = new Lesson
                                  {
                                      CourseId = match.Groups["id"].Value,
                                      Name = match.Groups["name"].Value,
                                      Index = Convert.ToInt32(match.Groups["index"].Value)
                                  };
                    tasks.Add(GetBbsId(obj));
                    lst.Add(obj);
                }
                else
                    lst.Add(
                            new WebLearnNew.Lesson
                                {
                                    CourseId = match.Groups["idx"].Value,
                                    Name = match.Groups["name"].Value,
                                    Index = Convert.ToInt32(match.Groups["index"].Value)
                                });
            }

            await Task.WhenAll(tasks);

            return objs;
        }

        public async Task<List<Term>> FetchAllLessonList()
        {
            var p = FetchPreviousLessonList();
            var c = FetchCurrentLessonList();
            await Task.WhenAll(p, c);
            p.Result.Add(c.Result);
            return p.Result;
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
    }
}
