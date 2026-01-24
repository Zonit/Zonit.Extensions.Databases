using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Extensions;

public class UserExtension : IDatabaseExtension<UserModel>
{
    public async Task<UserModel?> InitializeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = new UserModel
        {
            Id = id,
            Name = "UserName",
        };

        return await Task.FromResult(model);
    }
}

