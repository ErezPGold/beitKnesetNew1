// JewishService.cs — פרשה, הפטרה, מולד, שבת מברכים, רמב"ם
using System;
using System.Collections.Generic;
using System.Globalization;

namespace BeitKnessetDisplay
{
    public record MoladInfo(string Day, string Time, string Display);

    public class JewishService
    {
        private static readonly HebrewCalendar Hc = new();

        // —————————————— הפטרות (אשכנז) ——————————————
        private static readonly Dictionary<string, string> Haftarot = new()
        {
            ["בראשית"]="ישעיהו מ״ב, ה׳", ["נח"]="ישעיהו נ״ד, א׳",
            ["לך לך"]="ישעיהו מ׳, כ״ז", ["וירא"]="מלכים ב׳ ד׳, א׳",
            ["חיי שרה"]="מלכים א׳ א׳, א׳", ["תולדות"]="מלאכי א׳, א׳",
            ["ויצא"]="הושע י״ב, י״ג", ["וישלח"]="הושע י״א, ז׳",
            ["וישב"]="עמוס ב׳, ו׳", ["מקץ"]="מלכים א׳ ג׳, ט״ו",
            ["ויגש"]="יחזקאל ל״ז, ט״ו", ["ויחי"]="מלכים א׳ ב׳, א׳",
            ["שמות"]="ישעיהו כ״ז, ו׳", ["וארא"]="יחזקאל כ״ח, כ״ה",
            ["בא"]="ירמיהו מ״ו, י״ג", ["בשלח"]="שופטים ד׳, ד׳",
            ["יתרו"]="ישעיהו ו׳, א׳", ["משפטים"]="ירמיהו ל״ד, ח׳",
            ["תרומה"]="מלכים א׳ ה׳, כ״ו", ["תצוה"]="יחזקאל מ״ג, י׳",
            ["כי תשא"]="מלכים א׳ י״ח, א׳", ["ויקהל"]="מלכים א׳ ז׳, מ׳",
            ["פקודי"]="מלכים א׳ ז׳, נ״א", ["ויקהל–פקודי"]="מלכים א׳ ז׳, מ׳",
            ["ויקרא"]="ישעיהו מ״ג, כ״א", ["צו"]="ירמיהו ז׳, כ״א",
            ["שמיני"]="שמואל ב׳ ו׳, א׳", ["תזריע"]="מלכים ב׳ ד׳, מ״ב",
            ["מצורע"]="מלכים ב׳ ז׳, ג׳", ["תזריע–מצורע"]="מלכים ב׳ ז׳, ג׳",
            ["אחרי מות"]="יחזקאל כ״ב, א׳", ["קדושים"]="עמוס ט׳, ז׳",
            ["אחרי–קדושים"]="עמוס ט׳, ז׳",
            ["אמור"]="יחזקאל מ״ד, ט״ו", ["בהר"]="ירמיהו ל״ב, ו׳",
            ["בחוקותי"]="ירמיהו ט״ז, י״ט", ["בהר–בחוקותי"]="ירמיהו ט״ז, י״ט",
            ["במדבר"]="הושע ב׳, א׳", ["נשא"]="שופטים י״ג, ב׳",
            ["בהעלותך"]="זכריה ב׳, י״ד", ["שלח"]="יהושע ב׳, א׳",
            ["קרח"]="שמואל א׳ י״א, י״ד", ["חוקת"]="שופטים י״א, א׳",
            ["בלק"]="מיכה ה׳, ו׳", ["פינחס"]="מלכים א׳ י״ח, מ״ו",
            ["מטות"]="ירמיהו א׳, א׳", ["מסעי"]="ירמיהו ב׳, ד׳",
            ["מטות–מסעי"]="ירמיהו ב׳, ד׳",
            ["דברים"]="ישעיהו א׳, א׳", ["ואתחנן"]="ישעיהו מ׳, א׳",
            ["עקב"]="ישעיהו מ״ט, י״ד", ["ראה"]="ישעיהו נ״ד, י״א",
            ["שופטים"]="ישעיהו נ״א, י״ב", ["כי תצא"]="ישעיהו נ״ד, א׳",
            ["כי תבוא"]="ישעיהו ס׳, א׳", ["נצבים"]="ישעיהו ס״א, י׳",
            ["וילך"]="ישעיהו נ״ה, ו׳", ["נצבים–וילך"]="ישעיהו ס״א, י׳",
            ["האזינו"]="שמואל ב׳ כ״ב, א׳", ["וזאת הברכה"]="יהושע א׳, א׳",
        };

        public string GetHaftarah(string parasha) =>
            Haftarot.TryGetValue(parasha?.Trim() ?? "", out var h) ? h : "—";

        // —————————————— שבת מברכים + מולד ——————————————
        // האם השבת הקרובה היא שבת מברכים (השבת שלפני ר"ח, חוץ מתשרי)
        public bool IsShabbatMevarchim(DateTime today, out int monthNum, out string monthName)
        {
            int daysToShabbat = ((int)DayOfWeek.Saturday - (int)today.DayOfWeek + 7) % 7;
            DateTime shabbat = today.Date.AddDays(daysToShabbat);
            DateTime nextWeek = shabbat.AddDays(1);
            // ר"ח חודש הבא חייב להיות בשבוע שאחרי השבת
            for (int i = 0; i < 7; i++)
            {
                var d = nextWeek.AddDays(i);
                if (Hc.GetDayOfMonth(d) == 1)
                {
                    int m = Hc.GetMonth(d);
                    int y = Hc.GetYear(d);
                    bool leap = Hc.IsLeapYear(y);
                    // אין מברכין חודש תשרי
                    if (m == 1) { monthNum = 0; monthName = ""; return false; }
                    monthNum = m;
                    monthName = HebrewDateFormatter.Month(m, leap);
                    return true;
                }
            }
            monthNum = 0; monthName = ""; return false;
        }

