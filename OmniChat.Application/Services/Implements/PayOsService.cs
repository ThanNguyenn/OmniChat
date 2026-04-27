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
               
                var verifiedData = _payOS.verifyPaymentWebhookData(body);

                long payOsOrderCode = verifiedData.orderCode;

                var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
                var invoice = await invoiceRepo.SingleOrDefaultAsync(
                    predicate: x => x.InvoiceCode == payOsOrderCode, // Tìm theo InvoiceCode
                    include: x => x.Include(i => i.Orders).Include(i => i.CustomerProfile)
                );

                if (invoice == null) return true;
                if (invoice.InvoiceStatus == InvoiceStatus.Completed) return true;

                if (body.code == "00")
                {
                    invoice.InvoiceMethod = InvoiceMethod.BankTransfer;
                    invoice.InvoiceStatus = InvoiceStatus.Completed;
                    invoice.CompletedDate = DateTime.UtcNow;

                    foreach (var order in invoice.Orders)
                    {
                        order.Status = OrderStatus.Completed;
                        await _mailService.SendEmailAsync(new MailContent
                        {
                            To = order.CustomerProfile.Email,
                            Subject = "Order Completed",
                            Body = $"Your order {order.Name} has been completed. Thank you for shopping with us!"
                        });
                    }
                    invoiceRepo.Update(invoice);
                    await _unitOfWork.CommitAsync();
                    return true;
                }
                else
                {
                    await _mailService.SendEmailAsync(new MailContent
                    {
                        To = invoice.CustomerProfile.Email,
                        Subject = "Payment Failed",
                        Body = $"Your payment for invoice {invoice.Id} has failed. Please try again or contact support."
                    });
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
