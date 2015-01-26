using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    public class Blog
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Index("IX_Blog_BlogId")]
        public long BlogId { get; set; }

        [ForeignKey("BlogPost"), Index("IX_Blog_PostId")]
        public long PostId { get; set; }

        public string Overview { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public Post BlogPost { get; set; }

        [ForeignKey("BlogUser"), Index("IX_Blog_UserId")]
        public long UserId { get; set; }

        public User BlogUser { get; set; }
    }
}