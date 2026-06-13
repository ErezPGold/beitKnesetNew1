// HebrewDateFormatter.cs
using System.Globalization;

namespace BeitKnessetDisplay
{
    public static class HebrewDateFormatter
    {
        private static readonly string[] Units = { "", "א", "ב", "ג", "ד", "ה", "ו", "ז", "ח", "ט" };
        private static readonly string[] Tens = { "", "י", "כ", "ל", "מ", "נ", "ס", "ע", "פ", "צ" };

        public static string Day(int n)
        {
            if (n == 15) return "ט״ו";
            if (n == 16) return "ט״ז";
            int t = n / 10, u = n % 10;
            string r = "";
            if (t > 0) r += Tens[t];
            if (u > 0) r += Units[u];
            return r.Length == 1 ? r + "׳" : r.Insert(r.Length - 1, "״");
        }

        public static string Month(int month, bool leap)
        {
            // ב-HebrewCalendar של .NET: בשנה מעוברת חודש 6 = אדר א׳, 7 = אדר ב׳, וניסן = 8 וכו'
            string[] regular = { "", "תשרי","חשוון","כסלו","טבת","שבט","אדר",
                                 "ניסן","אייר","סיון","תמוז","אב","אלול" };
            string[] leapY = { "", "תשרי","חשוון","כסלו","טבת","שבט","אדר א׳","אדר ב׳",
                                 "ניסן","אייר","סיון","תמוז","אב","אלול" };
            return (leap ? leapY : regular)[month];
        }

        public static string Year(int year)
        {
            int n = year % 1000;                 // 786 → תשפ״ו
            string[] hundreds = { "", "ק", "ר", "ש", "ת", "תק", "תר", "תש", "תת", "תתק" };
            int h = n / 100, rest = n % 100;
            int t = rest / 10, u = rest % 10;

            // למנוע צירופים אסורים (טו/טז)
            if (rest == 15) { t = 0; u = 0; }
            string letters = hundreds[h];
            if (rest == 15) letters += "טו";
            else if (rest == 16) letters += "טז";
            else
            {
                if (t > 0) letters += Tens[t];
                if (u > 0) letters += Units[u];
            }
            return letters.Length == 1
                ? letters + "׳"
                : letters.Insert(letters.Length - 1, "״");
        }

    }
}
