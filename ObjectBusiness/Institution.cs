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
        public string InstitutionName { get; set; }
        public string InstitutionAddress { get; set; }
        public string InstitutionCity { get; set; }
        public string InstitutionState { get; set; }
        public string Password { get; set; }
        public string PickImage1 { get; set; }
        public string PickImage2 { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAgreedToTerms { get; set; } = false;
        public bool IsVerifiedEmail { get; set; } = false;
        public string? InstitutionPhone { get; set; }
        public string? InstitutionEmail { get; set; }
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
