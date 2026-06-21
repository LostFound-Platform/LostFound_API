using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class MessageChat
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MessageChatId { get; set; }
        [Required(ErrorMessage = "Chat ID can not be blank")]
        public int ChatId { get; set; }
        [Required(ErrorMessage = "Sender ID can not be blank")]
        public int UserSenderId { get; set; }
        [Required(ErrorMessage = "Message can not be blank")]
        public string MessageContent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public int UserAId { get; set; }
        [NotMapped]
        public int UserBId { get; set; }
        [NotMapped]
        public int PostId { get; set; }

        [NotMapped]
        public string? FirstNameUserA { get; set; }
        [NotMapped]
        public string? LastNameUserA { get; set; }
        [NotMapped]
        public string? FirstNameUserB { get; set; }
        [NotMapped]
        public string? LastNameUserB { get; set; }
        [NotMapped]
        public string? AvatarUserA { get; set; }
        [NotMapped]
        public string? AvatarUserB { get; set; }
        [NotMapped]
        public DateTime? DateSendMessage { get; set; }

        // Build relationships
        public Users? UserSender { get; set; }
        public Chat? Chat { get; set; }
    }
}
