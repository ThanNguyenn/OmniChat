using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Mail;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;



namespace OmniChat.Application.Services.Implements
{
    public class PayOsService : BaseService<PayOsService>, IPayOsService
    {
        private readonly IMailService _mailService;
        private readonly PayOS _payOS;
        public PayOsService(IUnitOfWork<OmniChatDbContext> unitOfWork,
              ILogger<PayOsService> logger,
              IMapper mapper,
              IHttpContextAccessor httpContextAccessor,
                IMailService mailService,
              IConfiguration configuration
              ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            var settings = configuration.GetSection("PayOs");


            _payOS = new PayOS(
                settings["ClientID"],
                settings["APIKey"],
                settings["ChecksumKey"]
            );
            _mailService = mailService;
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

            long payOsOrderCode = invoice.InvoiceCode;

            var items = invoice.Orders.Select(order => new ItemData(
                name: order.Name,
                quantity: 1,
                price: (int)order.TotalAmount
            )).ToList();


            var paymentData = new PaymentData(
              orderCode: payOsOrderCode,
                amount: (int)remainingAmount,
                 description: $"Payment for{invoice.CreateAt:dd-MM}",
                     cancelUrl: "https://omni-chat-web.vercel.app/payment?status=fail",
                         returnUrl: "https://omni-chat-web.vercel.app/payment?status=success",
                            items: items
             );


            var paymentLink = await _payOS.createPaymentLink(paymentData);

            // gửi link này về email cho khách hàng qua email address

            var customer = await _unitOfWork.GetRepository<CustomerProfile>().SingleOrDefaultAsync(predicate: x => x.Id == customerId);


            var mailContent = new MailContent
            {
                To = customer.Email,
                Subject = "Payment Link",
                Body = $"Please click the following link to complete your payment: {paymentLink.checkoutUrl}"
            };

            await _mailService.SendEmailAsync(mailContent);

            return paymentLink.checkoutUrl;
        }


        public async Task<bool> HandleWebhookAsync(WebhookType body)
        {
            try
            {
                _logger.LogInformation(">>> Processing Webhook Data...");
                var verifiedData = _payOS.verifyPaymentWebhookData(body);
                long payOsOrderCode = verifiedData.orderCode;

                var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
                var invoice = await invoiceRepo.SingleOrDefaultAsync(
                    predicate: x => x.InvoiceCode == payOsOrderCode,
                    include: x => x.Include(i => i.Orders).Include(i => i.CustomerProfile) // Đã load CustomerProfile ở đây rồi
                );

                // 1. Kiểm tra NULL trước khi làm bất cứ việc gì khác
                if (invoice == null)
                {
                    _logger.LogWarning($">>> Invoice with Code {payOsOrderCode} not found.");
                    return true;
                }

                if (invoice.InvoiceStatus == InvoiceStatus.Completed) return true;

                // 2. Lấy thông tin email từ CustomerProfile đã được Include (không cần query thêm)
                var customerEmail = invoice.CustomerProfile?.Email;

                if (body.code == "00")
                {
                    invoice.InvoiceMethod = InvoiceMethod.BankTransfer;
                    invoice.InvoiceStatus = InvoiceStatus.Completed;
                    invoice.CompletedDate = DateTime.UtcNow;

                    foreach (var order in invoice.Orders)
                    {
                        order.Status = OrderStatus.Completed;

                        if (!string.IsNullOrEmpty(customerEmail))
                        {
                            await _mailService.SendEmailAsync(new MailContent
                            {
                                To = customerEmail,
                                Subject = "Order Completed",
                                Body = $"Your order {order.Name} has been completed. Thank you!"
                            });
                        }
                    }
                    invoiceRepo.Update(invoice);
                    await _unitOfWork.CommitAsync();
                    return true;
                }
                else
                {
                    if (!string.IsNullOrEmpty(customerEmail))
                    {
                        await _mailService.SendEmailAsync(new MailContent
                        { To = customerEmail,
                          Subject = "Order Failed",
                          Body = $"Your invoice has Payment failed. Please try again later."
                        });
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xử lý Webhook PayOS");
                return false;
            }
        }
    }
}
