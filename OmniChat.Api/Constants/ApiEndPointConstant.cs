using OmniChat.Infrastructure.Models;

namespace OmniChat.Api.Constants;

public class ApiEndPointConstant
{
    static ApiEndPointConstant()
    { }

    public const string Root = "/api";
    public const string VersionV1 = "/v1";

    public const string ApiV1 = Root + VersionV1;

    public static class Webhooks
    {
        public const string Base = ApiV1 + "/webhooks";

       //  ZaloWebhook
        public const string ZaloWebhook = Base + "/zalo";
       // FacebookWebhook
        public const string FacebookWebhook = Base + "/facebook";
    }

    public static class CustomerMessageEndPoint
    {
        public const string CustomerMessage = ApiV1 + "/customer-message";
        public const string GetAllPagingByCustomerId =
            CustomerMessage + "/paging";
    }

    public static class ProviderEndPoint
    {
        public const string Provider = ApiV1 + "/provider";

        // Create Provider 
        public const string CreateProvider = Provider + "/create";

        // get paging by provider Name 
        public const string GetAllPagingByproviderName =
            Provider + "/paging";
    }

    public static class CustomerProfileEndPoint
    {
        public const string CustomerProfile = ApiV1 + "/customer-profile";

        public const string GetAllCustomerProfileByCustomerName = CustomerProfile + "/paging";
    }
}
