namespace WebLearnEntities
{
    public interface IFacade
    {
        void Login(WebLearnCredential cred);

        void FetchLesson(Lesson lesson);

        void SaveDocument(Document doc, string path);
    }
}
