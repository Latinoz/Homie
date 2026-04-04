using System.ComponentModel.DataAnnotations;

namespace Homie.Areas.Series.Models
{
    public class UpdateCommentRequest
    {
        public int Id { get; set; }

        [MaxLength(255)]
        public string Comment { get; set; }
    }
}