        // חישוב מולד החודש העברי הנתון (שנה+חודש שלאחר ההגעה לר"ח)
        // מבוסס על מולד בהר"ד: יום 2 (שני), 5 שעות, 204 חלקים מתחילת היום (6PM ראשון).
        // חודש סינודי = 29 ימים, 12 שעות, 793 חלקים = 765,433 חלקים. שעה=1080 חלקים.
        public MoladInfo GetMolad(DateTime nearShabbat)
        {
            // מצא את החודש שאת מולדו אנו רוצים — החודש שמתחיל אחרי השבת
            int daysToShabbat = ((int)DayOfWeek.Saturday - (int)nearShabbat.DayOfWeek + 7) % 7;
            DateTime shabbat = nearShabbat.Date.AddDays(daysToShabbat);
            int targetMonth = 0, targetYear = 0;
            for (int i = 1; i <= 8; i++)
            {
                var d = shabbat.AddDays(i);
                if (Hc.GetDayOfMonth(d) == 1) { targetMonth = Hc.GetMonth(d); targetYear = Hc.GetYear(d); break; }
            }
            if (targetMonth == 0) return new MoladInfo("", "", "—");

            // ספור חודשים מבריאת העולם (שנה 1 תשרי) ועד החודש היעד.
            long monthsFromCreation = MonthsUntil(targetYear, targetMonth);

            // מולד בהר"ד: 2d 5h 204p מתחילת מנין (שעה = 1080 חלקים, יום = 25920 חלקים).
            // אנו נשתמש בחלקים סך-הכול מתחילת השבוע (יום ראשון 00:00 ליל אור).
            // לפי המסורת תחילת המנין: יום ראשון בשעה 0 (לפי לוח עולמי 6PM שבת = תחילת יום א').
            // נשמור פשוט: nMolad = 2*25920 + 5*1080 + 204 + monthsFromCreation * 765433
            const long DayParts = 25920;       // 24*1080
            const long MonthParts = 765433;    // 29*25920 + 12*1080 + 793
            long total = 2L * DayParts + 5L * 1080L + 204L + monthsFromCreation * MonthParts;

            long dayOfWeek = ((total / DayParts) % 7 + 7) % 7; // 0=א',...,6=שבת
            long partsInDay = total % DayParts;
            long hours = partsInDay / 1080;
            long parts = partsInDay % 1080;
            long minutes = parts / 18;       // 18 חלקים = דקה
            long chalakim = parts % 18;

            // שעות יחסיות: 0 = 6PM של הערב הקודם → המרה לשעון רגיל
            // לפי מנהג, מציגים: יום, שעה (אחרי 6 = בוקר/יום, לפני 6 = ערב), דקות, וחלקים
            string[] days = { "ראשון","שני","שלישי","רביעי","חמישי","שישי","שבת" };
            int civilHour = (int)((hours + 18) % 24); // 0 חלקי יום = 18:00 אזרחי
            string suffix = civilHour >= 12 ? "בערב" : "בבוקר";
            int displayHour = ((civilHour + 11) % 12) + 1;

            string day = "יום " + days[dayOfWeek];
            string time = $"{displayHour}:{minutes:D2} {suffix} ו-{chalakim} חלקים";
            return new MoladInfo(day, time, $"{day}, {time}");
        }

        private static long MonthsUntil(int hebrewYear, int hebrewMonth)
        {
            // ספירה מבריאה (שנה 1) באמצעות מחזור מטוני (19 שנים = 235 חודשים).
            // לא ניתן להשתמש ב-Hc.IsLeapYear עבור שנים < 5343, לכן נשתמש בנוסחה.
            // שנים מעוברות במחזור: 3, 6, 8, 11, 14, 17, 19
            int yearsBefore = hebrewYear - 1;
            int fullCycles = yearsBefore / 19;
            int remYears = yearsBefore % 19;
            long months = (long)fullCycles * 235L;
            for (int y = 1; y <= remYears; y++)
                if (IsHebrewLeapYear(y)) months += 13; else months += 12;
            months += (hebrewMonth - 1);
            return months;
        }

        private static bool IsHebrewLeapYear(int yearInCycle)
        {
            // yearInCycle: 1..19 (מיקום השנה במחזור 19 שנים)
            int m = ((yearInCycle - 1) % 19) + 1;
            return m == 3 || m == 6 || m == 8 || m == 11 || m == 14 || m == 17 || m == 19;
        }

        // —————————————— רמב"ם 3 פרקים ליום ——————————————
        // המחזור הנוכחי התחיל כ"ז ניסן תשמ"ד = 29 באפריל 1984. סך 1000 פרקים ÷ 3 ≈ 334 ימים (ויום אחרון של פרק אחד).
        public string GetRambam3(DateTime date)
        {
            var startCycle = new DateTime(1984, 4, 29);
            int day = (date.Date - startCycle).Days;
            if (day < 0) return "—";
            int dayInCycle = (day % 334) + 1; // יום במחזור
            // הצגה גנרית: מס' היום במחזור + הערה שמייצג 3 פרקים רצופים
            return $"יום {HebrewNumber.Format(dayInCycle)} במחזור (3 פרקים)";
        }
    }
}
