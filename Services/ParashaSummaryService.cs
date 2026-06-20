using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BeitKnesset.Services
{
    public class ParshaInfo
    {
        public string Name { get; set; } = "";
        public string Summary { get; set; } = "";
    }

    public class ParashaSummaryService
    {
        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        public async Task<ParshaInfo> GetWeeklyParshaAsync()
        {
            var info = new ParshaInfo();
            try
            {
                var today = DateTime.Now.ToString("yyyy-MM-dd");
                var url = $"https://www.sefaria.org/api/calendars?year={DateTime.Now.Year}&month={DateTime.Now.Month}&day={DateTime.Now.Day}";
                var json = await _http.GetStringAsync(url);

                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("calendar_items", out var items))
                {
                    foreach (var item in items.EnumerateArray())
                    {
                        if (item.TryGetProperty("title", out var title) &&
                            title.TryGetProperty("he", out var he) &&
                            item.TryGetProperty("displayValue", out var disp) &&
                            disp.TryGetProperty("he", out var heVal) &&
                            title.GetProperty("en").GetString() == "Parashat Hashavua")
                        {
                            info.Name = heVal.GetString() ?? "";
                            break;
                        }
                    }
                }

                info.Summary = "פרשת " + info.Name + " — לחצו לסיכום מלא בעלון השבועי.";
            }
            catch
            {
                info.Name = "";
                info.Summary = "";
            }
            return info;
        }
    }
}
