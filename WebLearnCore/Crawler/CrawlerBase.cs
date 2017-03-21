using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace WebLearnCore.Crawler
{
    internal interface ILessonExtensionCrawler
    {
        Task<List<Announcement>> GetAnnouncements(Lesson obj);
        Task<List<Document>> GetDocuments(Lesson obj);
        Task<List<Assignment>> GetAssignments(Lesson obj);

        Task DownloadFile(Lesson lesson, ExtensionWithFile obj);
        Task SubmitAssignment(Lesson lesson, Assignment obj);
    }

    internal abstract class CrawlerBase
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

        private static string RandomString(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public HttpWebRequest MultipartFormDataPost(string url, Dictionary<string, object> postParameters)
        {
            var formDataBoundary = $"----WebKitFormBoundary{RandomString(16)}";

            var buff = GetMultipartFormData(postParameters, formDataBoundary, Encoding.UTF8);

            var req = WebRequest.CreateHttp(url);
            if (req == null)
                throw new Exception();

            req.CookieContainer = m_Cookie;
            req.Method = "POST";
            req.ContentType = $"multipart/form-data; boundary={formDataBoundary}";
            req.ContentLength = buff.Length;
            req.GetRequestStream().Write(buff, 0, buff.Length);

            return req;
        }

        protected static async Task<string> ReadToEnd(WebRequest req)
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

        /// <summary>
        ///     Check if it's neccessary to download it.
        /// </summary>
        /// <returns><c>true</c> to download</returns>
        protected static bool Check(Lesson lesson, ExtensionWithFile obj)
        {
            if (obj.IsIgnored)
                return true;

            if (obj.FileUrl == null)
                return true;

            var file = Path.Combine(lesson.Path, obj.FileName);
            if (!File.Exists(file))
                return false;

            if (File.GetLastAccessTime(file) != obj.Date)
                return false;

            return true;
        }

        private static async Task DownloadToFile(WebRequest req, string file)
        {
            var path = Path.GetDirectoryName(file);
            if (path != null)
                Directory.CreateDirectory(path);

            var res = await req.GetResponseAsync();
            using (var stream = File.OpenWrite(file))
            using (var ress = res.GetResponseStream())
                ress?.CopyTo(stream);
        }

        protected static async Task DownloadFile(Lesson lesson, ExtensionWithFile obj, WebRequest req)
        {
            var file = Path.Combine(lesson.Path, obj.FileName);
            await DownloadToFile(req, file);
            File.SetCreationTime(file, obj.Date);
            File.SetLastWriteTime(file, obj.Date);
            File.SetLastAccessTime(file, obj.Date);
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary,
                                                   Encoding encoding)
        {
            var formDataStream = new MemoryStream();
            var needsCrLf = false;

            foreach (var param in postParameters)
            {
                if (needsCrLf)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));
                needsCrLf = true;

                var value = param.Value as FileParameter;
                if (value != null)
                {
                    var fileToUpload = value;

                    var header =
                        $"--{boundary}\r\nContent-Disposition: form-data; name=\"{param.Key}\"; filename=\"{fileToUpload.FileName ?? param.Key}\"\r\nContent-Type: {fileToUpload.ContentType ?? "application/octet-stream"}\r\n\r\n";

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    var postData =
                        $"--{boundary}\r\nContent-Disposition: form-data; name=\"{param.Key}\"\r\n\r\n{param.Value}";
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            var footer = $"\r\n--{boundary}--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            formDataStream.Position = 0;
            var formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }

        protected sealed class FileParameter
        {
            public byte[] File { get; set; }
            public string FileName { get; set; }
            public string ContentType { get; set; }
        }
    }
}
