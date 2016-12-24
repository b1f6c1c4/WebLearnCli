using System.Collections.Generic;
using WebLearnCore;

namespace WebLearnCli
{
    internal class CheckoutCommand : LessonCommandBase
    {
        public CheckoutCommand() : base("checkout", "download files") { HasAdditionalArguments(null, "lessons"); }

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            Facade.Checkout(lessons).Wait();
            return 0;
        }
    }
}
