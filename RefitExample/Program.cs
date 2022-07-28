using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Refit;
using RefitExample;
using RefitExample.Models;

using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services
            .AddRefitClient<IUsersClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7025/"));
    }).Build();

var usersClient = host.Services.GetRequiredService<IUsersClient>();

#region Get All :
Console.WriteLine("------------------All Users-----------------");
var users = await usersClient.GetAll();
foreach (var item in users)
{
    Console.WriteLine(item);
}
#endregion

#region Get Users by Id :
Console.WriteLine("------------------User By Id-----------------");
var userById = await usersClient.GetUser(1);
Console.WriteLine(userById);
#endregion

#region Create User :
Console.WriteLine("------------------Create User-----------------");
var user = new User
{
    Name = "Ahmed Mohamed",
    Email = "ahmedmohamed@hotmail.com"
};
var userId = (await usersClient.CreateUser(user)).Id;
Console.WriteLine($"User with Id: {userId} created");
#endregion

#region Update User :
Console.WriteLine("------------------Update User-----------------");
var userToUpdate = new User
{
    Name = "Ahmed Mohamed AbdElfattah",
    Email = "ahmedmohamed@hotmail.com"
};
var updatedUser = await usersClient.UpdateUser(4, userToUpdate);
Console.WriteLine(updatedUser);
#endregion

#region Delete User :
Console.WriteLine("------------------Delete User-----------------");
var userIdToDelete = (await usersClient.CreateUser(user)).Id;
await usersClient.DeleteUser(userIdToDelete);
Console.WriteLine($"User with Id: {userIdToDelete} deleted");
#endregion

