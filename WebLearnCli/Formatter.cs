using System;
using WebLearnCore;

namespace WebLearnCli
{
    internal static class Formatter
    {
        public static string Format(DeadLine ddl)
        {
            var remain = ddl.DueDate.AddDays(1).Subtract(DateTime.Now);
            if (remain.TotalMilliseconds <= 0)
                return $"{ddl.DueDate:yyyyMMdd} === ALREADY DUE === {ddl.Name}/{ddl.Title}";
            if (remain.Days >= 2)
                return $"{ddl.DueDate:yyyyMMdd} {remain.Days} days left {ddl.Name}/{ddl.Title}";
            if (remain.Hours >= 2)
                return $"{ddl.DueDate:yyyyMMdd} {remain.Hours} hours left {ddl.Name}/{ddl.Title}";
            if (remain.Minutes >= 2)
                return $"{ddl.DueDate:yyyyMMdd} {remain.Minutes} mins left {ddl.Name}/{ddl.Title}";
            return $"{ddl.DueDate:yyyyMMdd} {remain.Seconds} secs left {ddl.Name}/{ddl.Title}";
        }
    }
}
