﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace WebLearnCore
{
    public sealed class Status
    {
        public List<LessonStatus> Lessons;

        public List<DeadLine> DeadLines;
    }

    public sealed class LessonStatus
    {
        public string Name { get; set; }

        public bool HasNewAnnouncement { get; set; }
        public bool HasNewDocument { get; set; }
        public bool HasDeadLine { get; set; }
    }

    public sealed class DeadLine
    {
        public string Name { get; set; }

        public string Title { get; set; }
        public DateTime DueDate { get; set; }
    }

    public static class StatusManager
    {
        public static Status Status { get; set; }

        public static void Load() =>
            Status = JsonConvert.DeserializeObject<Status>(File.ReadAllText(DbHelper.GetPath("status.json")));

        internal static void Save() =>
            File.WriteAllText(DbHelper.GetPath("status.json"), JsonConvert.SerializeObject(Status, Formatting.Indented));
    }
}