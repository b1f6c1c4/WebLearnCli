using System;
using System.Collections.Generic;
using System.IO;

namespace WebLearnCore
{
    public class Facade
    {
        public void Init()
        {
            if (Directory.Exists(DbHelper.GetPath("")))
                throw new ApplicationException("WebLearn folder already exists.");

            Directory.CreateDirectory(DbHelper.GetPath(""));
            Directory.CreateDirectory(DbHelper.GetPath("lessons/"));
            ConfigManager.Config =
                new Config
                    {
                        LessonSettings = new List<LessonSetting>()
                    };
            ConfigManager.Save();
        }
    }
}
