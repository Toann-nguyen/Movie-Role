using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;
using MvcMovie.Services;

namespace MvcMovie.Controllers;

public class AnimeDetailsController : Controller
{
    private readonly IMovieService _movieService;

    public AnimeDetailsController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    // public  Task<IActionResult> AnimeDetails(int id)
    // {
    //     // Lấy thông tin phim theo id
    //     var movie = _movieService.GetMovieById(id);
    //     if (movie == null)
    //     {
    //         return NotFound(); // Nếu không tìm thấy phim, trả về lỗi 404
    //     }

    //     return View(movie); // Truyền dữ liệu sang view
    // }
    public async Task<IActionResult> AnimeDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var movie = await _movieService.GetMovie(id.Value);
        if (movie == null)
        {
            return NotFound();
        }

        return View(movie);
    }
}
