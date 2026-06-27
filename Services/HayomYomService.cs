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
                    "BeitKnesetBoard",
                    "Cache");

                Directory.CreateDirectory(dir);
                return Path.Combine(dir, "hayom-yom.json");
            }
        }

        public static async Task<string> GetTodayQuoteAsync()
        {
            var today = TodayKey;

            // 1. זיכרון פנימי — הכי מהיר
            if (_memoryDate == today && !string.IsNullOrWhiteSpace(_memoryQuote))
                return _memoryQuote;

            // 2. Cache מקומי מהדיסק
            var cached = await ReadCacheAsync();
            if (cached != null &&
                cached.Date == today &&
                !string.IsNullOrWhiteSpace(cached.Quote))
            {
                _memoryDate = cached.Date;
                _memoryQuote = cached.Quote;
                return cached.Quote;
            }

            // 3. אם קיבלנו 429 לאחרונה — לא מנסים שוב ושוב
            if (DateTime.UtcNow < _nextAllowedRequestUtc)
                return "";

            await _gate.WaitAsync();

            try
            {
                // בדיקה חוזרת אחרי שנכנסנו ל־lock
                if (_memoryDate == today && !string.IsNullOrWhiteSpace(_memoryQuote))
                    return _memoryQuote;

                cached = await ReadCacheAsync();
                if (cached != null &&
                    cached.Date == today &&
                    !string.IsNullOrWhiteSpace(cached.Quote))
                {
                    _memoryDate = cached.Date;
                    _memoryQuote = cached.Quote;
                    return cached.Quote;
                }

                var url = "https://www.chabad.org/calendar/view/day_cdo/aid/2263399/jewish/Hayom-Yom.htm";

                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 BeitKnesetBoard/1.0");

                using var response = await _http.SendAsync(request);

                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    _nextAllowedRequestUtc = DateTime.UtcNow.Add(GetRetryDelay(response));
                    System.Diagnostics.Debug.WriteLine("[HayomYom] 429 Too Many Requests. Waiting before next try.");
                    return "";
                }

                if (!response.IsSuccessStatusCode)
                {
                    _nextAllowedRequestUtc = DateTime.UtcNow.AddMinutes(15);
                    System.Diagnostics.Debug.WriteLine($"[HayomYom] HTTP {(int)response.StatusCode} {response.ReasonPhrase}");
                    return "";
                }

                var html = await response.Content.ReadAsStringAsync();
                var quote = ExtractQuote(html);

                if (string.IsNullOrWhiteSpace(quote))
                {
                    _nextAllowedRequestUtc = DateTime.UtcNow.AddMinutes(30);
                    return "";
                }

                _memoryDate = today;
                _memoryQuote = quote;

                await WriteCacheAsync(new HayomYomCache
                {
                    Date = today,
                    Quote = quote
                });

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

        private static TimeSpan GetRetryDelay(HttpResponseMessage response)
        {
            var retryAfter = response.Headers.RetryAfter;

            if (retryAfter?.Delta != null)
                return retryAfter.Delta.Value;

            if (retryAfter?.Date != null)
            {
                var delay = retryAfter.Date.Value.UtcDateTime - DateTime.UtcNow;
                if (delay > TimeSpan.Zero)
                    return delay;
            }

            return TimeSpan.FromMinutes(30);
        }

        private static string ExtractQuote(string html)
        {
            var m = Regex.Match(
                html,
                @"<div[^>]*class=""[^""]*(?:Co_Body|article-body|co_body)[^""]*""[^>]*>(.*?)</div>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (!m.Success)
                return "";

            var text = Regex.Replace(m.Groups[1].Value, "<.*?>", " ");
            text = WebUtility.HtmlDecode(text);
            text = Regex.Replace(text, @"\s+", " ").Trim();

            if (text.Length > 900)
                text = text.Substring(0, 900) + "…";

            return text;
        }

        private static async Task<HayomYomCache?> ReadCacheAsync()
        {
            try
            {
                if (!File.Exists(CacheFile))
                    return null;

                var json = await File.ReadAllTextAsync(CacheFile);
                return JsonSerializer.Deserialize<HayomYomCache>(json);
            }
            catch
            {
                return null;
            }
        }

        private static async Task WriteCacheAsync(HayomYomCache cache)
        {
            try
            {
                var json = JsonSerializer.Serialize(cache, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

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
