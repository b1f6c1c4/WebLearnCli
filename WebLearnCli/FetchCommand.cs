using System.Collections.Generic;
using WebLearnCore;

namespace WebLearnCli
{
    internal class FetchCommand : LessonCommandBase
    {
        public FetchCommand() : base("fetch", "update information") { HasAdditionalArguments(null, "lessons"); }

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            Facade.Fetch(Previous, lessons).Wait();
            return 0;
        }
    }
}
