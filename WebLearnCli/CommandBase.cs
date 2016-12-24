﻿using System;
using System.Collections.Generic;
using System.Security.Authentication;
using ManyConsole;
using WebLearnCore;

namespace WebLearnCli
{
    internal abstract class CommandBase : ConsoleCommand
    {
        protected CommandBase(string command, string oneLineDescription = "")
        {
            IsCommand(command, oneLineDescription);
        }

        public override int Run(string[] remainingArguments)
        {
            try
            {
                return ConcreteRun(remainingArguments);
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

        protected abstract int ConcreteRun(string[] remainingArguments);
    }

    internal abstract class SelectionCommandBase : CommandBase
    {
        protected bool Previous;
        private bool m_NoCurrent;

        protected SelectionCommandBase(string command, string oneLineDescription = "")
            : base(command, oneLineDescription)
        {
            HasOption("previous", "include old lessons.", t => Previous = t != null);
            HasOption("no-current", "exclude current lessons.", t => m_NoCurrent = t != null);
            HasAdditionalArguments(null, "lessons");
        }

        protected override sealed int ConcreteRun(string[] remainingArguments) =>
            ConcreteRun(
                        remainingArguments.Length == 0
                            ? AbbrExpand.GetLessons(Previous, m_NoCurrent)
                            : AbbrExpand.GetLessons(remainingArguments, Previous, m_NoCurrent));

        protected abstract int ConcreteRun(IEnumerable<Lesson> lessons);
    }
}
