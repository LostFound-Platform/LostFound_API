using ObjectBusiness;

namespace FBLA_API.DTOs.Match
{
    public class MatchSuggestionDTO
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int CategoryPostId { get; set; }
        public TypePost TypePost { get; set; } // Lost or Found
        public string? Vector { get; set; } // Use to find similar image 
        public string? Image { get; set; }
        public string? UrlImage { get; set; }
        public string Code { get; set; } // Use to print out and stick on the stuff
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int FoundPostId { get; set; }
        public double Score { get; set; }
        public ObjectBusiness.Users User { get; set; }
    }
}
