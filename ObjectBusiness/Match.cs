using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class Match
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int MatchId { get; set; }
        public int LostPostId { get; set; }
        public int FoundPostId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        [NotMapped]
        public string? Code { get; set; }

        // Build relationship
        public VerificationCode? VerificationCode { get; set; }
        public Posts? LostPost { get; set; }
        public Posts? FoundPost { get; set; }
    }
}
