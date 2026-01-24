namespace Zonit.Extensions.Databases;

/// <summary>
/// Non-generic base interface for database extensions.
/// Used for AOT-safe service resolution without MakeGenericType.
/// </summary>
public interface IDatabaseExtension
{
    /// <summary>
    /// Loads the extension data for the specified entity ID.
    /// </summary>
    /// <param name="id">The foreign key value to load data for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded extension data as object, or null if not found.</returns>
    Task<object?> InitializeAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for loading external/computed data for entity properties.
/// Used by Extension() method in query builder.
/// Implement this interface to load data from external APIs, microservices, or computed values.
/// </summary>
/// <typeparam name="TExtension">The type of the extension data to load.</typeparam>
/// <example>
/// <code>
/// public class UserExtension : IDatabaseExtension&lt;UserModel&gt;
/// {
///     private readonly IUserApiClient _userApi;
///     
///     public UserExtension(IUserApiClient userApi) => _userApi = userApi;
///     
///     public async Task&lt;UserModel?&gt; InitializeAsync(Guid id, CancellationToken ct)
///     {
///         return await _userApi.GetUserAsync(id, ct);
///     }
/// }
/// 
/// // Usage in query:
/// var blogs = await _blogRepository.AsQuery()
///     .Extension(x => x.Author)  // Loads Author from external service
///     .GetListAsync();
/// </code>
/// </example>
public interface IDatabaseExtension<TExtension> : IDatabaseExtension where TExtension : class
{
    /// <summary>
    /// Loads the extension data for the specified entity ID.
    /// </summary>
    /// <param name="id">The foreign key value to load data for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loaded extension data, or null if not found.</returns>
    new Task<TExtension?> InitializeAsync(Guid id, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    async Task<object?> IDatabaseExtension.InitializeAsync(Guid id, CancellationToken cancellationToken)
        => await InitializeAsync(id, cancellationToken);
}
