using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    public class Post
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Index("IX_Post_PostId"), Column(Order = 1)]
        public long PostId { get; set; }

        [MaxLength(100, ErrorMessage = "Title is more than 100 characters.")]
        public string Title { get; set; }

        [MaxLength(100, ErrorMessage = "Meta is more than 100 characters.")]
        public string Meta { get; set; }

        [MaxLength(500, ErrorMessage = "Short Description is more than 500 characters.")]
        public string ShortDescription { get; set; }

        [MaxLength(5000, ErrorMessage = "Content is more than 5000 characters.")]
        public string Content { get; set; }
    }
}