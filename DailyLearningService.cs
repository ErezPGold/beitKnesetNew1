// DailyLearningService.cs — חומשי לימוד יומי
using System;

namespace BeitKnesset.Services
{
    public class DailyLearningService
    {
        // —————————— תהילים יומי לפי ימי החודש ——————————
        // החלוקה הקלאסית של תהילים ל-30 ימי החודש (מנהג ישראל).
        // לחודש חסר (29 יום), ביום כ"ט קוראים גם את חלקו של יום ל'.
        private static readonly (int from, int to)[] DailyTehillim = new (int, int)[]
        {
            (1, 9),     // א
            (10, 17),   // ב
            (18, 22),   // ג
            (23, 28),   // ד
            (29, 34),   // ה
            (35, 38),   // ו
            (39, 43),   // ז
            (44, 48),   // ח
            (49, 54),   // ט
            (55, 59),   // י
            (60, 65),   // יא
            (66, 68),   // יב
            (69, 71),   // יג
            (72, 76),   // יד
            (77, 78),   // טו
            (79, 82),   // טז
            (83, 87),   // יז
            (88, 89),   // יח
            (90, 96),   // יט
            (97, 103),  // כ
            (104, 105), // כא
            (106, 107), // כב
            (108, 112), // כג
            (113, 118), // כד
            (119, 119), // כה (כל פרק קי"ט)
            (120, 134), // כו (שיר המעלות)
            (135, 139), // כז
            (140, 144), // כח
            (145, 150), // כט
            (1, 150),   // ל (לחודש מלא, או כפול בכ"ט אם חודש חסר)
        };

        public (int from, int to) GetTehillim(int dayOfHebrewMonth)
        {
            int idx = Math.Clamp(dayOfHebrewMonth, 1, 30) - 1;
            return DailyTehillim[idx];
        }

        // —————————— חומש יומי (עליה לפי יום בשבוע) ——————————
        // הצגה גנרית — שיעור פרשת השבוע עם עליית היום.
        public string GetChumashAliya(DateTime date)
        {
            string[] aliyot = { "ראשון", "שני", "שלישי", "רביעי", "חמישי", "שישי", "שביעי" };
            int dow = (int)date.DayOfWeek; // Sunday=0..Saturday=6
            return $"עליית {aliyot[dow]} של פרשת השבוע";
        }

        // —————————— דף יומי (בבלי) ——————————
        // מחזור י"ד התחיל ה' בטבת תשפ"ב = 7 בינואר 2020.
        // סך כל הדפים בש"ס = 2711.
        private static readonly (string name, int pages)[] Masechtot =
        {
            ("ברכות", 64), ("שבת", 157), ("עירובין", 105), ("פסחים", 121),
            ("שקלים", 22), ("יומא", 88), ("סוכה", 56), ("ביצה", 40),
            ("ראש השנה", 35), ("תענית", 31), ("מגילה", 32), ("מועד קטן", 29),
            ("חגיגה", 27), ("יבמות", 122), ("כתובות", 112), ("נדרים", 91),
            ("נזיר", 66), ("סוטה", 49), ("גיטין", 90), ("קידושין", 82),
            ("בבא קמא", 119), ("בבא מציעא", 119), ("בבא בתרא", 176), ("סנהדרין", 113),
            ("מכות", 24), ("שבועות", 49), ("עבודה זרה", 76), ("הוריות", 14),
            ("זבחים", 120), ("מנחות", 110), ("חולין", 142), ("בכורות", 61),
            ("ערכין", 34), ("תמורה", 34), ("כריתות", 28), ("מעילה", 22),
            ("קינים", 4), ("תמיד", 10), ("מדות", 4), ("נדה", 73),
        };

        public string GetDafYomi(DateTime date)
        {
            var cycleStart = new DateTime(2020, 1, 5); // מחזור י"ד
            int day = (date.Date - cycleStart).Days;
            if (day < 0) return "—";
            int total = 0;
            foreach (var (_, p) in Masechtot) total += (p - 1);
            int pos = day % total;
            foreach (var (name, pages) in Masechtot)
            {
                int dafs = pages - 1;
                if (pos < dafs)
                    return $"{name} דף {BeitKnessetDisplay.HebrewNumber.Format(pos + 2)}";
                pos -= dafs;
            }
            return "—";
        }

        // —————————— תניא יומי ——————————
        // הצגה גנרית לפי יום בשנה.
        public string GetTanya(DateTime date)
        {
            int doy = date.DayOfYear;
            return $"שיעור תניא — יום {BeitKnessetDisplay.HebrewNumber.Format(doy)}";
        }

        // —————————— היום יום ——————————
        public string GetHayomYom(DateTime date)
        {
            return $"היום יום — {date:dd/MM}";
        }
    }
}
