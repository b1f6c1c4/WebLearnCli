using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Security.Authentication;
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

        public void FetchLesson(Lesson lesson) { throw new NotImplementedException(); }
        public void SaveDocument(Document doc, string path) { throw new NotImplementedException(); }
    }
}
