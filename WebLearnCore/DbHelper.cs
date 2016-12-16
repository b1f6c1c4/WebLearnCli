using System.IO;

namespace WebLearnCore
{
    internal static class DbHelper
    {
        public static string GetPath(string val) =>
            Path.Combine(".weblearn/", val);
    }
}
