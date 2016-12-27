using System;
using WebLearnCore;

namespace WebLearnCli
{
    internal static class Formatter
    {
        public static string Format(DeadLine ddl)
        {
            var remain = ddl.DueDate.Subtract(DateTime.Now);
            if (remain.TotalMilliseconds <= 0)
                return $"{ddl.DueDate:yyyyMMdd} === ALREADY DUE === {ddl.Name}/{ddl.Title}";
            if (remain.TotalDays >= 5)
                return $"{ddl.DueDate:yyyyMMdd} {remain.Days,13} days  {ddl.Name}/{ddl.Title}";
            if (remain.TotalDays >= 2)
                return $"{ddl.DueDate:yyyyMMdd} + {remain.Days,11} days  {ddl.Name}/{ddl.Title}";
            if (remain.TotalHours >= 24)
                return $"{ddl.DueDate:yyyyMMdd} +++ {remain.Hours,9} hours {ddl.Name}/{ddl.Title}";
            if (remain.TotalHours >= 2)
                return $"{ddl.DueDate:yyyyMMdd} ++++ {remain.Hours,8} hours {ddl.Name}/{ddl.Title}";
            if (remain.Minutes >= 2)
                return $"{ddl.DueDate:yyyyMMdd} +++++++ {remain.Minutes,5} mins  {ddl.Name}/{ddl.Title}";
            return $"{ddl.DueDate:yyyyMMdd} ++++++++ {remain.Seconds,4} secs  {ddl.Name}/{ddl.Title}";
        }
    }
}
