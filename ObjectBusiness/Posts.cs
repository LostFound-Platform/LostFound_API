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
    public class Posts
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int PostId { get; set; }
        [Required(ErrorMessage = "User ID cannot be blank")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Title post cannot be blank")]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required(ErrorMessage = "Category post cannot be blank")]
        public int CategoryPostId { get; set; }
        [Required(ErrorMessage = "Type of post cannot be blank")]
        public TypePost TypePost { get; set; } // Lost or Found
        public string? Vector { get; set; } // Use to find similar image 
        public string? Image { get; set; }
        [Required(ErrorMessage = "Code cannot be blank")]
        public string Code { get; set; } // Use to print out and stick on the stuff
        public bool? IsReceived { get; set; }
        public int? OldUserId { get; set; } // Use to store student transfers post to admin
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // IFormFile is used for only upload img from frontend
        [NotMapped]
        public IFormFile? ImageUpload { get; set; }
        [NotMapped]
        public string? UrlImage { get; set; }

        // Build relationship
        //[JsonIgnore]
        public CategoryPost? CategoryPost { get; set; }
        //[JsonIgnore]
        public Users? User { get; set; }
        [JsonIgnore]
        public ICollection<TransferRequests>? TransferRequests { get; set; }
    }
}
