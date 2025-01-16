namespace MvcMovie.Models;
public class Payment
{
    public int Id { get; set; }
    public string? PaymentMethod { get; set; }
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Note { get; set; }
    public double Amount { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? Status { get; set; }
    public int OrderId { get; set; }
}