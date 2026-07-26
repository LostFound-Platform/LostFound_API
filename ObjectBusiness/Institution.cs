using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class Institution
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InstitutionId { get; set; }
        [Required(ErrorMessage = "Institution name cannot be blank")]
        public string InstitutionName { get; set; }
        [Required]
        public string InstitutionAddress { get; set; }
        [Required]
        public string InstitutionCity { get; set; }
        [Required]
        public string InstitutionState { get; set; }
        public string? InstitutionPhone { get; set; }
        public string? InstitutionWebsite { get; set; }
        public string? InstitutionLogo { get; set; }
        public string? InstitutionDescription { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // IFormFile is used for only upload img from frontend
        [NotMapped]
        [JsonIgnore]
        public IFormFile? LogoFromFrontend { get; set; }

        // Buil relationships
        public ICollection<Users>? Users { get; set; }
    }
}
