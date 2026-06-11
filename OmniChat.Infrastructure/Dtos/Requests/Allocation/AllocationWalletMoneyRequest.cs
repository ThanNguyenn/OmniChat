using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Requests.Allocation
{
    public class AllocationWalletMoneyRequest
    {
        public Guid WalletId { get; set; }

        [RegularExpression(@"^(?!(?:0|0\.00)$)\d+(\.\d+)?$", ErrorMessage = "Số tiền khấu trừ phải là số dương và lớn hơn 0.")]
        public double deductedAmount { get; set; }
    }
}
