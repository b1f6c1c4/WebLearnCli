using System;
using ManyConsole;

namespace WebLearnCli
{
    internal class CredentialCommand : ConsoleCommand
    {
        public CredentialCommand() { IsCommand("credential", "management of WebLearn credentials"); }

        public override int Run(string[] remainingArguments) { throw new NotImplementedException(); }
    }
}
