# AmbientDbContext
AmbientDbContext helps you buildmodular and structured code using Entity framework. This utility works only with .Net 4.5.

For quite sometime I have been thinking about the ways of using DbContext in my projects. I always wanted a solution where I don't have pass around the DbContext as I felt passing DbContext around would make the code messier. Then thought about implementing a solution using ThreadStatic but solution using ThreadStatic cannot be used along with async method and linq queries. Finally came up with a solution of implementing the DbContext using CallContext.

What is CallContext?
According to microsoft documentation "The call context allows a remoting client and a remote object to share information without explicitly passing the information as a parameter or return value." . HttpContext.Current.items, Transactions are few of the items in the .Net framework that uses CallContext.

Without going into the details of the implementation. I would like to straight go into the ways this utility could be used. Also understand things that need to be done, things to be aware of when using this utility. This utility could be used with any design pattern you would be using around the data access layer.


