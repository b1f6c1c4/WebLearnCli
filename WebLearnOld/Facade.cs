using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebLearnEntities;

namespace WebLearnOld
{
    public class Facade : CrawlerBase, IFacade
    {
        public void Login(WebLearnCredential cred)
        {
            var req = Post(
                           "https://learn.tsinghua.edu.cn/MultiLanguage/lesson/teacher/loginteacher.jsp",
                           $"userid={cred.Username}&userpass={cred.Password}&submit1=%E7%99%BB%E5%BD%95");

            req.Accept = "text/html, application/xhtml+xml, */*";
            req.Referer = "http://learn.tsinghua.edu.cn/index.jsp";

            var res = req.GetResponse();
            try
            {
                using (var stream = res.GetResponseStream())
                {
                    if (stream == null)
                        throw new NetworkInformationException();
                    using (var rd = new StreamReader(stream))
                    {
                        var s = rd.ReadToEnd();
                        if (s.IndexOf("用户名或密码错误，登录失败！", StringComparison.Ordinal) >= 0)
                            throw new AuthenticationException();
                    }
                }
            }
            finally
            {
                res.Close();
                req.Abort();
            }
        }

        public async Task<Term> FetchCurrentLessonList()
        {
            var regex =
                new Regex(
                    @"<a.*?course_locate.jsp\?course_id=(?<id>[0-9]+).*?>\s*(?<name>.*?)\s*</a>[\s\S]*?span.*?>(?<homework>[0-9]+)</span>[\s\S]*?<span.*?>(?<note>[0-9]+)</span>[\s\S]*?<span.*?>(?<file>[0-9]+)</span>.*?</td>");

            var req = Get("http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/MyCourse.jsp?language=cn");

            req.Referer = "http://learn.tsinghua.edu.cn/MultiLanguage/lesson/student/mainstudent.jsp";

            string s;
            var res = await req.GetResponseAsync();
            try
            {
                using (var stream = res.GetResponseStream())
                {
                    if (stream == null)
                        throw new NetworkInformationException();
                    using (var rd = new StreamReader(stream))
                        s = rd.ReadToEnd();
                }
            }
            finally
            {
                res.Close();
                req.Abort();
            }

            var objs = new List<Lesson>();

            var matchCollection = regex.Matches(s);
            for (var i = 0; i < matchCollection.Count; i++)
            {
                var match = matchCollection[i];
                var obj = new Lesson();
                obj.CourseId = match.Groups["id"].Value;
                obj.Name = match.Groups["name"].Value;
                objs.Add(obj);
            }

            var tasks = objs.Select(GetBbsId);
            await Task.WhenAll(tasks);

            return
                new Term
                    {
                        Lessons = objs
                    };
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

        public async Task<List<Term>> FetchAllLessonList() { throw new NotImplementedException(); }

        public async Task FetchLesson(WebLearnEntities.Lesson lesson)
        {
            var l = lesson as Lesson;
            Debug.Assert(l != null);

            throw new NotImplementedException();
        }

        public async Task SaveDocument(Document doc, string path) { throw new NotImplementedException(); }
    }
}
