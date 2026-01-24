namespace Zonit.Extensions.Databases;

/// <summary>
/// Query after Skip that can only Take or execute.
/// Prevents Skip().Skip() anti-pattern.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public interface ISkippedQuery<TEntity> : IExecutableQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Takes N records after skipping.
    /// </summary>
    /// <param name="count">Number of records to take.</param>
    /// <returns>Executable query.</returns>
    IExecutableQuery<TEntity> Take(int count);
}
