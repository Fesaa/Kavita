using API.Entities.Enums.Theme;

namespace API.DTOs.Theme;

public class BookThemeDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string ColorHash { get; set; }
    public string Selector { get; set; }
    public bool IsDarkTheme { get; set; }
    public bool IsDefault { get; set; }
    public ThemeProvider Provider { get; set; }
    public string FileName { get; set; }
}
