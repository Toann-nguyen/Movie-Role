using System.ComponentModel.DataAnnotations;

namespace MvcMovie.Models;
public class MovieViewModel
{
    public int Id { get; set; }

    [Display(Name = "Tiêu đề")]
    public string? Title { get; set; }

    [Display(Name = "Ngày ra mắt")]
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    [Display(Name = "Thể loại")]
    public string? Genre { get; set; }

    [Display(Name = "Dòng phim")]
    public string? MovieType { get; set; }

    [Display(Name = "Độ tuổi")]
    public int Age { get; set; }

    [Display(Name = "Mô tả")]
    public string? Description { get; set; }
    public string? ImagePath { get; set; }
    public IFormFile? Image {get; set;}
}
