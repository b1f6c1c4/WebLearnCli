using System;

namespace WebLearnCli
{
    internal class IgnoreCommand : SelectionCommandBase
    {
        public IgnoreCommand() : base("ignore", "ignore lessson or something; mark as read") { }

        protected override int ConcreteRun(string[] remainingArguments) { throw new NotImplementedException(); }
    }
}
