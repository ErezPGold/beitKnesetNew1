// PrayerService.cs
using System.IO;
using System.Text.Json;

namespace BeitKnessetDisplay
{
    public class Prayers { public string Shacharit { get; set; } = ""; public string Mincha { get; set; } = ""; public string Arvit { get; set; } = ""; }

    public class PrayerService
    {
        private readonly Prayers? _prayers;
        

        public PrayerService()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "prayers.json");
            try
            {
                if (File.Exists("prayers.json"))
                    _prayers = JsonSerializer.Deserialize<Prayers>(File.ReadAllText(path));
            }
            catch { /* log */ }
        }
        public string GetFormatted() =>
            _prayers is null ? "לא נטענו זמני תפילות"
            : $"שחרית {_prayers.Shacharit}    מנחה {_prayers.Mincha}    ערבית {_prayers.Arvit}";
    }
}
