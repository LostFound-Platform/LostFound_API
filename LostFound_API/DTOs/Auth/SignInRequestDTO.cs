namespace LostFound_API.DTOs.Auth
{
    public class SignInRequestDTO
    {
        public string Password { get; set; }
        public string Email { get; set; }
        public List<int>? PickedIndexes { get; set; }
    }
}
