using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmniChat.Application.Services.Interface;

public interface IR2StorageService
{
    Task<bool> UploadImageAsync(Stream fileStream, string fileName, string category, Guid? relatedId = null);
    Task<bool> DeleteImageByRelatedIdAsync(string category, Guid relatedId);
}