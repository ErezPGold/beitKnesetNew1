using System.Collections.Generic;
using BeitKnesetDisplay.Models;

namespace BeitKnesetBoard.Models;

public class YahrzeitDay
{
    public string HebDate { get; set; } = string.Empty;
    public List<Tzaddik> Tzaddikim { get; set; } = new();
}
