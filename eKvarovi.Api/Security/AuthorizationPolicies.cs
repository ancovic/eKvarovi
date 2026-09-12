namespace eKvarovi.Api.Security;

public static class AuthorizationPolicies
{
    public const string AdminOnly = "AdminOnly";
    public const string Management = "Management";
    public const string ReporterOnly = "ReporterOnly";
    public const string TechnicianOnly = "TechnicianOnly";
}