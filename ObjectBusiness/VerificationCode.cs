using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class VerificationCode
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VerificationCodeId { get; set; }
        public int MatchId { get; set; }
        [Required(ErrorMessage = "Code cannot be blank")]
        public string Code { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Build relationship
        public Match? Match { get; set; }
    }
}
