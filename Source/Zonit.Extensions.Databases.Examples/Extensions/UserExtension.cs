using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Extensions;

public class UserExtension : IDatabaseExtension<UserModel>
{
    public async Task<UserModel> InicjalizeAsync(Guid UserId, CancellationToken cancellationToken = default)
    {
        var model = new UserModel { 
            Id = UserId,
            Name = "UserName",
        };

        return await Task.FromResult(model);
    }
}

