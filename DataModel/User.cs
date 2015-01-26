using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataModel
{
    public class User
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Index("IX_User_UserId")]
        public long UserId { get; set; }

        public string Name { get; set; }

        public string Occupation { get; set; }

        public ICollection<Blog> Blogs { get; set; } 
    }
}
