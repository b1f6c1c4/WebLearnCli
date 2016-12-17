using System;
using System.Collections.Generic;

namespace WebLearnCore
{
    public struct TermInfo : IComparable<TermInfo>
    {
        public int Year;
        public int Index;

        public static implicit operator TermInfo(string value)
        {
            if (value.Length != 13)
                throw new FormatException();

            int year;
            int index;

            int.TryParse(value.Substring(0, 4), out year);

            switch (value.Substring(9, 1))
            {
                case "秋":
                    index = 0;
                    break;
                case "春":
                    index = 1;
                    break;
                case "夏":
                    index = 2;
                    break;
                default:
                    throw new FormatException();
            }

            return new TermInfo { Year = year, Index = index };
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

        public override bool Equals(object obj) => obj is TermInfo && this == (TermInfo)obj;
        public override int GetHashCode() => Year.GetHashCode() ^ Index.GetHashCode();
        public static bool operator ==(TermInfo x, TermInfo y) => x.Year == y.Year && x.Index == y.Index;
        public static bool operator !=(TermInfo x, TermInfo y) => !(x == y);

        public int CompareTo(TermInfo other)
        {
            if (Year < other.Year)
                return -1;
            if (Year > other.Year)
                return 1;
            if (Index < other.Index)
                return -1;
            if (Index > other.Index)
                return 1;
            return 0;
        }

        public static bool operator <(TermInfo e1, TermInfo e2) => e1.CompareTo(e2) < 0;

        public static bool operator >(TermInfo e1, TermInfo e2) => e1.CompareTo(e2) > 0;
    }

    public sealed class Term
    {
        public TermInfo Info { get; set; }

        public IReadOnlyList<Lesson> Lessons;
    }

    public sealed class Lesson
    {
        public TermInfo Term { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }

        public bool Version { get; set; }

        public bool Ignore { get; set; }
        public string Path { get; set; }
        public List<string> Alias { get; set; }

        public string CourseId { get; set; }
        public string BbsId { get; set; }

        public override string ToString() =>
            $"{Term}/{Name}({Index})";
    }

    public sealed class LessonExtension
    {
        public List<Announcement> Announcements { get; set; }
        public List<Document> Documents { get; set; }
        public List<Assignment> Assignments { get; set; }
    }

    public sealed class Announcement
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string From { get; set; }
        public DateTime Date { get; set; }
    }

    public sealed class Document
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Abstract { get; set; }
        public double Size { get; set; }
        public DateTime Date { get; set; }
        public bool IsRead { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
    }

    public sealed class Assignment
    {
        public string Id { get; set; }
        public string RecId { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public double Size { get; set; }
        public string Content { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public string Score { get; set; }
        public string Assess { get; set; }
    }
}
