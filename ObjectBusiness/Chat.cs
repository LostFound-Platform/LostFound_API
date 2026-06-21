using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class Chat
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ChatId { get; set; }
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public int PostId { get; set; }
        public int UserALastReadMessageId { get; set; }
        public int UserBLastReadMessageId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string? MessageContent { get; set; }
        [NotMapped]
        public string? Title { get; set; }
        [NotMapped]
        public bool IsRead { get; set; }
        [NotMapped]
        public DateTime? DateSendMessage { get; set; }

        // Build relationships
        public Users? UserA { get; set; }
        public Users? UserB { get; set; }
        public Posts? Post { get; set; }
        public ICollection<MessageChat>? Messages { get; set; }
    }
}
