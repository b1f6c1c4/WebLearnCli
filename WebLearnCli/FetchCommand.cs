using System;
using System.Security.Authentication;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal class FetchCommand : ConsoleCommand
    {
        private bool m_Previous;

        public FetchCommand()
        {
            IsCommand("fetch", "update information");
            HasOption("previous", "fetch old lessons.", t => m_Previous = t != null);
            HasAdditionalArguments(null, "lessons");
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                if (remainingArguments.Length == 0)
                    Facade.Fetch(m_Previous, Config.Inst.Lessons).Wait();
                else
                    Facade.Fetch(m_Previous, AbbrExpand.GetLessons(remainingArguments)).Wait();
                var cmd = new StatusCommand();
                cmd.Run(remainingArguments);
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
