using System;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal class InitCommand : ConsoleCommand
    {
        public InitCommand() { IsCommand("init", "create weblearn folder"); }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Facade.Init();
                Console.Out.WriteLine("Initialized an empty weblearn folder.");
                return 0;
            }
            catch (ApplicationException e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unknown error: {e.Message}");
                return 1;
            }
        }
    }
}
