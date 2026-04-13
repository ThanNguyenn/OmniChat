using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface ISheetExportService
{
    Task<(Stream content, string filename)> ExportInvoiceToExcelAsync(Guid invoiceId, string path);
}
