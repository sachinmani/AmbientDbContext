 using System;
 using System.Data;
 using System.Data.Entity;
 using System.Data.SqlClient;
 using System.Linq;
 using System.Threading;
 using System.Threading.Tasks;
 using AmbientDbContext.Interfaces;
 using AmbientDbContext.Manager;
using DataModel;
using NUnit.Framework;

namespace UnitTests
{
    [TestFixture]
    public class AmbientDbContextTest: TestBase
    {
        private DropCreateDatabaseAlways<BloggerDbContext> _dropCreateDatabaseAlways;
        private BloggerDbContext _context;
        private IDbContextScopeFactory _dbContextScopeFactory;

        [SetUp]
        public void Setup()
        {
            _dbContextScopeFactory = new DbContextScopeFactory();
            _dropCreateDatabaseAlways = new DropCreateDatabaseAlways<BloggerDbContext>();
            Database.SetInitializer(_dropCreateDatabaseAlways);
            _context = new BloggerDbContext();
            _dropCreateDatabaseAlways.InitializeDatabase(_context);
        }

        [TearDown]
        public void TearDown()
        {
            
        }

        /// <summary>
        /// Testing the dbContext in the single thread.
        /// </summary>
        [Test]
        public void Saves_Successfully()
        {
            using (
                var dbContextScope =
                    _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                //Adding Blog to the database.
                var blog = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog);
                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog.BlogPost = post;
                dbContextScope.SaveAndCommitChanges();

                Assert.That(context.Blogs.Count() == 1);
                Assert.That(context.Posts.Count() == 1);
            }
        }

