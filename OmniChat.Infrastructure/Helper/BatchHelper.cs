using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Helper
{
    public class BatchHelper
    {
        public static string GenerateBatchCode(DateTime expiryDate, int suffix)
        {
            var dateStr = expiryDate.ToString("yyyyMMdd");

            return $"LOT{dateStr}{suffix:D2}";
        }
    }
}
