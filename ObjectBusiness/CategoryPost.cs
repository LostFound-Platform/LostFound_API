using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectBusiness
{
    public class CategoryPost
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CategoryPostId { get; set; }
        [Required(ErrorMessage = "Category post name cannot be blank")]
        public string CategoryPostName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
