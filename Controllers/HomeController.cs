using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;
using MvcMovie.Services;

namespace MvcMovie.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IMovieService _movieService;

    public HomeController(IMovieService movieService, ILogger<HomeController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber, int pageSize = 10)
    {
        // Lấy danh sách phim từ service
        var movies = await _movieService.GetMovies("", "", pageNumber ?? 1, pageSize);

        // Kiểm tra dữ liệu
        ViewBag.MovieCount = movies?.Count() ?? 0;
        ViewBag.HasMovies = movies != null && movies.Any();

        // Truyền dữ liệu sang View
        return View(movies);
    }
    public IActionResult UserInfor()
    {
        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
