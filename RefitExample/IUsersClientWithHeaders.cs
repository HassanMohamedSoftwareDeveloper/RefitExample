using Refit;
using RefitExample.Models;

namespace RefitExample;

[Headers("Authorization: Bearer")]
public interface IUsersClientWithHeaders
{
    [Get("/users")]
    Task<IEnumerable<User>> GetAll();
}
