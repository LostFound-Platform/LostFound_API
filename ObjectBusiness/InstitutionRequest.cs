using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class InstitutionRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int InstitutionRequestId { get; set; }
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
        public string? AdditionalInformation { get; set; }
        public string? AdditionalNote { get; set; }
        public StudentPopulationRange? EstimatedPopulation { get; set; }
        [Required(ErrorMessage = "Applicant name cannot be blank")]
        public string ApplicantName { get; set; }
        public string? ApplicantPhoneNumber { get; set; }
        [Required(ErrorMessage = "Work email cannot be blank")]
        public string WorkEmail { get; set; }
        [Required(ErrorMessage = "Job title cannot be blank")]
        public string JobTitle { get; set; }
        public bool IsVerifiedEmail { get; set; } = false;
        public bool IsVerifiedWebsite { get; set; } = false;
        public bool IsVerifiedInstitution { get; set; } = false;
        public StatusRequestInstitution Status { get; set; } = StatusRequestInstitution.Pending;
        public string? InstitutionDescription { get; set; }
        public string? ReviewedBy { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        public DateTime? WebsiteVerifiedAt { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? RejectReason { get; set; }
        public DateTime SubmittedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }
    }
}
