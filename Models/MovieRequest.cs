
using System.ComponentModel.DataAnnotations;
namespace MvcMovie.Models;
public class MovieRequest()
{
    public int Id { get; set; }

    [StringLength(60, MinimumLength = 3, ErrorMessage = "Tiêu đề phải từ 3 - 60 ký tự")]
    [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
    [Display(Name = "Tiêu đề")]
    public string? Title { get; set; }

    [Display(Name = "Ngày ra mắt")]
    [Required(ErrorMessage = "Vui lòng nhập ngày ra mắt")]
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thể loại")]
    [Display(Name = "Thể loại")]
    [StringLength(30, ErrorMessage = "Thể loại không vượt quá 30 ký tự")]
    public string? Genre { get; set; }

    [StringLength(60, MinimumLength = 3, ErrorMessage = "Dòng phim phải từ 3 - 60 ký tự")]
    [Display(Name = "Dòng phim")]
    [Required(ErrorMessage = "Vui lòng nhập dòng phim")]

    public string? MovieType { get; set; }

    [Range(3, 100, ErrorMessage = "Độ tuổi từ 3 - 100")]
    [Display(Name = "Độ tuổi")]
    public int Age { get; set; }

    [StringLength(300, MinimumLength = 3, ErrorMessage = "Mô tả phải từ 3 - 300 ký tự")]
    [Required(ErrorMessage = "Vui lòng nhập mô tả")]
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    public IFormFile? Image { get; set; }

}
