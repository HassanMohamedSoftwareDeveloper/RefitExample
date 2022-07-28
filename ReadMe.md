# Using Refit to Consume APIs in C#
> Consuming APIs over HTTP is a very common scenario when building applications. In this article, we are going to explore Refit to consume APIs in C#.

## What is Refit?

> The Refit library for C# provides us with a type-safe wrapper for interacting with HTTP-based APIs. Instead of using HttpClient, which is provided for us by ASP.NET Core, we can define an interface that represents the API we want to interact with.
>
> With this interface, we define the endpoints (GET, POST, PUT) our API contains, along with any route or body parameters. Also, we can include headers in the interface, such as ones for Authorization.

## Components of a Refit Client
> Before creating an application to demonstrate Refit, let’s explore some of the main components that make up a Refit client.
## HTTP Methods
> Any time we interact with an API over HTTP, we must be familiar with the different HTTP methods available to us, and how they work. Refit provides a set of attributes that allow us to decorate our interface methods:
```C#
[Get("/users")]
Task<IEnumerable<User>> GetUsers();
```
> By decorating the <code>GetUsers()</code> method with <code>[Get("/users")]</code>, we tell Refit this is an HTTP GET method, to the <code>/users</code> endpoint.

==Refit provides attributes for all the common HTTP methods.==

---
## Route Parameters
> When working with RESTful APIs that follow good <code>routing conventions</code>, we’ll often see an endpoint like <code>/users/1</code>, which we would expect to return us a user with id 1. Refit uses attribute routing, the same as ASP.NET Core, that allows us to easily define routes that contain parameters:

```C#
[Get("/users/{id}")]
Task<User> GetUser(int id);
```
> By adding <code>{</code> and <code>}</code> around <code>id</code> in the route, we tell Refit that this is a dynamic parameter that comes from the id parameter in the <code>GetUser()</code> method.

## Request and Response Serialization
> The most common way to send data over HTTP is by <code>serializing it as JSON</code> and adding it to the request body.
<code> Refit provides this automatically for us. </code>

> This allows us to provide classes as parameters to a Refit method, and also specify them as the return type that we expect to be returned from the API:

```C#
[Put("/users/{id}")]
Task<User> UpdateUser(int id, User user);
```
> Refit will automatically serialize the <code>user</code> parameter to JSON when sending the request and will attempt to deserialize the response into a <code>User</code> object.
## Instantiating a Refit Client
> Refit provides us with two ways to instantiate a client, either by using the <code>RestService</code> class provided by Refit, or by registering the Refit client with <code>HttpClientFactory</code>, and injecting the interface into a class constructor.

> Let’s assume we have an API for interacting with users, along with a Refit interface:

```C#
public interface IUsersClient
{
    [Get("/users")]
    Task<IEnumerable<User>> GetUsers();
}
```
> First, we can instantiate the client using the <code>RestService</code> class:

```C#
var usersClient = RestService.For<IUsersClient>("https://localhost:7025");
var users = await usersClient.GetUsers();
```

> We can also register the client with  <code>HttpClientFactory</code> provided by ASP.NET Core:

```C#
services
    .AddRefitClient<IUsersClient>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7025"));
```
<code>Both of these are valid ways to register and use Refit clients.</code>
> However, **if we want to make our code more maintainable and testable, registering the client with** <code> HttpClientFactory</code> **and injecting it into the required class constructors is the way to go**. This allows us to easily inject a mock of the interface for testing purposes, without having to rely on any of the implementation details of either <code>HttpClient</code> or the Refit library.

> <code>Because of this, we will opt for the latter method for the rest of this article.</code>

## Setting up an API
> Instead of setting up a new API from scratch, we can use **JSONPlaceholder**. It is a free, fake API that can be used for testing, and fits our needs perfectly. It provides various resources to interact with, but for this demo, we’ll use the <code>users</code> resource.

## Creating Console Application
> With our API solution chosen, let’s create a console application, either through the Visual Studio template or by using <code>dotnet new console</code>.

> We must also add the Refit library from NuGet. As we will be using the <code>HttpClientFactory</code> registration method, we need to add two packages:
+ Refit
+ Refit.HttpClientFactory

> As we have chosen the <code>users</code> resource, we’ll create a User model:

```C#
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public override string ToString() =>
        string.Join(Environment.NewLine, $"Id: {Id}, Name: {Name}, Email: {Email}");
}
```
> We override the <code>ToString()</code> method so we can easily display the users retrieved from the API in the console.

> Now we can create our Refit interface.

## Implementing Refit Client
> We start by creating an interface and defining a <code>GetAll</code> method:

```C#
public interface IUsersClient
{
    [Get("/users")]
    Task<IEnumerable<User>> GetAll();
}
```
> To turn this interface into a Refit client, we add the <code>Get</code> attribute to the <code>GetAll()</code> method, and define the route as <code>/users</code>. As the API will return us a list of users, the method return type is an <code>IEnumerable< User></code>.

## Consuming API Data

> As we’ve opted to register our Refit client with the ASP.NET Core dependency injection framework, we need to add the <code>Microsoft.Extensions.Hosting</code> NuGet package to our console application.

