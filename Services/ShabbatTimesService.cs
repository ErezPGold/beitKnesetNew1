using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BeitKnessetDisplay.Services
{
    public static class ShabbatTimesService
    {
        private static readonly HttpClient _http = new();

        public record ShabbatTimes(string CandleLighting, string Havdalah, string ParashaHebrew);

        // geonameid: ירושלים=281184, תל אביב=293397, חיפה=294801, באר שבע=295530
        public static async Task<ShabbatTimes> GetAsync(int geonameId = 293397, int havdalahMin = 50)
        {
            var url = $"https://www.hebcal.com/shabbat?cfg=json&geonameid={geonameId}&M=on&b=18&m={havdalahMin}&lg=h";
            var json = await _http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            string candle = "", havdalah = "", parasha = "";
            foreach (var item in doc.RootElement.GetProperty("items").EnumerateArray())
            {
                var cat = item.GetProperty("category").GetString();
                if (cat == "candles" && string.IsNullOrEmpty(candle))
                    candle = FormatTime(item.GetProperty("date").GetString());
                else if (cat == "havdalah" && string.IsNullOrEmpty(havdalah))
                    havdalah = FormatTime(item.GetProperty("date").GetString());
                else if (cat == "parashat" && string.IsNullOrEmpty(parasha))
                    parasha = item.GetProperty("hebrew").GetString() ?? "";
            }
            return new ShabbatTimes(candle, havdalah, parasha);
        }

        private static string FormatTime(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return "";
            return DateTime.Parse(dateString).ToString("HH:mm");
        }
    }
}