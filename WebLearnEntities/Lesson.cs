using System;
using System.Collections.Generic;

namespace WebLearnEntities
{
    public abstract class Lesson
    {
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string CourseId { get; set; }
        public string BbsId { get; set; }

        public List<Announcement> Announcements { get; set; }
        public List<Document> Documents { get; set; }
        public List<Assignment> Assignments { get; set; }
    }

    public abstract class Announcement
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string From { get; set; }

        public DateTime? Date { get; set; }
        public bool IsRead { get; set; }
        public bool IsStarred { get; set; }
        public string BbsType { get; set; }
        public string Id { get; set; }
    }

    public abstract class Document
    {
        public string Title { get; set; }
        public string Abstract { get; set; }
        public double Size { get; set; }
        public DateTime? Date { get; set; }
        public bool IsRead { get; set; }

        public string Path { get; set; }
    }

    public abstract class Assignment
    {
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public double Size { get; set; }

        public string Id { get; set; }
        public string Score { get; set; }
    }
}
