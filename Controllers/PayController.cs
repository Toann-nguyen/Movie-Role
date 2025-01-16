using MvcMovie.ViewModels.VNPay;
using MvcMovie.Services;
using Microsoft.AspNetCore.Mvc;
using MvcMovie.Models;
using MvcMovie.Data;
namespace MvcMovie.Controllers
{
    public class PayController : Controller
    {
        private readonly IVNPayService _vnPayService;
        private readonly MvcMovieContext _context;
        public PayController(IVNPayService vnPayService, MvcMovieContext context)
        {
            _vnPayService = vnPayService;
            _context = context;

        }

        public static Dictionary<string, string> vnp_TransactionStatus = new Dictionary<string, string>()
        {
            {"00","Transaction successful" },
            {"01","Transaction not completed" },
            {"02","Transaction failed" },
            {"04","Transaction is being rolled back" },
            {"05","VNPAY is processing the transaction" },
            {"06","VNPAY has sent a refund request to the bank" },
            {"07","Transaction is suspected of fraud" },
            {"09","Refund failed" }
        };

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Pay()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Pay(CheckoutViewModel request)
        {
            if (ModelState.IsValid)
            {
                if (request.PaymentMethod == "VNPay")
                {
                    // Loại bỏ tất cả dấu chấm trong chuỗi số tiền
                    string cleanedAmount = request.Amount!.Replace(".", "");

                    // Đảm bảo cleanedAmount là số hợp lệ
                    if (!double.TryParse(cleanedAmount, out double amount))
                    {
                        ModelState.AddModelError("Amount", "Số tiền không hợp lệ");
                        return View(request);
                    }
                    // Lưu thông tin thanh toán vào database
                    var payment = new Payment
                    {
                        PaymentMethod = request.PaymentMethod,
                        FullName = request.FullName,
                        Address = request.Address,
                        PhoneNumber = request.PhoneNumber,
                        Note = request.Note,
                        Amount = amount,
                        CreatedDate = DateTime.Now,
                        Status = "Đang thanh toán",
                    };

                    _context.Payments.Add(payment);
                    _context.SaveChanges();

                    var vnPayModel = new VNPaymentRequestModel
                    {
                        Amount = amount, // Số tiền đã được chuyển đổi thành số
                        CreatedDate = DateTime.Now,
                        Description = $"{request.FullName} {request.PhoneNumber}",
                        FullName = request.FullName,
                        OrderId = (int)(DateTime.Now.Ticks % 100000)
                    };

                    return Redirect(_vnPayService.CreatePaymentUrl(HttpContext, vnPayModel));
                }
                return View();
            }
            return View(request);
        }



        public IActionResult PaymentSuccess()
        {
            return View();
        }

        public IActionResult PaymentFail()
        {
            return View();
        }

        public IActionResult PaymentCallBack()
        {
            // Cập nhật trạng thái thanh toán


            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.VNPayResponseCode == "00")
            {
                    
                // Processed successfully
                return RedirectToAction(nameof(PaymentSuccess));
            }

            // Get the message corresponding to VNPayResponseCode from the dictionary
            if (vnp_TransactionStatus.TryGetValue(response.VNPayResponseCode!, out var message))
            {
                TempData["Message"] = $"Payment error: {message}";
            }
            else
            {
                TempData["Message"] = $"Unknown payment error: {response.VNPayResponseCode}";
            }

            // Chuyển đổi OrderId từ string sang int
            if (int.TryParse(response.OrderId, out int orderId))
            {
                var payment = _context.Payments.FirstOrDefault(p => p.OrderId == orderId);
                if (payment != null)
                {
                    payment.Status = response.VNPayResponseCode == "00" ? "Đã thanh toán" : "Thanh toán thất bại";
                    _context.SaveChanges();
                }
                else
                {

                }
            }

            return RedirectToAction(nameof(PaymentFail));
        }

        private void UpdatePaymentStatus(string? orderId, string v, string message)
        {

            // Chuyển đổi OrderId từ string sang int
            if (int.TryParse(orderId, out int parsedOrderId))
            {
                var payment = _context.Payments.FirstOrDefault(p => p.OrderId == parsedOrderId);
                if (payment != null)
                {
                    payment.Status = message;  // Cập nhật trạng thái thanh toán
                    _context.SaveChanges();  // Lưu thay đổi vào cơ sở dữ liệu
                }
            }
            throw new NotImplementedException();
        }

        // Action để hiển thị danh sách thanh toán
        public IActionResult List()
        {
            var payments = _context.Payments.OrderByDescending(p => p.CreatedDate).ToList();
            return View(payments);
        }


    }
}