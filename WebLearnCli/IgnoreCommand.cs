using System.Collections.Generic;
using WebLearnCore;

namespace WebLearnCli
{
    internal class IgnoreCommand : PathExtCommandBase
    {
        private bool m_Ignore = true;

        public IgnoreCommand() : base("ignore", "ignore lessson or something; mark as read")
        {
            HasOption("u|undo", "un-ignore them", t => m_Ignore = t == null);
        }

        protected override int ConcreteRun(IEnumerable<Lesson> lessons)
        {
            foreach (var lesson in lessons)
                lesson.Ignore = m_Ignore;
            return 0;
        }

        protected override int ConcreteRun(Lesson lesson, IEnumerable<Extension> objs)
        {
            foreach (var obj in objs)
                obj.IsIgnored = m_Ignore;
            return 0;
        }
    }
}
