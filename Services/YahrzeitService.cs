using BeitKnesetBoard.Services;
using BeitKnesetDisplay.Models;
using BeitKnessetDisplay.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeitKnesetDisplay.Services
{
    public static class YahrzeitService
    {
        public static async Task<IReadOnlyList<Tzaddik>> GetTodayAsync()
        {
            string hebrewKey;
            try
            {
                var hebDate = await HebcalDateService.GetTodayHebrewAsync();
                hebrewKey = BuildHebrewKey(hebDate.Day, hebDate.Month);
                System.Diagnostics.Debug.WriteLine($"[Yahrzeit] Raw='{hebDate.Hebrew}' | Key='{hebrewKey}'");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Yahrzeit] Hebcal failed: {ex.Message}");
                return GetHardcodedFallback();
            }

            var local = LocalYahrzeitService.TryGet(hebrewKey);
            if (local != null && local.Count > 0) return local;

            // ... (AI fallback + hardcoded fallback כמו שיש לך)
            return GetHardcodedFallback();
        }

        private static readonly Dictionary<string, string> MonthMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Tishrei"] = "תשרי",
            ["Cheshvan"] = "חשון",
            ["Heshvan"] = "חשון",
            ["Kislev"] = "כסלו",
            ["Tevet"] = "טבת",
            ["Sh'vat"] = "שבט",
            ["Shvat"] = "שבט",
            ["Adar"] = "אדר",
            ["Adar I"] = "אדר א",
            ["Adar II"] = "אדר ב",
            ["Nisan"] = "ניסן",
            ["Iyyar"] = "אייר",
            ["Iyar"] = "אייר",
            ["Sivan"] = "סיון",
            ["Tamuz"] = "תמוז",
            ["Av"] = "אב",
            ["Elul"] = "אלול",
        };

        private static string BuildHebrewKey(int day, string monthEn)
        {
            var monthHe = MonthMap.TryGetValue(monthEn.Trim(), out var m) ? m : monthEn;
            return $"{NumToGematria(day)} {monthHe}";
        }

        private static string NumToGematria(int n)
        {
            // 1-30 — מספיק לימי חודש
            string[] ones = { "", "א", "ב", "ג", "ד", "ה", "ו", "ז", "ח", "ט" };
            string[] tens = { "", "י", "כ", "ל" };
            if (n == 15) return "ט\"ו";
            if (n == 16) return "ט\"ז";
            int t = n / 10, o = n % 10;
            string letters = tens[t] + ones[o];
            if (letters.Length == 1) return letters + "'";
            return letters.Insert(letters.Length - 1, "\"");
        }


        private static async Task<IReadOnlyList<Tzaddik>?> TryFetchFromAiAsync(string hebrewKey)
        {
            var json = await OpenAiClient.AskJsonAsync(
                "You are a Jewish historian. Return JSON only.",
                $"List 3-4 famous tzaddikim whose yahrzeit is on {hebrewKey}. " +
                "Return JSON: {\"tzaddikim\":[{\"name\":\"...\",\"years\":\"...\",\"bio\":\"...\"}]}. " +
                "If unsure - return empty array. Bio in Hebrew, 2-3 sentences."
            );

            if (string.IsNullOrWhiteSpace(json)) return null;

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("tzaddikim", out var arr)) return null;

            var list = new List<Tzaddik>();
            foreach (var el in arr.EnumerateArray())
            {
                var bio = el.TryGetProperty("bio", out var b) ? b.GetString() ?? "" : "";
                list.Add(new Tzaddik
                {
                    Name = el.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                    Years = el.TryGetProperty("years", out var y) ? y.GetString() ?? "" : "",
                    Bio = bio,
                    Description = bio
                });
            }
            return list;
        }

        private static IReadOnlyList<Tzaddik> GetHardcodedFallback()
        {
            return new List<Tzaddik>
            {
                new() { Name = "הבעל שם טוב", Years = "תק\"כ",
                        Bio = "רבי ישראל בן אליעזר, מייסד תנועת החסידות.",
                        Description = "רבי ישראל בן אליעזר, מייסד תנועת החסידות." },
                new() { Name = "האר\"י הקדוש", Years = "של\"ב",
                        Bio = "רבי יצחק לוריא, מייסד תורת הקבלה החדשה בצפת.",
                        Description = "רבי יצחק לוריא, מייסד תורת הקבלה החדשה בצפת." },
                new() { Name = "הרמב\"ם", Years = "ד'תתקס\"ה",
                        Bio = "רבי משה בן מימון, מגדולי הפוסקים, מחבר 'משנה תורה'.",
                        Description = "רבי משה בן מימון, מגדולי הפוסקים, מחבר 'משנה תורה'." },
            };
        }
    }
}
