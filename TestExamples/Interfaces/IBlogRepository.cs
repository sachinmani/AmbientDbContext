using System.Collections.Generic;
using TestExamples.ValueObjects;

namespace TestExamples.Interfaces
{
    public interface IBlogRepository
    {
        VoBlog GetBlog(int blogId);

        IEnumerable<VoBlog> GetBlogs();

        void AddBlog(VoBlog voBlog);

        VoBlog GetUserRecentBlog(long userId);
    }
}
