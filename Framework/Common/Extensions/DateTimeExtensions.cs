using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Extensions
{
    public static class DateTimeExtensions
    {
        private static readonly PersianCalendar pc = new PersianCalendar();

        private static readonly string[] WeekDays = new[]
        {
            "یکشنبه",
            "دوشنبه",
            "سه‌شنبه",
            "چهارشنبه",
            "پنجشنبه",
            "جمعه",
            "شنبه"
        };

        private static readonly string[] Months = new[]
        {
            "", // index 0 خالی
            "فروردین",
            "اردیبهشت",
            "خرداد",
            "تیر",
            "مرداد",
            "شهریور",
            "مهر",
            "آبان",
            "آذر",
            "دی",
            "بهمن",
            "اسفند"
        };

        // 📅 فقط تاریخ (مثل: دوشنبه 25 خرداد 1405)
        public static string ToPersianDate(this DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);

            string dayName = WeekDays[(int)date.DayOfWeek];

            return $"{dayName} {day} {Months[month]} {year}";
        }

        // ⏰ تاریخ + زمان (مثل: دوشنبه 25 خرداد 1405 - 12:45:22)
        public static string ToPersianDateTime(this DateTime date)
        {
            int year = pc.GetYear(date);
            int month = pc.GetMonth(date);
            int day = pc.GetDayOfMonth(date);

            string dayName = WeekDays[(int)date.DayOfWeek];

            string time = date.ToString("HH:mm:ss");

            return $"{dayName} {day} {Months[month]} {year} - {time}";
        }
    }
}
