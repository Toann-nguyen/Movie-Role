using System.Net.Http.Headers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;
using MvcMovie.Models;

namespace MvcMovie.Services;

public class MovieService : IMovieService
{
    private readonly MvcMovieContext _context;
    private readonly IMovieService _movieService;
    private readonly IMapper _mapper;
    private readonly IStorageService _storageService;
    private const string USER_CONTENT_FOLDER_NAME = "user-content";

    public MovieService(MvcMovieContext context, IMapper mapper, IStorageService storageService)
    {
        _context = context;
        _mapper = mapper;
        _storageService = storageService;
    }
    private async Task<string> SaveFile(IFormFile file)
    {
        var originalFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName!.Trim('"');
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(originalFileName)}";
        await _storageService.SaveFileAsync(file.OpenReadStream(), fileName);
        return "/" + USER_CONTENT_FOLDER_NAME + "/" + fileName;
    }
    public async Task<Movie> Create(MovieRequest request)
    {
        var movie = _mapper.Map<Movie>(request);
        if (request.Image != null)
        {
            movie.ImagePath = await SaveFile(request.Image);
        }
        _context.Movie.Add(movie);
        await _context.SaveChangesAsync();
        return movie;
    }

    public async Task<bool> Delete(int id)
    {
        var movie = await _context.Movie.FindAsync(id);
        if (movie != null)
        {
            if (!string.IsNullOrEmpty(movie.ImagePath))
                await _storageService.DeleteFileAsync(movie.ImagePath.Replace("/" + USER_CONTENT_FOLDER_NAME + "/", ""));
            _context.Movie.Remove(movie);
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<MovieViewModel>> GetMovies()
    {
        var movies = await _context.Movie.ToListAsync();
        return _mapper.Map<IEnumerable<MovieViewModel>>(movies);
    }

    public async Task<MovieViewModel> GetMovie(int id)
    {
        var movie = await _context.Movie.FirstOrDefaultAsync(m => m.Id == id);

        return _mapper.Map<MovieViewModel>(movie);
    }

    public bool MovieExists(int id)
    {
        return _context.Movie.Any(e => e.Id == id);
    }

    public async Task<bool> Update(int id, MovieViewModel movie)
    {
        // Save image file
        if (movie.Image != null)
        {
            if (!string.IsNullOrEmpty(movie.ImagePath))
                await _storageService.DeleteFileAsync(movie.ImagePath.Replace("/" + USER_CONTENT_FOLDER_NAME + "/", ""));
            movie.ImagePath = await SaveFile(movie.Image);
        }
        _context.Update(_mapper.Map<Movie>(movie));
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<MovieViewModel>> GetMovies(string searchString)
    {
        var movies = _context.Movie.AsQueryable();

        if (!string.IsNullOrEmpty(searchString))
        {
            movies = movies.Where(s => s.Title != null && s.Title.Contains(searchString));
        }

        return _mapper.Map<IEnumerable<MovieViewModel>>(await movies.ToListAsync());
    }

    

    public async Task<Movie> GetMovieById(int id)
    {
        return await _context.Movie.FindAsync(id);
    }

    // Action hiển thị chi tiết phim
    
}