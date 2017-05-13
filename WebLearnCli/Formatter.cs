using System;

namespace WebLearnCli
{
    internal static class Formatter
    {
        public static string Format(DeadLine ddl)
        {
            var remain = ddl.DueDate.Subtract(DateTime.Now);
            if (remain.TotalMilliseconds <= 0)
                return $"{ddl.DueDate:yyyyMMdd} === ALREADY DUE === {ddl.Name}/d/{ddl.Title}";
            if (remain.TotalDays >= 5)
                return $"{ddl.DueDate:yyyyMMdd} {remain.Days,13} days  {ddl.Name}/d/{ddl.Title}";
            if (remain.TotalDays >= 2)
                return $"{ddl.DueDate:yyyyMMdd} + {remain.Days,11} days  {ddl.Name}/d/{ddl.Title}";
            if (remain.TotalHours >= 24)
                return $"{ddl.DueDate:yyyyMMdd} +++ {(int)remain.TotalHours,9} hours {ddl.Name}/d/{ddl.Title}";
            if (remain.TotalHours >= 2)
                return $"{ddl.DueDate:yyyyMMdd} ++++ {(int)remain.TotalHours,8} hours {ddl.Name}/d/{ddl.Title}";
            if (remain.TotalMinutes >= 2)
                return $"{ddl.DueDate:yyyyMMdd} +++++++ {(int)remain.TotalMinutes,5} mins  {ddl.Name}/d/{ddl.Title}";
            return $"{ddl.DueDate:yyyyMMdd} ++++++++ {(int)remain.TotalSeconds,4} secs  {ddl.Name}/d/{ddl.Title}";
        }
    }
}
