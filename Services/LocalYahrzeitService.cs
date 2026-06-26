using BeitKnesetDisplay.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BeitKnessetDisplay.Services
{
    /// <summary>
    /// טוען רשימת צדיקים מקובץ JSON מקומי (Data/yahrzeits.json)
    /// המפתח במילון = תאריך עברי בפורמט "י\"א תמוז"
    /// </summary>
    public static class LocalYahrzeitService
    {
        private static Dictionary<string, List<Tzaddik>>? _data;
        private static readonly object _lock = new();

        private static void EnsureLoaded()
        {
            if (_data != null) return;
            lock (_lock)
            {
                if (_data != null) return;
                try
                {
                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "yahrzeits.json");
                    if (!File.Exists(path))
                    {
                        System.Diagnostics.Debug.WriteLine($"[LocalYahrzeit] File not found: {path}");
                        _data = new Dictionary<string, List<Tzaddik>>();
                        return;
                    }

                    var json = File.ReadAllText(path);
                    var raw = JsonSerializer.Deserialize<Dictionary<string, List<TzaddikDto>>>(json);
                    _data = new Dictionary<string, List<Tzaddik>>(StringComparer.Ordinal);

                    if (raw != null)
                    {
                        foreach (var kv in raw)
                        {
                            var key = NormalizeKey(kv.Key);
                            var list = kv.Value.Select(d => new Tzaddik
                            {
                                Name = d.name ?? "",
                                Years = d.years ?? "",
                                Bio = d.bio ?? "",
                                Description = d.bio ?? ""
                            }).ToList();

                            if (_data.ContainsKey(key))
                                _data[key].AddRange(list);
                            else
                                _data[key] = list;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"[LocalYahrzeit] Loaded {_data.Count} dates from JSON");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[LocalYahrzeit] Load error: {ex.Message}");
                    _data = new Dictionary<string, List<Tzaddik>>();
                }
            }
        }

        /// <summary>
        /// מחזיר רשימת צדיקים לתאריך עברי. null אם אין התאמה.
        /// </summary>
        public static IReadOnlyList<Tzaddik>? TryGet(string hebrewDate)
        {
            EnsureLoaded();
            if (string.IsNullOrWhiteSpace(hebrewDate)) return null;

            var key = NormalizeKey(hebrewDate);
            if (_data!.TryGetValue(key, out var list) && list.Count > 0)
                return list;

            return null;
        }

        /// <summary>
        /// מנרמל: מסיר רווחים כפולים, גרשיים שונים, ומפצל "כ"ה תשרי תשפ"ו" → "כ"ה תשרי"
        /// </summary>
        private static string NormalizeKey(string input)
        {
            var s = input.Trim()
                         .Replace("\u05F3", "'")   // גרש עברי
                         .Replace("\u05F4", "\"")  // גרשיים עבריות
                         .Replace("׳", "'")
                         .Replace("״", "\"");

            // השאר רק יום + חודש (שתי המילים הראשונות)
            var parts = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return parts[0] + " " + parts[1];

            return s;
        }

        private class TzaddikDto
        {
            public string? name { get; set; }
            public string? years { get; set; }
            public string? bio { get; set; }
        }
    }
}
