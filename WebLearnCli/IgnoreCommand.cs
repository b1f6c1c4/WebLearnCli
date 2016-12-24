using System;

namespace WebLearnCli
{
    internal class IgnoreCommand : SelectionCommandBase
    {
        public IgnoreCommand() : base("ignore", "ignore lessson or something; mark as read")
        {
            HasAdditionalArguments(null, "paths");
            HasLongDescription(@"Path syntax:
*           any lesson
<L>         one lesson
<L>/*       everything of the lesson
<L>/a       every announcement of the lesson
<L>/f       every file of the lesson
<L>/d       every deadline of the lesson
"); // TODO
        }

        protected override int ConcreteRun(string[] remainingArguments) { throw new NotImplementedException(); }
    }
}
