using System;
using System.Collections.Generic;

namespace WebLearnEntities
{
    public sealed class Term
    {
        public int Year { get; set; }
        public int Index { get; set; }

        public IReadOnlyList<Lesson> Lessons;

        public Term(string value)
        {
            if (value.Length != 13)
                throw new FormatException();

            int year;
            int id;

            int.TryParse(value.Substring(0, 4), out year);

            switch (value.Substring(9, 1))
            {
                case "秋":
                    id = 0;
                    break;
                case "春":
                    id = 1;
                    break;
                case "夏":
                    id = 2;
                    break;
                default:
                    throw new FormatException();
            }

            Year = year;
            Index = id;
        }

        public override string ToString()
        {
            switch (Index)
            {
                case 0:
                    return $"{Year}-{Year + 1}秋季学期";
                case 1:
                    return $"{Year}-{Year + 1}春季学期";
                case 2:
                    return $"{Year}-{Year + 1}夏季学期";
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public abstract class Lesson
    {
        public string Name { get; set; }

        public IReadOnlyList<Announcement> Announcements { get; set; }
        public IReadOnlyList<Document> Documents { get; set; }
        public IReadOnlyList<Assignment> Assignments { get; set; }
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
