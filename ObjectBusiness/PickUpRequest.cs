using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class PickUpRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int RequestId { get; set; }
        [Required(ErrorMessage = "User ID can not be blank")]
        public int PostId { get; set; }
        public string Description { get; set; }
        public DateTime? PickUpDate { get; set; }
        public StatusRequest Status { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? UpdatedDate { get; set; }

        // Build relationships
        public Posts? Post { get; set; }
    }
}
