namespace CrmUtility.Backend.Models
{
    public class StandardDealDto
    {
        public string? Crm { get; set; }  
        public string? Type { get; set; }  
        public string? Id { get; set; }   

        public string DealName { get; set; } = "";
        public string Stage { get; set; } = "";
        public string Pipeline { get; set; } = "";
        public decimal? Amount { get; set; }

        public string? Owner { get; set; }  
    }
}
