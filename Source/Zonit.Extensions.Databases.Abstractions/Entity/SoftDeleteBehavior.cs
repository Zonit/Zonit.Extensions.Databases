namespace Zonit.Extensions.Databases;

/// <summary>
/// Specifies how soft-deleted entities should be handled in queries.
/// </summary>
public enum SoftDeleteBehavior
{
    /// <summary>
    /// Include all entities regardless of deletion status.
    /// </summary>
    IncludeAll = 0,

    /// <summary>
    /// Exclude soft-deleted entities (default behavior).
    /// </summary>
    ExcludeDeleted = 1,

    /// <summary>
    /// Include only soft-deleted entities.
    /// </summary>
    OnlyDeleted = 2
}
