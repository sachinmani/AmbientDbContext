using DataModel;
using TestExamples.ValueObjects;

namespace TestExamples.Interfaces
{
    public interface IPostRepository
    {
        void AddPost(VoPost post);

        Post GetPost(long postId);
    }
}
