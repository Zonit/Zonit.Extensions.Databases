using Zonit.Extensions.Databases.Examples.Entities;

namespace Zonit.Extensions.Databases.Examples.Extensions;

public class OrganizationExtension : IDatabaseExtension<OrganizationModel>
{
    public async Task<OrganizationModel> InitializeAsync(Guid UserId, CancellationToken cancellationToken = default)
    {
        var model = new OrganizationModel
        { 
        };

        return await Task.FromResult(model);
    }
}

