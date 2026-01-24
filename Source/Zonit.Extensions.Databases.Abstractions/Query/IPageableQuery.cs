namespace Zonit.Extensions.Databases;

/// <summary>
/// Query that can be paginated with Skip and Take.
/// </summary>
/// <typeparam name="TEntity">The entity type being queried.</typeparam>
public interface IPageableQuery<TEntity> : IExecutableQuery<TEntity>
    where TEntity : class
{
    /// <summary>
    /// Skips N records. Must be followed by Take() or execution.
    /// Cannot be called twice (returns ISkippedQuery without Skip method).
    /// </summary>
    /// <param name="count">Number of records to skip.</param>
    /// <returns>Query that can Take or execute.</returns>
    ISkippedQuery<TEntity> Skip(int count);

    /// <summary>
    /// Takes N records. Cannot be called after Skip (use ISkippedQuery.Take instead).
    /// </summary>
    /// <param name="count">Number of records to take.</param>
    /// <returns>Executable query (no more pagination allowed).</returns>
    IExecutableQuery<TEntity> Take(int count);
}
