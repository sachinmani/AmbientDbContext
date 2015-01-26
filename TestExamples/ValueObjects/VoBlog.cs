using System;

namespace TestExamples.ValueObjects
{
    public class VoBlog
    {
        public long BlogId { get; set; }

        public long PostId { get; set; }

        public long UserId { get; set; }

        public VoPost Post { get; set; }

        public string Overview { get; set; }

        public DateTime CreatedDateTime { get; set; }
    }

    public class VoPost 
    {
        public string Title { get; set; }

        public string Meta { get; set; }

        public string ShortDescription { get; set; }

        public string Content { get; set; }
    }
}
