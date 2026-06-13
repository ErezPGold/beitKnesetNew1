// HebrewNumber.cs — מספרים לאותיות עבריות (1..999)
namespace BeitKnessetDisplay
{
    public static class HebrewNumber
    {
        private static readonly string[] Units = { "", "א", "ב", "ג", "ד", "ה", "ו", "ז", "ח", "ט" };
        private static readonly string[] Tens  = { "", "י", "כ", "ל", "מ", "נ", "ס", "ע", "פ", "צ" };
        private static readonly string[] Hundreds = { "", "ק", "ר", "ש", "ת", "תק", "תר", "תש", "תת", "תתק" };

        public static string Format(int n)
        {
            if (n <= 0) return n.ToString();
            string r = "";
            int h = n / 100; n %= 100;
            r += Hundreds[System.Math.Min(h, 9)];
            if (n == 15) r += "טו";
            else if (n == 16) r += "טז";
            else { r += Tens[n / 10]; r += Units[n % 10]; }
            if (r.Length == 1) return r + "׳";
            return r.Insert(r.Length - 1, "״");
        }

        public static string Range(int from, int to) =>
            from == to ? Format(from) : $"{Format(from)}–{Format(to)}";
    }
}
