using MvcMovie.Models;

namespace MvcMovie.Services;

public interface IMovieService
{
    Task<IEnumerable<MovieViewModel>> GetMovies(string searchString, string sortOrder , int pageNumber, int pageSize);


    Task<MovieViewModel> GetMovie(int id);
    Task<Movie> Create(MovieRequest request);

    Task<bool> Update(int id, MovieViewModel movei);
    Task<bool> Delete(int id);
    bool MovieExists(int id);
}