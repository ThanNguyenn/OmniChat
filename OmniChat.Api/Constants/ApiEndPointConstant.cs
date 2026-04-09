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

        public const string GetCustomerProfileByConversationId = CustomerProfile + "/{conversationId}";

        public const string CustomerMerge = ApiV1 + "/customer-profile/merge";

        public const string EnrichCustomerProfile =
           CustomerProfile + "/enrich";
    }

    public static class StaffPerformanceEndPoint
    {
        public const string StaffPerformance = ApiV1 + "/staff-performance";
        public const string InitializePerformanceForStaff = StaffPerformance + "/initialize-performance/{staffId}";
        public const string GetTotalAverage = StaffPerformance + "/total-average";
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

    public static class SupportTaskEndPoint
    {
        public const string SupportTask = ApiV1 + "/support-task";
        public const string CompleteSupportTask = SupportTask + "/{id}/complete-task";
        public const string GetSupportTaskByConversationId = SupportTask + "/conversation/{conversationId}";
        public const string GetTaskIntentDashboard = SupportTask + "/dashboard";
    }

    public static class SupportConversationEndPoint
    {
        public const string SupportConversation = ApiV1 + "/support-conversation";
        public const string SupportConversations = ApiV1 + "/support-conversations";
        public const string GetById = SupportConversation + "/{id}";
        public const string GetAllPagingByCustomerName = SupportConversation + "/paging";
        public const string StaffPendingSidebar = SupportConversations + "/staff/{staffId}/pending";
        public const string GetConversationDetail = SupportConversations + "/{conversationId}";
        public const string GetCompletedConversationDetail = SupportConversations + "/history/{conversationId}";
        public const string CustomerCompleteConversationHistory = SupportConversations + "/customer/{customerId}/complete-history";
        public const string CompleteConversation = SupportConversation + "/{id}/complete";
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
        public const string AssignIntent = "{id}/assign-intent";
        public const string UnassignIntent = "{id}/unassign-intent";
        public const string StaffDashboard = "{id}/dashboard";
        public const string getStaffTasks = "{id}/tasks";
    }

    public static class Shipper
    {
        public const string Base = ApiV1 + "/shippers";

        // GET: /api/v1/shippers
        public const string GetAll = "";

        // GET: /api/v1/shippers/{id}
        public const string GetById = "{id}";

        // POST: /api/v1/shippers/{id}/assign-order
        public const string AssignOrder = "{id}/assign-order";

        // GET: /api/v1/shippers/dashboard
        public const string Dashboard = "dashboard";
    }

    public static class Product
    {
        public const string Base = ApiV1 + "/products";
        public const string Create = "create";
        public const string Update = "update/{id}";
        public const string UpdateImage = "update/{id}/image";
        public const string Delete = "delete/{id}";
        public const string GetAll = "get";
        public const string GetForCreateOrder = "get/create-order";
        public const string GetById = "get/{id}";
        public const string AddStock = "add-stock";
        public const string GetProductBatches = "{id}/batches";
        public const string Dashboard = "/dashboard";
    }

    public static class ClaimEndPoint
    {
        public const string Base = ApiV1 + "/claims";
        public const string GetPending = Base + "/pending";
        public const string GetHistory = Base + "/history";
        public const string Dashboard = Base + "/dashboard";
        public const string Create = Base;
        public const string Update = Base + "/{id}";
        public const string Approve = Base + "/{id}/approve";
        public const string Reject = Base + "/{id}/reject";
        public const string GetByStaffId = Base + "/staff/{staffId}";
        public const string ReAssign = Base + "/{conversationId}/reassign/{newStaffId}";
    }

    public static class Order
    {
        public const string Base = ApiV1 + "/orders";
        public const string Create = "create";
        public const string Update = "update/{id}";
        public const string Delete = "delete/{id}";
        public const string GetAll = "get";
        public const string GetById = "get/{id}";
        public const string GetByIdForPostSale = "get/{id}/post-sale";
        public const string GetForCreateOrder = "get/create-order";
        public const string GetByCustomerId = "customer/{customerId}/get";
        public const string CancelOrder = "{id}/cancel";
        public const string CompleteDeliveredOrder = "{id}/complete-delivery";
        public const string Dashboard = "dashboard";
        public const string Shipper = "shipper";
    }

    public static class FacebookOAuthToken
    {
        public const string Base = ApiV1 + "/facebook-token";
        public const string Create = Base;
        public const string Update = Base + "/{id}";
        public const string Delete = Base + "/{id}";
    }

    public static class InstagramOAuthToken
    {
        public const string Base = ApiV1 + "/instagram-token";
        public const string Create = Base;
        public const string Update = Base + "/{id}";
        public const string Delete = Base + "/{id}";
    }

    public static class Brand
    {
        public const string Base = ApiV1 + "/brands";
        public const string GetAll = "get";
    }

    public static class Wallet
    {
        public const string Base = ApiV1 + "/wallets";
        public const string Payment = Base + "/payment";

    }

    public static class PostSaleRequest
    {
        public const string Base = ApiV1 + "/post-sale-requests";
        public const string Create = Base + "/create";
        public const string Update = Base + "/{id}/update";
        public const string Delete = Base + "/{id}/delete";
        public const string GetAll = Base + "get";
        public const string GetById = Base + "/{id}";
        public const string Approve = Base + "/{id}/approve";
        public const string Reject = Base + "/{id}/reject";
    }


    public static class ConversationWarning
    {
        public const string Base = ApiV1 + "/conversation-warnings";
        public const string GetAll = Base + "/get";
        public const string GetById = Base + "/{id}/warning";
    }
    public static class Keyword
    {
        public const string Base = ApiV1 + "/keywords";
        public const string Create = "create";
        public const string Update = "update/{id}";
        public const string Delete = "delete/{id}";
        public const string GetAll = "get";
        public const string GetById = "get/{id}";

    }

    public static class Invoice
    {
        public const string Base = ApiV1 + "/invoices";
        public const string TotalRevenue = Base + "/total-revenue";
        public const string TotalUnpaid = Base + "/total-unpaid";

    }


    public static class IntentType
    {
        public const string Base = ApiV1 + "/intent-type";
        public const string GetAll = Base + "/gets";
    }

    public static class ChatTemplate
    {
        public const string Base = ApiV1 + "/chat-templates";
        public const string Create = Base;           
        public const string Update = Base + "/{id}"; 
        public const string Delete = Base + "/{id}"; 
        public const string GetAll = Base;           
        public const string GetById = Base + "/{id}";

    }

    public static class TaskActionEndPoint
    {
        public const string Base = ApiV1 + "/task-action";
        public const string GetAll = Base + "/get-all";
        public const string GetById = Base + "/{id}";
    }
}
