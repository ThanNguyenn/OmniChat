    using Amazon.S3;
    using Amazon.S3.Model;
    using AutoMapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using OmniChat.Application.Services.Interface;
    using OmniChat.Infrastructure.Exceptions;
    using OmniChat.Infrastructure.Metadatas;
    using OmniChat.Infrastructure.Models;
    using OmniChat.Infrastructure.Persistence;
    using OmniChat.Infrastructure.Repositories.Interfaces;
    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.Formats.Webp;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Threading.Tasks;

    namespace OmniChat.Application.Services.Implements;

    public class R2StorageService : BaseService<R2StorageService>, IR2StorageService
    {
        private readonly string _bucketName;
        private readonly string _publicUrl;
        private readonly IAmazonS3 _s3Client;

        private readonly Dictionary<string, string> _defaultImages = new()
        {
            { "products", "https://pub-28eb3560d5b74d478da589a1c3dd7e34.r2.dev/products/default_product.webp" },
            { "staff", "https://pub-28eb3560d5b74d478da589a1c3dd7e34.r2.dev/staffs/default_staff.webp" },
        };

        public R2StorageService(
        IUnitOfWork<OmniChatDbContext> unitOfWork,
        ILogger<R2StorageService> logger,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
        IAmazonS3 s3Client,
        R2Settings settings
        ) : base(unitOfWork, logger, mapper, httpContextAccessor)
        {
            _s3Client = s3Client;
            _bucketName = settings.BucketName;
            _publicUrl = settings.PublicUrl.TrimEnd('/');
        }

        private static readonly Configuration CustomImageConfiguration = CreateCustomConfiguration();

        private static Configuration CreateCustomConfiguration()
        {
            var config = Configuration.Default.Clone();
            config.Configure(new HeyRed.ImageSharp.Heif.Formats.Heif.HeifConfigurationModule());
            return config;
        }

        public async Task<bool> DeleteImageByRelatedIdAsync(string category, Guid relatedId)
        {
            if (string.IsNullOrWhiteSpace(category))
                throw new BusinessException($"Unsupported category '{category}'.");

            category = category.ToLowerInvariant().Trim('/');
            string? fileUrl = null;

            await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                switch (category)
                {
                    case "products":
                        var productRepo = _unitOfWork.GetRepository<Product>();
                        var product = await productRepo.GetByIdAsync(relatedId);
                        if (product == null) throw new BusinessException("Product not found.");

                        fileUrl = product.ImageUrl;
                        product.ImageUrl = _defaultImages["products"];
                        productRepo.Update(product);
                        break;
                    case "staffs":
                        var staffRepo = _unitOfWork.GetRepository<Staff>();
                        var staff = await staffRepo.GetQueryable(s => s.Id == relatedId, q => q.Include(s => s.Account)).FirstOrDefaultAsync();

                        if (staff == null) throw new BusinessException("Staff not found.");

                        fileUrl = staff.Account?.AvatarUrl;
                        staff.Account.AvatarUrl = _defaultImages["staff"];
                        break;
                    default:
                        throw new BusinessException($"Unsupported category '{category}'.");
                }
            });


            if (!string.IsNullOrWhiteSpace(fileUrl) && fileUrl.StartsWith(_publicUrl, StringComparison.OrdinalIgnoreCase))
            {
                var objectKey = fileUrl.Replace($"{_publicUrl}/", "", StringComparison.OrdinalIgnoreCase);

                try
                {
                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = objectKey
                    });

                    _logger.LogInformation("Deleted file from R2: {Key}", objectKey);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete file from R2: {Key}", objectKey);
                }
            }
            return true;
        }

        public async Task<bool> UploadImageAsync(Stream fileStream, string fileName, string category, Guid? relatedId = null)
        {
            if (fileStream == null || string.IsNullOrWhiteSpace(fileName))
                throw new BusinessException("Invalid file upload parameters.");

            category = category.ToLowerInvariant().Trim('/');

            await using var optimizedStream = await OptimizeImageAsync(fileStream);

            string newFileName = Path.GetFileNameWithoutExtension(fileName) + ".webp";
            string objectKey = $"{category}/{relatedId}_{Guid.NewGuid()}_{newFileName}";

            string fileUrl = $"{_publicUrl}/{objectKey}";

            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = objectKey,
                InputStream = optimizedStream,
                ContentType = "image/webp",
                DisablePayloadSigning = true,
                DisableDefaultChecksumValidation = true,
            });

            await _unitOfWork.ProcessInTransactionAsync(async () =>
            {
                try
                {
                    switch (category)
                    {
                        case "products" when relatedId.HasValue:
                            var productRepo = _unitOfWork.GetRepository<Product>();
                            var product = await productRepo.GetByIdAsync(relatedId.Value);

                            if (product is null)
                                throw new BusinessException("Product not found");

                            product.ImageUrl = fileUrl;
                            productRepo.Update(product);
                            break;

                        case "staffs" when relatedId.HasValue:
                            var staffRepo = _unitOfWork.GetRepository<Staff>();
                            var staff = await staffRepo.GetQueryable(s => s.Id == relatedId.Value, q => q.Include(s => s.Account)).FirstOrDefaultAsync();
                            if (staff is null || staff.Account is null)
                                throw new BusinessException("Staff or associated account not found");
                            staff.Account.AvatarUrl = fileUrl;
                            break;

                        default:
                            throw new BusinessException($"Unsupported or missing category: {category}");
                    }
                }
                catch
                {
                    await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = objectKey
                    });

                    throw;
                }
            });

            return true;
        }

        private static async Task<Stream> OptimizeImageAsync(Stream inputStream)
        {
            inputStream.Position = 0;

            var options = new DecoderOptions
            {
                Configuration = CustomImageConfiguration
            };

            using var image = await Image.LoadAsync(options, inputStream);

            image.Metadata.ExifProfile = null;
            image.Metadata.IccProfile = null;
            image.Metadata.XmpProfile = null;

            var ms = new MemoryStream();

            await image.SaveAsync(ms, new WebpEncoder
            {
                Quality = 80
            });

            ms.Position = 0;
            return ms;
        }

        public async Task<bool> UploadUpdatedImageAsync(
            Stream fileStream,
            string fileName,
            string category,
            Guid? relatedId = null)
        {
            category = category.ToLowerInvariant().Trim('/');
            string? oldFileUrl = null;

            if (relatedId.HasValue)
            {
                switch (category)
                {
                    case "products":
                        var product = await _unitOfWork
                            .GetRepository<Product>()
                            .GetByIdAsync(relatedId.Value);

                        oldFileUrl = product?.ImageUrl;
                        break;

                    case "staffs":
                        var staff = await _unitOfWork
                            .GetRepository<Staff>()
                            .GetQueryable(s => s.Id == relatedId.Value,
                                q => q.Include(s => s.Account))
                            .FirstOrDefaultAsync();

                        oldFileUrl = staff?.Account?.AvatarUrl;
                        break;

                    default:
                        throw new BusinessException($"Unsupported category '{category}'.");
                }
            }

            var uploadSuccess = await UploadImageAsync(fileStream, fileName, category, relatedId);

            if (uploadSuccess &&
                !string.IsNullOrWhiteSpace(oldFileUrl) &&
                oldFileUrl.StartsWith(_publicUrl, StringComparison.OrdinalIgnoreCase) &&
                !_defaultImages.ContainsValue(oldFileUrl))
            {
                var objectKey = oldFileUrl.Replace($"{_publicUrl}/", "", StringComparison.OrdinalIgnoreCase);

                await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey
                });
            }

            return uploadSuccess;
        }
    }
