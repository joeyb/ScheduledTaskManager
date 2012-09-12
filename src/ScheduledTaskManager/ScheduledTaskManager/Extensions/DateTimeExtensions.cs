using System;

namespace ScheduledTaskManager.Extensions
{
    internal static class DateTimeExtensions
    {
        /// <summary>
        /// Extension to get DateTime at the exact minute, dropping seconds and milliseconds
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime WithoutSeconds(this DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, 0, 0);
        }
    }
}
