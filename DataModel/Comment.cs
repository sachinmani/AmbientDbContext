using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    public class Comment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Index("IX_Comment_CommentId")]
        public virtual long CommentId { get; set; }

        [MaxLength(300, ErrorMessage = "Description is more than 300 characters.")]
        public virtual string Description { get; set; }

        public virtual EnumConstants.CommentType Type { get; set; }

        public virtual EnumConstants.CommentStatus CommentStatus { get; set; }
    }

    public class EnumConstants
    {
        public enum CommentStatus
        {
            Pending = 10001,
            Approved = 10002
        }

        public enum CommentType
        {
            Like = 20001,
            Comment = 20002
        }
    }
}