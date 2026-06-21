namespace FBLA_API.DTOs.Users
{
    public class UserDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile? AvatarUpload { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
    }
}
