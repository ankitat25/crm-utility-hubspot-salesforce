using System.ComponentModel.DataAnnotations;

namespace CrmUtility.Backend.Models
{
    public class StandardContactDto
    {
       
        public string? Id { get; set; }

        [Required]
        public string Crm { get; set; }

        [Required]
        public string Type { get; set; }

        public string? FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Company { get; set; }


        public string? Owner { get; set; }
    }
}
