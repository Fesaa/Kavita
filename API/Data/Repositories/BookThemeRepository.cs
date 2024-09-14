#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs.Theme;
using API.Entities;
using API.Extensions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;


namespace API.Data.Repositories;

public interface IBookThemeRepository
{
    void Add(BookTheme bookTheme);
    void Remove(BookTheme bookTheme);

    Task<IEnumerable<BookThemeDto>> GetBookThemeDtosAsync();
    Task<BookThemeDto?> GetBookThemeDtoAsync(int bookThemeId);
    Task<BookThemeDto?> GetBookThemeDtoByNameAsync(string name);

    Task<IEnumerable<BookTheme>> GetBookThemesAsync();
    Task<BookTheme?> GetBookThemeAsync(int bookThemeId);
    Task<bool> IsBookThemeInUseAsync(int bookThemeId);
}

public class BookThemeRepository: IBookThemeRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public BookThemeRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void Add(BookTheme bookTheme)
    {
        _context.Add(bookTheme);
    }

    public void Remove(BookTheme bookTheme)
    {
        _context.Remove(bookTheme);
    }

    public async Task<IEnumerable<BookThemeDto>> GetBookThemeDtosAsync()
    {
        return await _context.BookTheme
            .OrderBy(s => s.IsDefault ? -1 : 0)
            .ThenBy(s => s)
            .ProjectTo<BookThemeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

    }

    public async Task<BookThemeDto?> GetBookThemeDtoAsync(int bookThemeId)
    {
        return await _context.BookTheme
            .Where(b => b.Id == bookThemeId)
            .ProjectTo<BookThemeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<BookThemeDto?> GetBookThemeDtoByNameAsync(string name)
    {
        return await _context.BookTheme
            .Where(b => b.NormalizedName.Equals(name.ToNormalized()))
            .ProjectTo<BookThemeDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<BookTheme>> GetBookThemesAsync()
    {
        return await _context.BookTheme
            .ToListAsync();
    }

    public async Task<BookTheme?> GetBookThemeAsync(int bookThemeId)
    {
        return await _context.BookTheme
            .FirstOrDefaultAsync(b => b.Id == bookThemeId);
    }

    public async Task<bool> IsBookThemeInUseAsync(int bookThemeId)
    {
        return await _context.AppUserPreferences
            .Join(_context.BookTheme,
                p => p.BookThemeName,
                b => b.Name,
                (p, b) => new {p, b})
            .AnyAsync(joined => joined.b.Id == bookThemeId);
    }
}
