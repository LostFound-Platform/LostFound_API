using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class TransferRequests
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RequestId { get; set; }
        [Required(ErrorMessage = "Post ID cannot be blank")]
        public int PostId { get; set; }
        [Required(ErrorMessage = "From user ID cannot be blank")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "To user ID cannot be blank")]
        public int ToUserId { get; set; } // To admin
        public StatusRequest Status { get; set; } = StatusRequest.Pending; // Pending, Confirmed, Cancelled
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? ConfirmedAt { get; set; }

        [NotMapped]
        public string? FirstName { get; set; }
        [NotMapped]
        public string? LastName { get; set; }
        [NotMapped]
        public string? NameItem { get; set; }
        [NotMapped]
        public Role? Role { get; set; }

        // Build relationships
        public Posts? Post { get; set; }
    }
}
