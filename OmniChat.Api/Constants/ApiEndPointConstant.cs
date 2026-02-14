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

        public const string GetCustomerByEmailOrPhone =
      CustomerProfile + "/search";

        public const string UpdateCustomerProfile =
           CustomerProfile + "/{customerId}";
    }

    public static class CustomerServiceMergeEndpoint
    {
        public const string CustomerMerge =
        ApiV1 + "/customer-profile/merge";
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
        public const string SupportConversations = ApiV1 + "/support-conversations";
        public const string GetById = SupportConversation + "/{id}";
        public const string GetAllPagingByCustomerName = SupportConversation + "/paging";
        public const string StaffPendingSidebar = SupportConversations + "/staff/{staffId}/pending";
        public const string GetConversationDetail = SupportConversations + "/{conversationId}";
        public const string CustomerConversationHistory = SupportConversations + "/customer/{customerId}/history";
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

    public static class ClaimTypeEndPoint
    {
        public const string ClaimType = ApiV1 + "/claim-type";

        public const string Create = ClaimType + "/create";
        public const string GetAll = ClaimType + "/get-all";
        public const string Update = ClaimType + "/{id}/update";
        public const string Delete = ClaimType + "/{id}/delete";
    }
    public static class Staff
    {
        public const string Base = ApiV1 + "/staff";
        public const string Create = "create";  
        public const string Update = "update/{id}";
        public const string Delete = "delete/{id}";
        public const string GetAll = "get";
    }

    public static class Product
    {
        public const string Base = ApiV1 + "/products";
        public const string Create = "create";  
        public const string Update = "update/{id}";
        public const string Delete = "delete/{id}";
        public const string GetAll = "get";
        public const string GetById = "get/{id}";
        public const string AddStock = "add-stock";
    }

    public static class ClaimEndPoint
    {
        public const string Base = "api/v1/claims";

        public const string GetAll = Base;
        public const string Create = Base;
        public const string Update = Base + "/{id}";
        public const string Approve = Base + "/{id}/approve";
        public const string Reject = Base + "/{id}/reject";
    }


}
