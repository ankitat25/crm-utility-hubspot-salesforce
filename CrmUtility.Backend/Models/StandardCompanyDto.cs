namespace CrmUtility.Backend.Models
{
    public class StandardCompanyDto
    {
        // For reading / updating – not required on create
        public string? Id { get; set; }

        // Metadata – not required in request body
        public string? Crm { get; set; }
        public string? Type { get; set; }

        // Main business fields
        public string? Name { get; set; }
        public string? Domain { get; set; }

        // Extra info when reading
        public string? Owner { get; set; }
    }
}
