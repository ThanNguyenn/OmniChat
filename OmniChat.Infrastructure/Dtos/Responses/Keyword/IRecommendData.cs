using OmniChat.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OmniChat.Infrastructure.Dtos.Responses.Keyword
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
    [JsonDerivedType(typeof(SearchOrderHistoryRecommendData), "order")]
    [JsonDerivedType(typeof(SearchProductRecommendData), "product")]
    [JsonDerivedType(typeof(SearchCustomerInfoRecommendData), "customer")]
    public interface IRecommendData
    {
      
    }
    public class SearchOrderHistoryRecommendData : IRecommendData
    {
        public Guid? OrderId { get; set; }
        public string? OrderCode { get; set; }
        public double? TotalAmount { get; set; }
        public string? OrderName { get; set; }
        public OrderStatus? OrderStatus { get; set; }
        public DeliveryStatus? DeliveryStatus { get; set; }
        public string? CustomerName { get; set; }
    }

    public class SearchProductRecommendData : IRecommendData
    {
        public Guid? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductImageUrl { get; set; }
    }

    public class SearchCustomerInfoRecommendData : IRecommendData
    {
        public string? CustomerName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerAddress { get; set; }
    }

}
