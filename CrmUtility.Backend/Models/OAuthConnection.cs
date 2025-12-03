namespace CrmUtility.Backend.Models
{
    public enum CrmType
    { 
    HubSpot =1,
    Salesforce =2
    }
    public class OAuthConnection
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public CrmType Crm { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
        public DateTime ExpiresAtUtc { get; set; }

        public string? HubSpotPortalId { get; set; }
        public string? InstanceUrl { get; set; }


        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    }
}
