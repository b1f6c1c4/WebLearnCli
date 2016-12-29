using System;
using System.Collections.Generic;

namespace WebLearnCore
{
    public sealed class Status
    {
        private static Status m_Inst;

        public static Status Inst
        {
            get
            {
                if (m_Inst == null)
                    Load();
                return m_Inst;
            }
            set { m_Inst = value; }
        }

        public static void Load() => Inst = "status.json".InDb().LoadJson<Status>();

        internal static void Save() => "status.json".InDb().SaveJson(Inst);

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
