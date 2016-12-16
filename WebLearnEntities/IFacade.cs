using System.Threading.Tasks;

namespace WebLearnEntities
{
    public interface IFacade
    {
        Task FetchLesson(Lesson lesson);

        Task SaveDocument(Document doc, string path);
    }
}
