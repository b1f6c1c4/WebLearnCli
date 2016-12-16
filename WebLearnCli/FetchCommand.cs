using System;
using System.Security.Authentication;
using ManyConsole;
using WebLearnEntities;
using WebLearnOld;

namespace WebLearnCli
{
    internal class FetchCommand : ConsoleCommand
    {
        public FetchCommand() { IsCommand("fetch", "update information"); }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                var facade = new Facade();
                facade.Login(CredentialManager.TryGetCredential());
                return 0;
            }
            catch (AuthenticationException)
            {
                Console.Error.WriteLine("Invalid credential.");
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
