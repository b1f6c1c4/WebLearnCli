using System;

namespace WebLearnCli
{
    internal class InitCommand : CommandBase
    {
        public InitCommand() : base("init", "create weblearn folder") { }

        protected override int ConcreteRun(string[] remainingArguments)
        {
            Facade.Init();
            Console.Out.WriteLine("Initialized an empty weblearn folder.");
            return 0;
        }
    }
}
