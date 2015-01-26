using AmbientDbContext.Manager;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Operations;
using TestExamples.ValueObjects;

namespace TestExamples.Repository
{
    public class PostRepository : IPostRepository
    {
        private readonly PostOperations _postOperations;

        public PostRepository(PostOperations postOperations)
        {
            _postOperations = postOperations;
        }

        public void AddPost(VoPost post)
        {
            using (
                var dbContextScope =
                    DbContextScopeFactory.CreateAmbientDbContextinTransactionMode<BloggerDbContext>())
            {
                _postOperations.AddPost(post);
                dbContextScope.SaveChanges(true);
            }
        }

        public Post GetPost(long postId)
        {
            throw new System.NotImplementedException();
        }
    }
}
