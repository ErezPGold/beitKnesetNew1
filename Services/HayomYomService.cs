using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeitKnessetDisplay.Services
{
    public static class HayomYomService
    {
        private static readonly HttpClient _http = new();

        public static async Task<string> GetTodayQuoteAsync()
        {
            try
            {
                var url = "https://www.chabad.org/calendar/view/day_cdo/aid/2263399/jewish/Hayom-Yom.htm";
                var html = await _http.GetStringAsync(url);

                // נסה לחלץ את גוף ה"היום יום" (Co_Body / class המכיל את הטקסט)
                var m = Regex.Match(html,
                    @"<div[^>]*class=""[^""]*(?:Co_Body|article-body|co_body)[^""]*""[^>]*>(.*?)</div>",
                    RegexOptions.Singleline | RegexOptions.IgnoreCase);

                if (!m.Success) return "";

                var text = Regex.Replace(m.Groups[1].Value, "<.*?>", " ");
                text = System.Net.WebUtility.HtmlDecode(text);
                text = Regex.Replace(text, @"\s+", " ").Trim();

                if (text.Length > 320) text = text.Substring(0, 320) + "…";
                return text;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HayomYom] {ex.Message}");
                return "";
            }
        }
    }
}
