namespace BeitKnesetDisplay.Models
{
    public class Tzaddik
    {
        private string _description = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Years { get; set; } = string.Empty;

        // כדי שקוד ישן שמשתמש ב-Bio לא יפיל שגיאה
        public string Bio { get; set; } = string.Empty;

        // זה מה שה-XAML שלך מציג
        public string Description
        {
            get => string.IsNullOrWhiteSpace(_description) ? Bio : _description;
            set => _description = value ?? string.Empty;
        }

        public string Saying { get; set; } = string.Empty;
    }
}
