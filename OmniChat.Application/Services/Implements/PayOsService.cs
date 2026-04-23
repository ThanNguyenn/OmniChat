using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;



namespace OmniChat.Application.Services.Implements
{
    public class PayOsService : BaseService<PayOsService> ,IPayOsService
    {
        private readonly PayOS _payOS;
        public PayOsService(IUnitOfWork<OmniChatDbContext> unitOfWork,
              ILogger<PayOsService> logger,
              IMapper mapper,
              IHttpContextAccessor httpContextAccessor,
              IConfiguration configuration
              ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            var settings = configuration.GetSection("PayOs");

         
            _payOS = new PayOS(
                settings["ClientID"],
                settings["APIKey"],
                settings["ChecksumKey"]
            );
        }

        public async Task<string> CreatePaymentLinkAsync(Guid customerId)
        {
            var InvoiceRepo = _unitOfWork.GetRepository<Invoice>();


            var invoice = await InvoiceRepo.SingleOrDefaultAsync(
                predicate: x => x.CustomerId == customerId,
                include: x => x.Include(x => x.Orders)
            );

            if (invoice == null) throw new Exception("Invoice not found");

            var total = invoice.Total;
            var deducted = invoice.DeductedAmount;
            var paid = invoice.PaidAmount;
            var remainingAmount = total - deducted - paid;

            long payOsOrderCode = long.Parse(DateTimeOffset.Now.ToString("MMddHHmmss"));

            var items = invoice.Orders.Select(order => new ItemData(
                name: order.Name,
                quantity: 1,
                price: (int)order.TotalAmount
            )).ToList();


            var paymentData = new PaymentData(
              orderCode: payOsOrderCode,
                amount: (int)remainingAmount,
                 description: $"INV|{invoice.Id}", 
                     cancelUrl: "https://yourapp.com/payment-cancel",
                         returnUrl: "https://yourapp.com/payment-return",
                            items: items
             );


            var paymentLink = await _payOS.createPaymentLink(paymentData);

            // gửi link này về email cho khách hàng qua email address

            return paymentLink.checkoutUrl;
        }


        public async Task<bool> HandleWebhookAsync(WebhookType body)
        {
            try
            {
                // Bước 1: Xác thực chữ ký để đảm bảo dữ liệu chuẩn từ PayOS
                // verifiedData sẽ chứa thông tin giao dịch đã được giải mã
                var verifiedData = _payOS.verifyPaymentWebhookData(body);

                // Bước 2: Kiểm tra description để lấy InvoiceId
                string description = verifiedData.description;
                if (string.IsNullOrEmpty(description) || !description.StartsWith("INV|"))
                    return false;

                string invoiceIdStr = description.Split('|')[1];
                if (!Guid.TryParse(invoiceIdStr, out Guid invoiceId))
                    return false;

                // Bước 3: Truy vấn Invoice và cập nhật
                var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
                var invoice = await invoiceRepo.SingleOrDefaultAsync(
                    predicate: x => x.Id == invoiceId,
                    include: x => x.Include(i => i.Orders)
                );

                if (invoice == null || invoice.InvoiceStatus == InvoiceStatus.Completed)
                    return true; // Trả về true để PayOS không bắn lại nữa nếu đã xử lý xong

                // Bước 4: Kiểm tra mã thành công (code "00")
                if (body.code == "00")
                {
                    //invoice.PaidAmount += verifiedData.amount;
                    invoice.InvoiceMethod = InvoiceMethod.BankTransfer;

                    //double totalNeedToPay = invoice.Total - invoice.DeductedAmount;

                    //if (invoice.PaidAmount >= totalNeedToPay)
                    //{
                        invoice.InvoiceStatus = InvoiceStatus.Completed;
                        invoice.CompletedDate = DateTime.UtcNow;

                        foreach (var order in invoice.Orders)
                        {
                            order.Status = OrderStatus.Completed;
                        }
                    //}
                    //else
                    //{
                    //    invoice.InvoiceStatus = InvoiceStatus.PartialPaid;
                    //}

                    _unitOfWork.GetRepository<Invoice>().Update(invoice);
                     await _unitOfWork.CommitAsync();
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý Webhook PayOS");
                return false;
            }
        }
    }
}
