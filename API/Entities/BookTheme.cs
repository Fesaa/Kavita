using API.Entities.Enums.Theme;

namespace API.Entities;

public class BookTheme
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public required string NormalizedName { get; set; }

    public required string FileName { get; set; }

    public required ThemeProvider Provider { get; set; }

    public string ColorHash { get; set; }

    public bool IsDefault { get; set; }

    public bool IsDarkTheme { get; set; }

    public string Selector { get; set; }
}
