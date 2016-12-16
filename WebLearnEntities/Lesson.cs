using System;
using System.Collections.Generic;

namespace WebLearnEntities
{
    public struct TermInfo: IComparable<TermInfo>
    {
        public readonly int Year;
        public readonly int Index;

        public TermInfo(int year, int index)
        {
            Year = year;
            Index = index;
        }

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

            return new TermInfo(year, index);
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

    public abstract class Lesson
    {
        public string Name { get; set; }
        public int Index { get; set; }

        public IReadOnlyList<Announcement> Announcements { get; set; }
        public IReadOnlyList<Document> Documents { get; set; }
        public IReadOnlyList<Assignment> Assignments { get; set; }
    }

    public class Announcement
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string From { get; set; }

        public DateTime? Date { get; set; }
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
