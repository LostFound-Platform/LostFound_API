using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class Notifications
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int NotificationId { get; set; }
        [Required(ErrorMessage = "Post original ID cannot be blank")]
        public int PostOriginalId { get; set; }
        [Required(ErrorMessage = "Post matched ID cannot be blank")]
        public int PostMatchedId { get; set; }
        public NotificationType NotificationType { get; set; } // MatchImage, MatchDescription
        [Required(ErrorMessage = "Notification content cannot be blank")]
        public string NotificationContent { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Not mapped fields
        [NotMapped]
        public string? FirstNameOriginal { get; set; }
        [NotMapped]
        public string? LastNameOriginal { get; set; }
        [NotMapped]
        public string? FirstNameMatched { get; set; }
        [NotMapped]
        public string? LastNameMatched { get; set; }
        [NotMapped]
        public string? TitlePostMatched { get; set; }
        [NotMapped]
        public string? ImagePostMatched { get; set; }
        [NotMapped]
        public string? UrlImagePostMatched { get; set; }
        [NotMapped]
        public string? TitlePostOriginal { get; set; }
        [NotMapped]
        public string? ImagePostOriginal { get; set; }
        [NotMapped]
        public string? UrlImagePostOriginal { get; set; }
        [NotMapped]
        public string? AvatarUserMatched { get; set; }
        [NotMapped]
        public string? UrlAvatarUserMatched { get; set; }
        [NotMapped]
        public string? DescriptionPostMatched { get; set; }
        [NotMapped]
        public TypePost? TypePost { get; set; }

        // Build relationships
        public Posts? PostOriginal { get; set; }
        public Posts? PostMatched { get; set; }
    }
}
