using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebLearnCli
{
    internal class CheckoutCommand : LessonCommandBase
    {
        public CheckoutCommand() : base("checkout", "download files") { HasAdditionalArguments(null, "lessons"); }

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            async Task<int> Checkout()
            {
                var facade = await Facade.Login(true);
                await Task.WhenAll(lessons.Select(l => facade.CheckoutLesson(l.Extension())));

                Facade.GenerateStatus();
                return 0;
            }

            return Checkout().Result;
        }
    }
}
