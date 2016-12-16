using System;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal class CredentialCommand : ConsoleCommand
    {
        private bool m_Force;
        private bool m_Drop;

        public CredentialCommand()
        {
            IsCommand("credential", "management of WebLearn credentials");
            HasOption("d|drop", "drop the stored credential if exists.", t => m_Drop = t != null);
            HasOption("f|force", "override existing credential.", t => m_Force = t != null);
        }

        public override int Run(string[] remainingArguments)
        {
            if (m_Drop)
            {
                if (CredentialManager.DropCredential())
                {
                    Console.Out.WriteLine("Sucessfully dropped the stored credential.");
                    return 0;
                }

                Console.Out.WriteLine("Stored credential not found.");
                return 1;
            }

            var cred = CredentialManager.TryGetCredential(m_Force);
            if (cred != null)
            {
                Console.Out.WriteLine($"Credential stored: Username {cred.Username}, Password: ********");
                return 0;
            }

            Console.Out.WriteLine("Operation cancelled.");
            return 1;
        }
    }
}
