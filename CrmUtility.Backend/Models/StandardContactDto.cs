namespace CrmUtility.Backend.Models
{
    public class StandardContactDto
    {
        // For updates/reads – can be null on create, set from route for updates
        public string? Id { get; set; }

        // Metadata – not required in requests
        public string? Crm { get; set; }
        public string? Type { get; set; }

        // Contact fields – all optional for update
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }

        // Extra info for reading/mapping
        public string? Company { get; set; }
        public string? Owner { get; set; }
    }
}
