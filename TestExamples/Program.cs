using System;
using System.Data.Entity;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Operations;
using TestExamples.Repository;
using TestExamples.ValueObjects;

namespace TestExamples
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Creating Database first");
            //Creating database first
            var _dropCreateDatabaseAlways = new DropCreateDatabaseAlways<BloggerDbContext>();
            Database.SetInitializer(_dropCreateDatabaseAlways);
            var _context = new BloggerDbContext();
            _dropCreateDatabaseAlways.InitializeDatabase(_context);

            //Creating an user
            Console.WriteLine("Creating an user");
            IUserRepository userRepository = new UserRepository(new UserOperations());
            var userId = userRepository.AddUser(new VoUser
            {
                Name = "TestUser",
                Occupation = "Software Developer"
            });

            //Creating a blog and its post
            Console.WriteLine("Creating a blog post");
            IBlogRepository blogRepository = new BlogRepository(new BlogOperations(), new PostOperations());
            var blog = new VoBlog
            {
                CreatedDateTime = DateTime.Now,
                Overview = "This is a sample overview.",
                Post = new VoPost
                {
                    Meta = "Sample, Test",
                    Content = "This is a sample overview",
                    ShortDescription = "This is a sample short description",
                    Title = "Test Title"
                },
                UserId = userId
                
            };
            blogRepository.AddBlog(blog);

            //Retrieving the user in a async mode. This is where thread switching happens
            Console.WriteLine("Retrieving the created user");
            var user =  userRepository.GetUserAsync(userId).ConfigureAwait(false);

            Console.WriteLine("Name of the created user is "+ user.GetAwaiter().GetResult().Name);

            //Get user most recent blog
            Console.WriteLine("Retrieving the user most recent blog"); 
            var recentBlog = blogRepository.GetUserRecentBlog(userId);
            Console.WriteLine("Overview "+ recentBlog.Overview);
            Console.WriteLine("BlogPost Meta " + recentBlog.Post.Meta);
            Console.WriteLine("BlogPost Short Description " + recentBlog.Post.ShortDescription);
            Console.WriteLine("BlogPost Content " + recentBlog.Post.Content);


            Console.ReadKey();
        }
    }
}
