using System;
using System.Collections.Generic;
using Zmanim;
using Zmanim.TimeZone;
using Zmanim.Utilities;

namespace BeitKnesset.Services
{
    public record ZmanItem(string Name, string Time);

    public class ZmanimService
    {
        private readonly GeoLocation _location;

        public ZmanimService(double lat = 32.08, double lng = 34.78, double elevation = 0)
        {
            ITimeZone tz;
            try
            {
                var tzi = TimeZoneInfo.FindSystemTimeZoneById("Israel Standard Time");
                tz = new WindowsTimeZone(tzi);
            }
            catch
            {
                tz = new WindowsTimeZone(TimeZoneInfo.Local);
            }

            _location = new GeoLocation("Beit Knesset", lat, lng, elevation, tz);
        }

        public IReadOnlyList<ZmanItem> GetTodayZmanim()
        {
            // יוצרים מופע חדש בכל קריאה — זה ה-fix
            var cz = new ComplexZmanimCalendar(_location)
            {
                DateWithLocation = new DateWithLocation(DateTime.Now, _location)
            };

            string Fmt(DateTime? dt) => dt.HasValue ? dt.Value.ToString("HH:mm") : "--:--";

            return new List<ZmanItem>
            {
                new("עלות השחר",       Fmt(cz.GetAlosHashachar())),
                new("נץ החמה",         Fmt(cz.GetSunrise())),
                new("סוף זמן ק\"ש",    Fmt(cz.GetSofZmanShmaGRA())),
                new("סוף זמן תפילה",   Fmt(cz.GetSofZmanTfilaGRA())),
                new("חצות",            Fmt(cz.GetChatzos())),
                new("מנחה גדולה",      Fmt(cz.GetMinchaGedola())),
                new("מנחה קטנה",       Fmt(cz.GetMinchaKetana())),
                new("שקיעה",           Fmt(cz.GetSunset())),
                new("צאת הכוכבים",     Fmt(cz.GetSunsetOffsetByDegrees(96))),
            };
        }
    }
}
