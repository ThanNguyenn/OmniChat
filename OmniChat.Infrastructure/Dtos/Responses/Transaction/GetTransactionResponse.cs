using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Transaction;

public class GetTransactionResponse
{
        public Guid Id { get; set; }

        public double Amount { get; set; }

        public DateTime? CreateDate { get; set; }

        public TransactionType TransactionType { get; set; }
}

