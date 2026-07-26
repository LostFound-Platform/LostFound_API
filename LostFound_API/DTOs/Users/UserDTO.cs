using ObjectBusiness;
using System.ComponentModel.DataAnnotations.Schema;

namespace LostFound_API.DTOs.Users
{
    public class UserDTO
    {
        public int StudentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile? AvatarUpload { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public string? InstitutionName { get; set; }
        public string? InstitutionAddress { get; set; }
        public string? InstitutionCity { get; set; }
        public string? InstitutionState { get; set; }
        public string? InstitutionPhone { get; set; }
        public string? InstitutionWebsite { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public Role Role { get; set; }
        public string Password { get; set; }
        public string PickImage1 { get; set; }
        public string PickImage2 { get; set; }

        [NotMapped] // This property is not mapped to the database
        public string? AccessToken { get; set; }
        [NotMapped]
        public int ExpiresIn { get; set; }
    }
}
