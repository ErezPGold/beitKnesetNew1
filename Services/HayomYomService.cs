using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BeitKnessetDisplay.Services
{
    public static class HayomYomService
    {
        private static readonly HttpClient _http = new();
        private static readonly SemaphoreSlim _gate = new(1, 1);

        private static string? _memoryDate;
        private static string? _memoryQuote;
        private static DateTime _nextAllowedRequestUtc = DateTime.MinValue;

        private static string TodayKey => DateTime.Now.ToString("yyyy-MM-dd");

        private static string CacheFile
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "BeitKnesetBoard", "Cache");
                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "hayom-yom.json");
            }
        }

        public static async Task<string> GetTodayQuoteAsync()
        {
            var today = TodayKey;

            if (_memoryDate == today && !string.IsNullOrWhiteSpace(_memoryQuote))
                return _memoryQuote!;

            var cached = await ReadCacheAsync();
            if (cached?.Date == today && !string.IsNullOrWhiteSpace(cached.Quote))
            {
                _memoryDate = cached.Date;
                _memoryQuote = cached.Quote;
                return cached.Quote!;
            }

            if (DateTime.UtcNow < _nextAllowedRequestUtc)
                return "";

            await _gate.WaitAsync();
            try
            {
                if (_memoryDate == today && !string.IsNullOrWhiteSpace(_memoryQuote))
                    return _memoryQuote!;

                // פנייה דרך r.jina.ai – מחזיר Markdown נקי של הדף בעברית
                var url = "https://r.jina.ai/https://he.chabad.org/calendar/view/day_cdo/aid/2263399/jewish/Hayom-Yom.htm";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 BeitKnesetBoard/1.0");
                request.Headers.Accept.ParseAdd("text/plain");

                using var response = await _http.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _nextAllowedRequestUtc = DateTime.UtcNow.AddMinutes(30);
                    System.Diagnostics.Debug.WriteLine("[HayomYom] 429 from jina.");
                    return "";
                }

                if (!response.IsSuccessStatusCode)
                {
                    _nextAllowedRequestUtc = DateTime.UtcNow.AddMinutes(15);
                    System.Diagnostics.Debug.WriteLine($"[HayomYom] HTTP {(int)response.StatusCode}");
                    return "";
                }

                var markdown = await response.Content.ReadAsStringAsync();
                var quote = ExtractQuote(markdown);

                if (string.IsNullOrWhiteSpace(quote))
                {
                    _nextAllowedRequestUtc = DateTime.UtcNow.AddMinutes(30);
                    System.Diagnostics.Debug.WriteLine("[HayomYom] empty quote after extraction.");
                    return "";
                }

                _memoryDate = today;
                _memoryQuote = quote;
                await WriteCacheAsync(new HayomYomCache { Date = today, Quote = quote });
                return quote;
            }
            catch (Exception ex)
            {
                _nextAllowedRequestUtc = DateTime.UtcNow.AddMinutes(15);
                System.Diagnostics.Debug.WriteLine($"[HayomYom] {ex.Message}");
                return "";
            }
            finally
            {
                _gate.Release();
            }
        }

        private static string ExtractQuote(string md)
        {
            // r.jina.ai מחזיר Markdown. נחתוך החל מהכותרת "היום יום" ועד "שיעורים" / "לוח השיעורים"
            var startIdx = md.IndexOf("היום יום", StringComparison.Ordinal);
            if (startIdx < 0) startIdx = 0;

            var slice = md.Substring(startIdx);

            var endMarkers = new[] { "שיעורים", "לוח השיעורים", "תגובות", "שתפו", "Hayom" };
            int end = slice.Length;
            foreach (var marker in endMarkers)
            {
                var idx = slice.IndexOf(marker, 50, StringComparison.Ordinal);
                if (idx > 0 && idx < end) end = idx;
            }
            slice = slice.Substring(0, end);

            // ניקוי שורות לא רלוונטיות
            var lines = slice.Split('\n');
            var sb = new System.Text.StringBuilder();
            foreach (var raw in lines)
            {
                var line = CleanLine(raw);
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.Length < 25) continue;                  // ניווט / כותרות
                if (line.Contains("chabad.org", StringComparison.OrdinalIgnoreCase)) continue;
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("===") || line.StartsWith("---")) continue;
                if (Regex.IsMatch(line, @"^\d{1,2}\s")) continue; // תאריכים בתחילת שורה

                sb.AppendLine(line);
                if (sb.Length > 700) break;
            }

            var result = sb.ToString().Trim();
            if (result.Length > 900) result = result.Substring(0, 900) + "…";
            return result;
        }

        private static string CleanLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return "";
            line = Regex.Replace(line, @"!\[[^\]]*\]\([^)]+\)", "");
            line = Regex.Replace(line, @"\[(.*?)\]\([^)]+\)", "$1");
            line = line.Replace("**", "").Replace("__", "");
            line = WebUtility.HtmlDecode(line);
            line = Regex.Replace(line, @"\s+", " ").Trim();
            return line;
        }

        private static async Task<HayomYomCache?> ReadCacheAsync()
        {
            try
            {
                if (!File.Exists(CacheFile)) return null;
                var json = await File.ReadAllTextAsync(CacheFile);
                return JsonSerializer.Deserialize<HayomYomCache>(json);
            }
            catch { return null; }
        }

        private static async Task WriteCacheAsync(HayomYomCache cache)
        {
            try
            {
                var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(CacheFile, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HayomYom Cache] {ex.Message}");
            }
        }

        private sealed class HayomYomCache
        {
            public string? Date { get; set; }
            public string? Quote { get; set; }
        }
    }
}
