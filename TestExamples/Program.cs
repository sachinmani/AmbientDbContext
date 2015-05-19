using System;
using System.Data.Entity;
using DataModel;
using TestExamples.Interfaces;
using TestExamples.Services;
using TestExamples.ValueObjects;

namespace TestExamples
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Creating Database first");
            //Creating database first
            var dropCreateDatabaseAlways = new DropCreateDatabaseAlways<BloggerDbContext>();
            Database.SetInitializer(dropCreateDatabaseAlways);
            var context = new BloggerDbContext();
            dropCreateDatabaseAlways.InitializeDatabase(context);

            //Creating an user
            Console.WriteLine("Creating an user");
            IUserServices userRepository = new UserServices();
            var userId = userRepository.AddUser(new VoUser
            {
                Name = "TestUser",
                Occupation = "Software Developer"
            });

            //Creating a blog and its post
            Console.WriteLine("Creating a blog post");
            IBlogServices blogRepository = new BlogServices();
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

            //Retrieving the user in an async mode. This is where thread switching happens
            Console.WriteLine("Retrieving the created user");
            var user =  userRepository.GetUserAsync(userId).ConfigureAwait(false);

            Console.WriteLine("Name of the created user is "+ user.GetAwaiter().GetResult().Name);

            //Get the user's most recent blog
            Console.WriteLine("Retrieving the user most recent blog"); 
            var recentBlog = blogRepository.GetUserRecentBlog(userId);
            Console.WriteLine("Overview "+ recentBlog.Overview);
            Console.WriteLine("BlogPost Meta " + recentBlog.Post.Meta);
            Console.WriteLine("BlogPost Short Description " + recentBlog.Post.ShortDescription);
            Console.WriteLine("BlogPost Content " + recentBlog.Post.Content);

            //Create a blog and try to save it. If the creation fails , update the  creation failure count in the user profile.
            var blog2 = new VoBlog
            {
                CreatedDateTime = DateTime.Now,
                Post = new VoPost
                {
                    Meta = "Sample, Test",
                    Content = "This is a sample overview",
                    ShortDescription = "This is a sample short description",
                    Title = "Test Title"
                },
                UserId = userId

            };
            try
            {
                blogRepository.AddBlog(blog2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);

            }
            Console.ReadKey();
        }
    }
}
