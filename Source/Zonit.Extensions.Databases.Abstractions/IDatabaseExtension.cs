namespace Zonit.Extensions.Databases;

public interface IDatabaseExtension<TExtension> where TExtension : class
{
    Task<TExtension> InitializeAsync(Guid id, CancellationToken cancellationToken = default);
}
