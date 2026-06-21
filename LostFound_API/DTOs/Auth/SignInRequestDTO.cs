namespace FBLA_API.DTOs.Auth
{
    public class SignInRequestDTO
    {
        public int StudentId { get; set; }
        public string Password { get; set; }
        public string? Email { get; set; }
        public List<int>? PickedIndexes { get; set; }
    }
}
