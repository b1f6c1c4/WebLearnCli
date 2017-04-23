using System;
using System.Collections.Generic;
using System.IO;

namespace WebLearnCli
{
    internal class InitCommand : CommandBase
    {
        public InitCommand() : base("init", "create weblearn folder") { }

        protected override int ConcreteRun(string[] remainingArguments)
        {
            if (Directory.Exists("".InDb()))
                throw new ApplicationException("WebLearn folder already exists.");

            Directory.CreateDirectory("".InDb());
            Directory.CreateDirectory("lessons/".InDb());
            Config.Inst =
                new Config
                    {
                        Lessons = new List<Lesson>()
                    };
            Config.Save();

            Console.Out.WriteLine("Initialized an empty weblearn folder.");
            return 0;
        }
    }
}
