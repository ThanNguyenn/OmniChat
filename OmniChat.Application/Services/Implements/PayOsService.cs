using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Net.payOS;
using Net.payOS.Types;
using OmniChat.Application.Services.Interface;
using OmniChat.Infrastructure.Dtos.Requests.Mail;
using OmniChat.Infrastructure.Exceptions;
using OmniChat.Infrastructure.Models;
using OmniChat.Infrastructure.Persistence;
using OmniChat.Infrastructure.Repositories.Interfaces;
using Transaction = OmniChat.Infrastructure.Models.Transaction;



namespace OmniChat.Application.Services.Implements
{
    public class PayOsService : BaseService<PayOsService>, IPayOsService
    {
        private readonly IMailService _mailService;
        private readonly IWalletService _walletService;
        private readonly PayOS _payOS;
        public PayOsService(IUnitOfWork<OmniChatDbContext> unitOfWork,
              ILogger<PayOsService> logger,
              IMapper mapper,
              IHttpContextAccessor httpContextAccessor,
                IMailService mailService,
              IConfiguration configuration,
              IWalletService walletService
              ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            var settings = configuration.GetSection("PayOs");


            _payOS = new PayOS(
                settings["ClientID"],
                settings["APIKey"],
                settings["ChecksumKey"]
            );
            _mailService = mailService;
            _walletService = walletService;
        }

        public async Task<string> CreatePaymentLinkAsync(Guid id)
        {
            var InvoiceRepo = _unitOfWork.GetRepository<Invoice>();


            var invoice = await InvoiceRepo.SingleOrDefaultAsync(
                predicate: x => x.Id == id &&
                (x.InvoiceStatus == InvoiceStatus.Pending || x.InvoiceStatus == InvoiceStatus.PartialPaid),
                include: x => x.Include(x => x.Orders).Include(x => x.CustomerProfile)
            );

            if (invoice == null) throw new Exception("Invoice not found");

            //var total = invoice.Total;
            var paid = invoice.PaidAmount;
            //var remainingAmount = total - paid;

            long payOsOrderCode = invoice.InvoiceCode;

            var items = invoice.Orders.Select(order => new ItemData(
                name: order.Name,
                quantity: 1,
                price: (int)paid
            )).ToList();


            var paymentData = new PaymentData(
              orderCode: payOsOrderCode,
                //amount: (int)remainingAmount,
                 amount: (int)paid,
                 description: $"Payment for {invoice.CreateAt:dd-MM}",
                     cancelUrl: "https://omnichat.click/api/v1/invoices/confirm-payment",
                         returnUrl: "https://omnichat.click/api/v1/invoices/confirm-payment",
                            items: items
             );
            _logger.LogInformation($"[PayOsService]: {paymentData}");

            var paymentLink = await _payOS.createPaymentLink(paymentData);

            // gửi link này về email cho khách hàng qua email address

            var customer = await _unitOfWork.GetRepository<CustomerProfile>().SingleOrDefaultAsync(predicate: x => x.Id == invoice.CustomerId);
            if (customer == null) { 
            throw new NotFoundException("Customer not found");
            }

            //var mailContent = new MailContent
            //{
            //    To = customer.Email,
            //    Subject = "Đường dẫn thanh toán",
            //    Body = $"Vui lòng nhấp vào liên kết sau để hoàn tất thanh toán của bạn: {paymentLink.checkoutUrl}" 
            //};

                //await _mailService.SendEmailAsync(mailContent);

            string pathLink = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MailTemplate", "PaymentLinkTemplate.html");
            if (File.Exists(pathLink))
            {
                string bodyLink = await File.ReadAllTextAsync(pathLink);

                bodyLink = bodyLink.Replace("{{CustomerName}}", customer.CustomerName ?? "Quý khách")
                                   .Replace("{{CheckoutUrl}}", paymentLink.checkoutUrl);

                var mailContent = new MailContent
                {
                    To = customer.Email,
                    Subject = "Đường dẫn thanh toán đơn hàng - BaoHanCompany",
                    Body = bodyLink
                };

                await _mailService.SendEmailAsync(mailContent);
            }
            return paymentLink.checkoutUrl;
        }


