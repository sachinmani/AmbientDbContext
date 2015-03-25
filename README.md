# AmbientDbContext
AmbientDbContext helps you buildmodular and structured code using Entity framework. This utility works only with .Net 4.5.

For quite sometime I have been thinking about the ways of using DbContext in my projects. I always wanted a solution where I don't have pass around the DbContext as I felt passing DbContext around would make the code messier. Then thought about implementing a solution using ThreadStatic but solution using ThreadStatic cannot be used along with async method and linq queries. Finally came up with a solution of implementing the DbContext using CallContext.

What is CallContext?
According to microsoft documentation "The call context allows a remoting client and a remote object to share information without explicitly passing the information as a parameter or return value." . HttpContext.Current.items, Transactions are few of the items in the .Net framework that uses CallContext.

Without going into the details of the implementation. I would like to straight go into the ways this utility could be used. Also understand things that need to be done, things to be aware of when using this utility. This utility could be used with any design pattern you would be using around the data access layer.

Make sure you wrap all your call inside "using" statement

Starting a new readonly business transaction normally a GET request in the web.

Make sure you wrap all your call inside "using" statement

using(dbContextScopeFactory.CreateAmbientDbContextInReadonlyMode())
{

}

or

in case you want to pass different isolation level to the default IsolationLevel.ReadCommitted

using(dbContextScopeFactory.CreateAmbientDbContextInReadonlyMode(isolationLevel))
{
	
}

Starting a new write business transaction normally POST/PUT/DELETE request in the web

using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode())
{

	dbContextScope.SaveChanges();
	or 
	dbContextScope.SaveChangesAsync();
}
or

in case you want to pass different isolation level to the default IsolationLevel.ReadCommitted

using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode(isolationLevel))
{

	dbContextScope.SaveChanges();
	or 
	dbContextScope.SaveChangesAsync();
}

Nesting Ambient DbContextScope

This utility allows you to nest dbContextScopes as call to SaveChanges from the child doesn't commit the business transaction. Only the SaveChanges from parent dbContextScope commit the transaction. Following are the things to have in mind.

1. When the parent dbContextScope is in readonly mode, you cannot create a child dbContextScope in write transaction mode. Trying to do will do throw exception.

2. Cannot change the isolation level in the child scope, trying to do will throw exception.

3. You can have a parent dbContextScope in write scope and a child dbContextScope in readonly mode.

using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode())
{
	using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode())
	{
		//this call below does not save the transaction
		dbContextScope.SaveChanges();
		or 
		dbContextScope.SaveChangesAsync();
	}

	dbContextScope.SaveChanges();
	
	or 
	
	dbContextScope.SaveChangesAsync();
}

Though this is good, but I personally try to avoid nesting dbContextScope

Still lot more documentation to come. Please get back to me if you have any issues or doubts on any API.

What is coming up

1. Allow developers to call commit tranasction explicitly to allow them to do dirty reads inside the transaction.

using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode())
{
	...
	
	dbContextScope.SaveChanges();
	dbContextScope.CommitTransaction();
}

2. Better logging and exception handling and throwing 
