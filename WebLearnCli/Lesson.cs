using System;
using System.Collections.Generic;

namespace WebLearnCli
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

        public static TermInfo Current
        {
            get
            {
                var term = new TermInfo { Year = DateTime.Now.Year };
                if (DateTime.Now.Month <= 1)
                {
                    term.Year--;
                    term.Index = 0;
                    return term;
                }
                if (DateTime.Now.Month <= 6)
                {
                    term.Year--;
                    term.Index = 1;
                    return term;
                }
                if (DateTime.Now.Month <= 8)
                {
                    term.Year--;
                    term.Index = 2;
                    return term;
                }
                term.Index = 0;
                return term;
            }
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
        // ReSharper disable NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Year.GetHashCode() ^ Index.GetHashCode();
        // ReSharper restore NonReadonlyMemberInGetHashCode
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
            $"{Term}/{Name.Replace(':', '-')}({Index})";
    }

    public abstract class Extension
    {
        public string Title { get; set; }
        public bool IsIgnored { get; set; }
    }

    public sealed class Announcement : Extension
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public string From { get; set; }
        public DateTime Date { get; set; }
    }

    public abstract class ExtensionWithFile : Extension
    {
        public double FileSize { get; set; }
        public string FileUrl { get; set; }
        public string FileName { get; set; }
        public DateTime Date { get; set; }
    }

    public sealed class Document : ExtensionWithFile
    {
        public string Id { get; set; }
        public string Abstract { get; set; }
    }

    public sealed class Assignment : ExtensionWithFile
    {
        public string Id { get; set; }
        public string RecId { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsSubmitted { get; set; }
        public string Content { get; set; }
        public string Score { get; set; }
        public string Assess { get; set; }
    }
}