> With this done, let’s register the Refit client in the <code>Program</code> class:

```C#
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services
            .AddRefitClient<IUsersClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7025/"));
    }).Build();
```
> We use the <code>AddRefitClient()</code> extension method to register the <code>IUsersClient</code> interface, and then configure the <code>HttpClient</code>, setting the <code>BaseAddress</code> to the JSONPlaceholder address.

> With our service registration complete, we can retrieve an instance of <code>IUsersClient</code>, and retrieve some users:

```C#
var usersClient = host.Services.GetRequiredService<IUsersClient>();
var users = await usersClient.GetAll();

foreach (var user in users)
{
    Console.WriteLine(user);
}
```
> First, we retrieve an <code>IUsersClient</code> from the service collection, and call the <code>GetAll()</code> method to retrieve a list of users, which we then print to the console.

> This demonstrates how simple it is to use a Refit client to abstract HTTP calls. We make a method call that returns our populated <code>User</code> model.

> Next, let’s explore some of the further capabilities of Refit, by adding more methods to <code>IUsersClient</code>.

## Extending IUsersClient
> Let’s add some basic CRUD <code>(Create, Read, Update, Delete)</code> operations for our API:

```C#
public interface IUsersClient
{
    [Get("/users")]
    Task<IEnumerable<User>> GetAll();

    [Get("/users/{id}")]
    Task<User> GetUser(int id);

    [Post("/users")]
    Task<User> CreateUser([Body] User user);

    [Put("/users/{id}")]
    Task<User> UpdateUser(int id, [Body] User user);

    [Delete("/users/{id}")]
    Task DeleteUser(int id);
}
```
> Firstly, we add the <code>GetUser()</code> method, which takes an <code>id</code> parameter to identify the user we want to retrieve. We decorate this method with the Get attribute, and in the route we define a dynamic parameter using <code>{</code> and <code>}</code>.

> Next up is the <code>CreateUser()</code> method, which takes a <code> User</code> as a parameter, and because we want this to be passed in the HTTP request body, we decorate the parameter with the <code>Body</code> attribute. This time, it’s a <code>Post</code> request that the API expects.

> To update a user, we need a <code>Put</code> method, combining both a route parameter, <code>id</code>, and body content, which is the <code>User</code> we want to update.

> Finally, to delete a user, we make a <code>Delete</code> request providing the <code>id</code> of the user to delete.

> This gives us CRUD functionality on the Users API. Now we can test this out.

## Testing CRUD Functionality
> Back in the <code>Program</code> class, let’s start by creating a new user:

```C#
var user = new User
{
    Name = "Hassan Mohamed",
    Email = "HassanMohamed_hm@hotmail.com"
};

var usersClient = host.Services.GetRequiredService<IUsersClient>();
var userId = (await usersClient.CreateUser(user)).Id;

Console.WriteLine($"User with Id: {userId} created");
```
> Initially, we create a new <code>User</code> object. With this <code>user</code>, we call <code>CreateUser()</code>, which will return a User object, giving us the <code>Id</code> of the newly created user, which we log to the console.

> Next, we can retrieve an existing user using the <code>GetUser()</code> method:

```C#
var existingUser = await usersClient.GetUser(1);
```
> With this user, let’s update the <code>Email</code>:

```C#
existingUser.Email = "Hassanmohamed_HM@hotmail.com";
var updatedUser = await usersClient.UpdateUser(existingUser.Id, existingUser);

Console.WriteLine($"User email updated to {updatedUser.Email}");
```
> Here, we use the <code>UpdateUser()</code> method, passing in the Id of the user, along with the updated user object.

> The final step is to delete the user:

```C#
await usersClient.DeleteUser(userId);
```
> We simply call <code>DeleteUser()</code>, providing the <code>userId</code> to delete.

> This covers the basic CRUD functionality and shows how simply we can create an interface to interact with an API, without the need of handling complex HTTP logic with an <code>HttpClient</code>.


# Another Implementation with Headers Like Authorization 
## Extending IUsersClientWithHeaders
```C#
[Headers("Authorization: Bearer")]
public interface IUsersClientWithHeaders
{
    [Get("/users")]
    Task<IEnumerable<User>> GetAll();
}
```
> In Program 
```C#
#region Another Implementation with Headers Like Authorization :
Console.WriteLine();
Console.WriteLine();
Console.WriteLine();
Console.WriteLine("Another Implementation with Headers Like Authorization");
var authHeaderToken = "token value";
var userClientWithHeaders = RestService.For<IUsersClientWithHeaders>("https://localhost:7025/", new RefitSettings()
{
    AuthorizationHeaderValueGetter = () => Task.FromResult(authHeaderToken)
});
var usersList = await userClientWithHeaders.GetAll();
foreach (var item in usersList)
{
    Console.WriteLine(item);
}
#endregion
```
## Source Code
> To download source code  [Click here ](https://github.com/HassanMohamedSoftwareDeveloper/RefitExample).
## Conclusion
In this article, we’ve learned how we can abstract interaction with HTTP-based APIs by using Refit and creating a simple interface for our API. This allowed us to avoid dealing with complex HTTP logic, such as creating request messages and deserializing responses and instead focus on the core logic relating to our applications.