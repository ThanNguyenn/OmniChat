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
        //public const string ZaloWebhook = Base + "/zalo";
       // FacebookWebhook
        public const string FacebookWebhook = Base + "/facebook";

        public const string InstagramWebhook = Base + "/instagram";
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

    public static class SupportStaffMessageEndPoint
    {
        public const string SupportStaffMessage = ApiV1 + "/support-staff-message";
        public const string GetAllPagingByStaffId = SupportStaffMessage + "/paging";
        public const string SendZaloMessage = SupportStaffMessage + "/send-zalo";
        public const string SendFacebookMessage = SupportStaffMessage + "/send-facebook";
        public const string SendInstagramMessage = SupportStaffMessage + "/send-instagram";
        public const string UpdateStatusToSent = SupportStaffMessage + "/{id}/status/sent";
    }

    public static class SupportConversationEndPoint
    {
        public const string SupportConversation = ApiV1 + "/support-conversation";
        public const string GetById = SupportConversation + "/{id}";
        public const string GetAllPagingByCustomerName = SupportConversation + "/paging";
    }

    public static class Auth
    {
        public const string Base = ApiV1 + "/auth";

        public const string Login = "login";

        public const string RefreshToken = "refresh-token";

        public const string ChangePassword = "change-password";
    }

    public static class Account 
    {
        public const string Base = ApiV1 + "/accounts";
        public const string Create = "create";

    }
}
