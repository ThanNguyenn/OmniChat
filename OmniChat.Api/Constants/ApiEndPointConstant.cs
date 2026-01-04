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

        public const string Zalo = Base + "/zalo";
        public const string Facebook = Base + "/facebook";
    }
}
