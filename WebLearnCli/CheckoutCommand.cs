using System;
using System.Security.Authentication;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal class CheckoutCommand : ConsoleCommand
    {
        public CheckoutCommand()
        {
            IsCommand("checkout", "download files");
            HasAdditionalArguments(null, "lessons");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (remainingArguments.Length == 0)
                    Facade.Checkout(Config.Inst.Lessons).Wait();
                else
                    Facade.Checkout(AbbrExpand.GetLessons(remainingArguments)).Wait();
                return 0;
            }
            catch (AuthenticationException)
            {
                Console.Error.WriteLine("Invalid credential.");
                return 1;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Unknown error: {e}");
                return 1;
            }
        }
    }
}
