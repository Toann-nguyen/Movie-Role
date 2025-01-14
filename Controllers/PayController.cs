using MvcMovie.ViewModels.VNPay;
using MvcMovie.Services;
using Microsoft.AspNetCore.Mvc;

namespace MvcMovie.Controllers
{
    public class PayController : Controller
    {
        private readonly IVNPayService _vnPayService;
        public PayController(IVNPayService vnPayService)
        {
            _vnPayService = vnPayService;
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
                    string cleanedAmount = request.Amount.Replace(".", "");

                    // Đảm bảo cleanedAmount là số hợp lệ
                    if (!double.TryParse(cleanedAmount, out double amount))
                    {
                        ModelState.AddModelError("Amount", "Số tiền không hợp lệ");
                        return View(request);
                    }

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

            return RedirectToAction(nameof(PaymentFail));
        }
    }
}