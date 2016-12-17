using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebLearnCore.Crawler
{
    public sealed class CrawlerFacade
    {
        private readonly CrawlerOld m_Old;
        private readonly CrawlerNew m_New;

        private bool m_Roaming;

        public CrawlerFacade()
        {
            m_Old = new CrawlerOld();
            m_New = new CrawlerNew();

            m_Roaming = false;
        }

        private async Task Roaming()
        {
            if (!m_Roaming)
            {
                var ticket = await m_Old.FetchRoamingTicket();
                await m_New.Login(ticket);
                m_Roaming = true;
            }
        }

        public async Task Login(WebLearnCredential cred, bool roaming = false)
        {
            await m_Old.Login(cred);
            if (roaming)
                await Roaming();
        }

        public async Task<Term> FetchCurrentLessonList()
        {
            await Roaming();
            return await m_Old.FetchCurrentLessonList();
        }

        public async Task<List<Term>> FetchAllLessonList()
        {
            await Roaming();
            var c = await m_Old.FetchCurrentLessonList();
            var p = await m_Old.FetchPreviousLessonList();
            p.Add(c);
            return p;
        }

        public async Task<LessonExtension> FetchLesson(Lesson lesson)
        {
            ILessonExtensionCrawler crawler;
            if (lesson.Version)
                crawler = m_New;
            else
                crawler = m_Old;

            var ann = crawler.GetAnnouncements(lesson);
            var doc = crawler.GetDocuments(lesson);
            var ass = crawler.GetAssignments(lesson);

            await Task.WhenAll(ann, doc, ass);

            return
                new LessonExtension
                    {
                        Announcements = ann.Result,
                        Documents = doc.Result,
                        Assignments = ass.Result
                    };
        }

        public async Task CheckoutLesson(Lesson lesson, LessonExtension ext)
        {
            ILessonExtensionCrawler crawler;
            if (lesson.Version)
                crawler = m_New;
            else
                crawler = m_Old;

            var docs = ext.Documents.Select(o => crawler.DownloadDocument(lesson, o));
            var asss = ext.Assignments.Select(o => crawler.DownloadAssignment(lesson, o));

            await Task.WhenAll(docs.Union(asss));
        }
    }
}