        /// <summary>
        /// Testing the dbContext in the single thread but in async mode which causes the await task to run on a different thread.
        /// </summary>
        [Test]
        public void Saves_Successfully_InAsyncMode()
        {
            using (
                var dbContextScope =
                    _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>(IsolationLevel.ReadCommitted))
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                //Adding Blog to the database.
                var blog = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog);

                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog.BlogPost = post;
                dbContextScope.SaveAndCommitChanges();
                var result = Task.Run(() => context.Blogs.FirstAsync());
                result.Wait();
                Assert.That(result.Result != null);
                Assert.That(context.Blogs.Count() == 1);
                Assert.That(context.Posts.Count() == 1);
                
            }
        }

        /// <summary>
        /// Testing the dbContext in the nested mode.
        /// </summary>
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Should_ThrowInvalidOperationException_WhenParentInRead_AndChildInWriteMode()
        {
            using (_dbContextScopeFactory.CreateAmbientDbContextInReadonlyMode<BloggerDbContext>())
            {

                using (
                    var dbContextScope2 =
                        _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                    //Adding Blog to the database.
                    var blog = new Blog
                    {
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        BlogUser = new User
                        {
                            Name = "TestUser",
                            Occupation = "Software Developer",
                        },
                        Overview = "This is a test overview"
                    };
                    context.Blogs.Add(blog);

                    var post = new Post
                    {
                        Content = "Test Content",
                        Meta = "Test",
                        ShortDescription = "This is an example test content",
                        Title = "Ambient Simple Test Context"
                    };
                    blog.BlogPost = post;
                    dbContextScope2.SaveAndCommitChanges();
                }
            }
        }

        /// <summary>
        /// Testing the dbContext in the nested mode.
        /// </summary>
        [Test]
        public void Should_Successfully_Save_WhenParentAndChild_InWriteMode()
        {
            using (
                var dbContextScope =
                    _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {

                using (
                    var dbContextScope2 =
                        _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();
                    //Adding Blog to the database.
                    var blog2 = new Blog
                    {
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        BlogUser = new User
                        {
                            Name = "TestUser",
                            Occupation = "Software Developer",
                        },
                        Overview = "This is a test overview"
                    };
                    context2.Blogs.Add(blog2);

                    var post2 = new Post
                    {
                        Content = "Test Content",
                        Meta = "Test",
                        ShortDescription = "This is an example test content",
                        Title = "Ambient Simple Test Context"
                    };
                    blog2.BlogPost = post2;
                    dbContextScope2.SaveAndCommitChanges();
                    Assert.That(!context2.Blogs.Any());
                    Assert.That(!context2.Posts.Any());
                }

                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                //Adding Blog to the database.
                var blog = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog);
                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog.BlogPost = post;
                dbContextScope.SaveAndCommitChanges();
                Assert.That(context.Blogs.Count() == 2);
                Assert.That(context.Posts.Count() == 2);
            }
        }

        [Test]
        public void Should_SaveSuccessfully_WhenUsingAsyncMethods()
        {
            var task = new Task(async () => await AddBlog());
            task.Start();
            task.Wait();
        }

        [Test]
        public void Should_SaveSuccessfully_WhenUsingNestedAsyncMethods()
        {
            using (
                var dbContextScope2 =
                    _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();

                var task = new Task(async() => await AddBlog());
                task.Start();
                task.Wait();
                Assert.That(!context2.Blogs.Any());
                Assert.That(!context2.Posts.Any());
                dbContextScope2.SaveAndCommitChanges();
                Assert.That(context2.Blogs.Count() == 1);
            }
        }

        private async Task AddBlog()
        {
            using (
                    var dbContextScope2 =
                        _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();

                //Adding Blog to the database.
                var blog2 = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context2.Blogs.Add(blog2);

                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog2.BlogPost = post;
                await dbContextScope2.SaveAndCommitChangesAsync(new CancellationToken());
            }
        }
        
        [Test]
        public void Saves_Successfully_WhenExternalTransactionUsed()
        {
            var initializer = new CreateDatabaseIfNotExists<BloggerDbContext>();
            Database.SetInitializer(initializer);
            initializer.InitializeDatabase(new BloggerDbContext());
            using (var conn = new SqlConnection(@"Server=.\SqlExpress;Database=Blog;Integrated Security=true;Trusted_Connection=true"))
            {
                conn.Open();

                using (var sqlTxn = conn.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    using (
                        var dbContextScope2 =
                            _dbContextScopeFactory.CreateAmbientDbContextWithExternalTransaction<BloggerDbContext>(sqlTxn, conn))
                    {
                        //Get the current DbContext of type BloggerDbContext
                        var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();

                        //Adding Blog to the database.
                        var blog2 = new Blog
                        {
                            CreatedDate = DateTime.Now,
                            UpdatedDate = DateTime.Now,
                            BlogUser = new User
                            {
                                Name = "TestUser",
                                Occupation = "Software Developer",
                            },
                            Overview = "This is a test overview"
                        };
                        context2.Blogs.Add(blog2);

                        var post = new Post
                        {
                            Content = "Test Content",
                            Meta = "Test",
                            ShortDescription = "This is an example test content",
                            Title = "Ambient Simple Test Context"
                        };
                        blog2.BlogPost = post;
                        dbContextScope2.SaveAndCommitChanges();
                        Assert.That(context2.Blogs.Count() == 1);
                    }
                    sqlTxn.Commit();
                }
                conn.Close();
                conn.Dispose();
            }
        }

        [Test]
        public void NonAmbientDbContext_Saves_Successfully()
        {
            using (var dbContextScope =
                     _dbContextScopeFactory.CreateNonAmbientDbContextInTransactionMode<BloggerDbContext>(IsolationLevel.Serializable))
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                //Adding Blog to the database.
                var blog = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog);
                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog.BlogPost = post;
                dbContextScope.SaveAndCommitChanges();

                Assert.That(context.Blogs.Count() == 1);
                Assert.That(context.Posts.Count() == 1);
            } 
        }

        [Test]
        public void NonAmbientDbContext_Saves_Successfully_WhenNestedInside_AmbinetContext()
        {
            using (var dbContextScope2 =
                _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>(IsolationLevel.Serializable))
            {
                using (var dbContextScope =
                    _dbContextScopeFactory.CreateNonAmbientDbContextInTransactionMode<BloggerDbContext>(IsolationLevel.ReadCommitted))
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                    //Adding Blog to the database.
                    var blog = new Blog
                    {
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        BlogUser = new User
                        {
                            Name = "TestUser",
                            Occupation = "Software Developer",
                        },
                        Overview = "This is a test overview"
                    };
                    context.Blogs.Add(blog);
                    var post = new Post
                    {
                        Content = "Test Content",
                        Meta = "Test",
                        ShortDescription = "This is an example test content",
                        Title = "Ambient Simple Test Context"
                    };
                    blog.BlogPost = post;
                    dbContextScope.SaveAndCommitChanges();

                    Assert.That(context.Blogs.Count() == 1);
                    Assert.That(context.Posts.Count() == 1);
                }
            }
        }

        [Test]
        public void NonAmbientDbContext_And_AmbientDbContext_Saves_Successfully_WhenContextsAreNested()
        {
            using (var dbContextScope2 =
                _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                using (var dbContextScope =
                    _dbContextScopeFactory.CreateNonAmbientDbContextInTransactionMode<BloggerDbContext>(IsolationLevel.ReadCommitted))
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                    //Adding Blog to the database.
                    var blog = new Blog
                    {
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now,
                        BlogUser = new User
                        {
                            Name = "TestUser",
                            Occupation = "Software Developer",
                        },
                        Overview = "This is a test overview"
                    };
                    context.Blogs.Add(blog);
                    var post = new Post
                    {
                        Content = "Test Content",
                        Meta = "Test",
                        ShortDescription = "This is an example test content",
                        Title = "Ambient Simple Test Context"
                    };
                    blog.BlogPost = post;
                    dbContextScope.SaveAndCommitChanges();

                    Assert.That(context.Blogs.Count() == 1);
                    Assert.That(context.Posts.Count() == 1);
                }

                //Get the current DbContext of type BloggerDbContext
                var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();
                //Adding Blog to the database.
                var blog2 = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context2.Blogs.Add(blog2);
                var post2 = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog2.BlogPost = post2;
                dbContextScope2.SaveAndCommitChanges();

                Assert.That(context2.Blogs.Count() == 2);
                Assert.That(context2.Posts.Count() == 2);
            }
        }

        [Test]
        public void Allows_DirtyReads_And_DoesntSaveWhenTransactionNotCommitted()
        {
            using (var dbContextScope = _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();

                //Adding Blog to the database.
                var blog2 = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog2);

                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog2.BlogPost = post;
                dbContextScope.SaveChanges();
                using (
                        _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();
                    Assert.That(context2.Blogs.Count() == 1);
                }
            }

            using (_dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                Assert.That(!context.Blogs.Any());
            }
        }

        [Test]
        public void SavesSuccessfully_WhenTransactionCommitted()
        {
            using (var dbContextScope = _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();

                //Adding Blog to the database.
                var blog2 = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog2);

                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog2.BlogPost = post;
                dbContextScope.SaveChanges();
                using (
                    var dbContextScope2 =
                        _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();
                    Assert.That(context2.Blogs.Count() == 1);
                }
                dbContextScope.Commit();
            }

            using (_dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                Assert.That(context.Blogs.Count() == 1);
            }
        }

        [Test]
        public void DoesntSaveTransaction_FromChildDbContext()
        {
            using (var dbContextScope = _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();

                //Adding Blog to the database.
                var blog2 = new Blog
                {
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    BlogUser = new User
                    {
                        Name = "TestUser",
                        Occupation = "Software Developer",
                    },
                    Overview = "This is a test overview"
                };
                context.Blogs.Add(blog2);

                var post = new Post
                {
                    Content = "Test Content",
                    Meta = "Test",
                    ShortDescription = "This is an example test content",
                    Title = "Ambient Simple Test Context"
                };
                blog2.BlogPost = post;
                dbContextScope.SaveChanges();
                using (
                    var dbContextScope2 =
                        _dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
                {
                    //Get the current DbContext of type BloggerDbContext
                    var context2 = DbContextLocator.GetDbContext<BloggerDbContext>();
                    Assert.That(context2.Blogs.Count() == 1);

                    dbContextScope2.Commit();
                }
            }

            using (_dbContextScopeFactory.CreateAmbientDbContextInTransactionMode<BloggerDbContext>())
            {
                //Get the current DbContext of type BloggerDbContext
                var context = DbContextLocator.GetDbContext<BloggerDbContext>();
                Assert.That(!context.Blogs.Any());
            }
        }
    }
}
