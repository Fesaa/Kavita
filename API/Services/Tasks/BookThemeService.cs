using System.IO;
using System.Threading.Tasks;
using API.Data;
using API.Entities;
using API.Entities.Enums.Theme;
using API.Services.Tasks;
using API.Services.Tasks.Scanner.Parser;
using Kavita.Common;
using Microsoft.Extensions.Logging;

namespace API.Services;

public interface IBookThemeService
{

    Task<BookTheme> CreateBookThemeFromFileAsync(string path);
    Task Delete(int themeId, bool force);

}

public class BookThemeService: IBookThemeService
{
    public static readonly string DefaultTheme = "Dark";

    private readonly IDirectoryService _directoryService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<FontService> _logger;

    public BookThemeService(IDirectoryService directoryService, IUnitOfWork unitOfWork, ILogger<FontService> logger)
    {
        _directoryService = directoryService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BookTheme> CreateBookThemeFromFileAsync(string path)
    {
        if (!_directoryService.FileSystem.File.Exists(path))
        {
            _logger.LogInformation("Unable to create BookTheme, file does not exist. {Path}", path);
            throw new KavitaException("errors.book-theme-file-missing"); // TODO: Add all translation keys
        }

        var fileName = _directoryService.FileSystem.FileInfo.New(path).Name;
        var nakedFileName = _directoryService.FileSystem.Path.GetFileNameWithoutExtension(fileName);
        var bookThemeName = Parser.PrettifyFileName(nakedFileName);
        var normalizedName = Parser.Normalize(nakedFileName);

        if (await _unitOfWork.BookThemeRepository.GetBookThemeDtoByNameAsync(bookThemeName) != null)
        {
            throw new KavitaException("errors.book-theme-already-in-use");
        }

        _directoryService.CopyFileToDirectory(path, _directoryService.BookThemeDirectory);
        var finalLocation = _directoryService.FileSystem.Path.Join(_directoryService.BookThemeDirectory, fileName);

        var bookTheme = new BookTheme()
        {
            Name = bookThemeName,
            NormalizedName = normalizedName,
            FileName = Path.GetFileName(finalLocation),
            Provider = ThemeProvider.Custom,
            ColorHash = "", // TODO: Generate this from somewhere? Ask this user this?
            IsDefault = false,
            IsDarkTheme = false, // TODO: Ask this user this? Based on ColorHash?
            Selector = "brtheme-" + normalizedName
        };

        _unitOfWork.BookThemeRepository.Add(bookTheme);
        await _unitOfWork.CommitAsync();

        // TODO: Send update to UI
        return bookTheme;
    }

    public Task Delete(int themeId, bool force)
    {
        throw new System.NotImplementedException();
    }
}
