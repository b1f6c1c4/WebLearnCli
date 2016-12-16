using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using ManyConsole;
using WebLearnEntities;
using Facade = WebLearnCore.Facade;

namespace WebLearnCli
{
    internal class InitCommand : ConsoleCommand
    {
        public InitCommand()
        {
            IsCommand("init", "create weblearn folder");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                Facade.Init();
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
