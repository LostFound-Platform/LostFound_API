using ObjectBusiness;

namespace FBLA_API.DTOs.Posts
{
    public class PostDTO
    {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryPostId { get; set; }
        public TypePost TypePost { get; set; } // Lost or Found
        public string? Vector { get; set; } // Use to find similar image 
        public string? Image { get; set; }
        public string Code { get; set; } // Use to print out and stick on the stuff
        public IFormFile? ImageUpload { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
