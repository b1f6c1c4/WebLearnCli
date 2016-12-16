using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WebLearnEntities
{
    public abstract class CrawlerBase
    {
        private readonly CookieContainer m_Cookie = new CookieContainer();

        protected CrawlerBase() { ServicePointManager.DefaultConnectionLimit = 1000; }

        protected HttpWebRequest Head(string url)
        {
            var req = WebRequest.CreateHttp(url);
            if (req == null)
                throw new Exception();

            req.CookieContainer = m_Cookie;
            req.Method = "HEAD";

            return req;
        }

        protected HttpWebRequest Get(string url)
        {
            var req = WebRequest.CreateHttp(url);
            if (req == null)
                throw new Exception();

            req.CookieContainer = m_Cookie;
            req.Method = "GET";

            return req;
        }

        protected HttpWebRequest Post(string url, string data)
        {
            var buff = Encoding.UTF8.GetBytes(data);

            var req = WebRequest.CreateHttp(url);
            if (req == null)
                throw new Exception();

            req.CookieContainer = m_Cookie;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = buff.Length;
            req.GetRequestStream().Write(buff, 0, buff.Length);

            return req;
        }

        protected async Task<string> ReadToEnd(HttpWebRequest req)
        {
            var res = await req.GetResponseAsync();
            try
            {
                using (var stream = res.GetResponseStream())
                {
                    if (stream == null)
                        throw new NetworkInformationException();
                    using (var rd = new StreamReader(stream))
                        return rd.ReadToEnd();
                }
            }
            finally
            {
                res.Close();
                req.Abort();
            }
        }
    }
}
