using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FBLA_API.DTOs.Chat
{
    public class MessageChatDTO
    {
        public int MessageChatId { get; set; }
        public int ChatId { get; set; }
        public string MessageContent { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public int PostId { get; set; }
    }
}
