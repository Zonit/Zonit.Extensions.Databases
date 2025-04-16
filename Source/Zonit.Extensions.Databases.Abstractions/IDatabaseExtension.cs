namespace Zonit.Extensions.Databases;

public interface IDatabaseExtension<TExtension> where TExtension : class
{
    Task<TExtension> InicjalizeAsync(Guid id, CancellationToken cancellationToken = default);
}
