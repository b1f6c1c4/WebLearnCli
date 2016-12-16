using System;
using System.Collections.Generic;

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
}
