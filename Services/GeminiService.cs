using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BeitKnessetDisplay.Services
{
    public class GeminiService
    {
        private readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        private readonly string _apiKey;
        private const string Model = "gemini-2.0-flash";

        public GeminiService(string apiKey) { _apiKey = apiKey; }

        public async Task<string> GetParshaSummaryAsync(string parshaName)
        {
            var prompt = $@"כתוב סיכום קצר וברור בעברית של פרשת {parshaName}, באורך 4-5 משפטים.
                        התמקד באירועים המרכזיים והמסרים העיקריים. כתוב בשפה פשוטה ונגישה. 
                        ללא כותרת, ללא הקדמה - רק הסיכום עצמו.";

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{Model}:generateContent?key={_apiKey}";
            var body = new
            {
                contents = new[] {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await _http.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();

            var respJson = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(respJson);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString()?.Trim() ?? "";
        }
    }
}

