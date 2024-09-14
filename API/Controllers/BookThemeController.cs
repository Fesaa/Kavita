using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using API.Constants;
using API.Data;
using API.DTOs.Font;
using API.DTOs.Theme;
using API.Entities.Enums.Theme;
using API.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MimeTypes;

namespace API.Controllers;

[Authorize]
public class BookThemeController: BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDirectoryService _directoryService;
    private readonly IBookThemeService _bookThemeService;
    private readonly IMapper _mapper;

    public BookThemeController(IUnitOfWork unitOfWork, IDirectoryService directoryService, IBookThemeService bookThemeService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _directoryService = directoryService;
        _bookThemeService = bookThemeService;
        _mapper = mapper;
    }

    /// <summary>
    /// List out all book themes
    /// </summary>
    /// <returns></returns>
    //[ResponseCache(CacheProfileName = ResponseCacheProfiles.TenMinute)] Turned off for development
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<BookThemeDto>>> GetBookThemes()
    {
        return Ok(await _unitOfWork.BookThemeRepository.GetBookThemeDtosAsync());
    }

    /// <summary>
    /// Returns a book theme
    /// </summary>
    /// <param name="bookThemeId"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<BookThemeDto>> GetBookTheme(int bookThemeId)
    {
        var bookTheme = await _unitOfWork.BookThemeRepository.GetBookThemeDtoAsync(bookThemeId);
        if (bookTheme == null)
        {
            return NotFound();
        }

        if (bookTheme.Provider == ThemeProvider.System)
        {
            return BadRequest("System provided themes are not loaded by API");
        }

        var contentType = MimeTypeMap.GetMimeType(Path.GetExtension(bookTheme.FileName));
        var path = Path.Join(_directoryService.BookThemeDirectory, bookTheme.FileName);

        return PhysicalFile(path, contentType, true);
    }

    /// <summary>
    /// Removes a book theme from the system
    /// </summary>
    /// <param name="bookThemeId"></param>
    /// <param name="force">If the book theme is in use by other users and an admin wants it deleted, can they force delete it</param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<IActionResult> DeleteBookTheme(int bookThemeId, bool force = false)
    {
        var forceDelete = User.IsInRole(PolicyConstants.AdminRole) && force;
        await _bookThemeService.Delete(bookThemeId, forceDelete);
        return Ok();
    }

    /// <summary>
    /// Manual upload
    /// </summary>
    /// <param name="formFile"></param>
    /// <returns></returns>
    [HttpPost("upload")]
    public async Task<ActionResult<BookThemeDto>> UploadFont(IFormFile formFile)
    {
        if (!formFile.FileName.EndsWith(".css")) return BadRequest("Invalid file");

        if (formFile.FileName.Contains("..")) return BadRequest("Invalid file");


        var tempFile = await UploadToTemp(formFile);
        var font = await _bookThemeService.CreateBookThemeFromFileAsync(tempFile);
        return Ok(_mapper.Map<EpubFontDto>(font));
    }

    private async Task<string> UploadToTemp(IFormFile file)
    {
        var outputFile = Path.Join(_directoryService.TempDirectory, file.FileName);
        await using var stream = System.IO.File.Create(outputFile);
        await file.CopyToAsync(stream);
        stream.Close();
        return outputFile;
    }
}