        public async Task<bool> HandleWebhookAsync(WebhookType body)
        {
            try
            {
                _logger.LogInformation(">>> Processing Webhook Data...");
                var verifiedData = _payOS.verifyPaymentWebhookData(body);
                long payOsOrderCode = verifiedData.orderCode;

                var orderRepo = _unitOfWork.GetRepository<Order>();
                var invoiceRepo = _unitOfWork.GetRepository<Invoice>();
                var walletRepo = _unitOfWork.GetRepository<Wallet>();
                var transactionRepo = _unitOfWork.GetRepository<Transaction>();
                var allocationRepo = _unitOfWork.GetRepository<Allocation>();


                var invoice = await invoiceRepo.SingleOrDefaultAsync(
                    predicate: x => x.InvoiceCode == payOsOrderCode,
                    include: x => 
                    x.Include(i => i.Orders)
                    .Include(i => i.CustomerProfile)
                    .ThenInclude(cp => cp.Wallet)
                );

               
                if (invoice == null)
                {
                    _logger.LogWarning($">>> Invoice with Code {payOsOrderCode} not found.");
                    return true;
                }

                if (invoice.InvoiceStatus == InvoiceStatus.Completed) return true;

                var wallet = invoice.CustomerProfile?.Wallet;
                if (wallet == null)
                {
                    _logger.LogError(">>> Customer wallet not found for invoice.");
                    return false;
                }


                var customerEmail = invoice.CustomerProfile?.Email;

                if (body.code == "00")
                {

                    var successfulAllocationTrans = new Transaction
                    {
                        WalletId = wallet.Id,
                        Amount = verifiedData.amount,
                        TransactionType = TransactionType.AllocateForInvoice,
                    };
                        
                    wallet.Amount += verifiedData.amount;


                    var allocation = new Allocation
                    {
                        WalletId = wallet.Id,
                        InvoiceId = invoice.Id,
                        Amount = verifiedData.amount,
                        AllocationType = AllocationType.Payment,
                       Transaction = successfulAllocationTrans
                    };

                    await transactionRepo.InsertAsync(successfulAllocationTrans);

                    await allocationRepo.InsertAsync(allocation);

                    wallet.Amount -= verifiedData.amount;
                    wallet.UpdatedDate = DateTime.UtcNow;


                    invoice.InvoiceMethod = InvoiceMethod.BankTransfer;
                    invoice.InvoiceStatus = InvoiceStatus.Completed;
                    invoice.PaidAmount += verifiedData.amount;
                    invoice.CompletedDate = DateTime.UtcNow;

                    foreach (var order in invoice.Orders)
                    {
                        order.Status = OrderStatus.Completed;

                        if (!string.IsNullOrEmpty(customerEmail))
                        {
                            //await _mailService.SendEmailAsync(new MailContent
                            //{
                            //    To = customerEmail,
                            //    Subject = "Hoàn thành Đơn hàng",
                            //    Body = $"Đơn hàng của bạn {order.Name} đã được hoàn thành. Cảm ơn bạn! \n"
                            //});

                            string pathCompleted = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MailTemplate", "OrderCompletedTemplate.html");
                            string bodyCompleted = await File.ReadAllTextAsync(pathCompleted);

                            bodyCompleted = bodyCompleted.Replace("{{OrderName}}", order.Name);

                            await _mailService.SendEmailAsync(new MailContent
                            {
                                To = customerEmail,
                                Subject = "Hoàn thành Đơn hàng - BaoHanCompany",
                                Body = bodyCompleted
                            });
                        }
                    }
                    walletRepo.Update(wallet);
                    orderRepo.UpdateRange(invoice.Orders);
                    invoiceRepo.Update(invoice);
                    await _unitOfWork.CommitAsync();
                    return true;
                }
                else
                {

                    if (!string.IsNullOrEmpty(customerEmail))
                    {
                        //await _mailService.SendEmailAsync(new MailContent
                        //{ To = customerEmail,
                        //  Subject = "Đơn hàng thất bại",
                        //  Body = $"Hóa đơn của bạn đã thất bại trong việc thanh toán. Vui lòng thử lại sau."
                        //});

                        string pathFailed = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MailTemplate", "PaymentFailedTemplate.html");
                        string bodyFailed = await File.ReadAllTextAsync(pathFailed);

                        await _mailService.SendEmailAsync(new MailContent
                        {
                            To = customerEmail,
                            Subject = "Đơn hàng thất bại - Thanh toán không thành công",
                            Body = bodyFailed
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
