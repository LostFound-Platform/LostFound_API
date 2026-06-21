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
    public class Users
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }
        [Required(ErrorMessage = "First name cannot be blank")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name cannot be blank")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email cannot be blank")]
        [EmailAddress]
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        [Required(ErrorMessage = "The roles cannot be different between student, parent, and admin")]
        public Role Role { get; set; } // Student or Admin
        [Required(ErrorMessage = "Password cannot be blank")]
        public string Password { get; set; }
        public string? Avatar { get; set; }
        public string PickImage1 { get; set; }
        public string PickImage2 { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsAgreedToTerms { get; set; } = false;
        public bool IsVerifiedEmail { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped] // This property is not mapped to the database
        public string? AccessToken { get; set; }
        [NotMapped]
        public int ExpiresIn { get; set; }
        [NotMapped]
        public string? UrlAvatar { get; set; }
        [NotMapped]
        public int StudentId { get; set; }

        // IFormFile is used for only upload img from frontend
        [NotMapped]
        [JsonIgnore]
        public IFormFile? AvatarFromFrontend { get; set; }

        // Build relationship
        public Student? Student { get; set; }
    }
}
