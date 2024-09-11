using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataScribeCSP.Models
{
    public class Files
    {
        [Key]
        public int FileId { get; set; }
        public string FileName { get; set; } = string.Empty;

        public User User { get; set; } = null;
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
    }
}
