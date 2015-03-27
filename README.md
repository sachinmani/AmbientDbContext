# AmbientDbContext
AmbientDbContext helps you build modular and structured code using Entity framework. This utility works only with .Net 4.5.

For quite sometime I have been thinking about the ways of using DbContext in my projects. I always wanted a solution where I don't have pass around the DbContext as I felt passing DbContext around would make the code messier. Then thought about implementing a solution using ThreadStatic but solution using ThreadStatic cannot be used along with async method and linq queries. Finally came up with a solution of implementing the DbContext using CallContext.

What is CallContext?
According to microsoft documentation "The call context allows a remoting client and a remote object to share information without explicitly passing the information as a parameter or return value." . HttpContext.Current.items, Transactions are few of the items in the .Net framework that uses CallContext.

Without going into the details of the implementation. I would like to straight go into the ways this utility could be used. Also understand things that need to be done, things to be aware of when using this utility. This utility could be used with any design pattern you would be using around the data access layer.

Make sure you wrap all your call inside "using" statement

Also, extend your DbContext with the interface IAmbientDbContext

<h3><b>Starting a new readonly business transaction normally a GET request in the web.</b></h3>

Make sure you wrap all your call inside <b>"using"</b> statement

using(dbContextScopeFactory.CreateAmbientDbContextInReadonlyMode())
{

}

or

in case you want to pass different isolation level to the default IsolationLevel.ReadCommitted

using(dbContextScopeFactory.CreateAmbientDbContextInReadonlyMode(isolationLevel))
{
	
}

<h3><b>Starting a new write business transaction normally POST/PUT/DELETE request in the web</b></h3>
<code>
using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode())
{

	dbContextScope.SaveChanges();
	or 
	dbContextScope.SaveChangesAsync();
}
</code>
or

in case you want to pass different isolation level to the default IsolationLevel.ReadCommitted

using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode(isolationLevel))
{

	dbContextScope.SaveChanges();
	or 
	dbContextScope.SaveChangesAsync();
}

<h3><b>Nesting Ambient DbContextScope</b></h3>

This utility allows you to nest dbContextScopes as call to SaveChanges from the child doesn't commit the business transaction. Only the SaveChanges from parent dbContextScope commit the transaction. Following are the things to have in mind.

<h4><b>1. When the parent dbContextScope is in readonly mode, you cannot create a child dbContextScope in write transaction mode. Trying to do will do throw exception.</b></h4>

<h4><b>2. Cannot change the isolation level in the child scope, trying to do will throw exception.</b></h4>

<h4><b>3. You can have a parent dbContextScope in write scope and a child dbContextScope in readonly mode.</b></h4>

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

 This dbContextScope will get committed irrespective of whether the Ambient DbContextScope if any exists passes or fails.

Though this is good, but I personally try to avoid nesting dbContextScope.


<h3><b>DbContextScope in StandAloneMode</b></h3>

This mode is created to handle scenarios where an user want to capture details on failure of a operation. For example, you want to update the user profile on whether the email is not sent.
<code>
try
{
	SendEmail()
}
catch(SmtpException){}
finally
{
	if(exeptionOccured)
	{
		using(var dbContextScope = dbContextScopeFactory.CreateNonAmbientDbContextInTransactionMode())
		{
			//find the user, and update the status
			dbContextScope.SaveChanges()
		}
	}
}
</code>
Still lot more documentation to come. Please get back to me if you have any issues or doubts on any API. I am currently working on some more features to this utility after which there would be an official 1.0 version of this utility. This utility still works and has all that is needed to get job done but just lacking documentation. Promise would soon put more documentation with examples.

<h4><b>What is coming up</b></h4>

1. Allow developers to call commit tranasction explicitly to allow them to do dirty reads inside the transaction.

using(var dbContextScope = dbContextScopeFactory.CreateAmbientDbContextInTransactionMode())
{
	...
	
	dbContextScope.SaveChanges();
	dbContextScope.CommitTransaction();
}

2. Better logging and exception handling and throwing 
