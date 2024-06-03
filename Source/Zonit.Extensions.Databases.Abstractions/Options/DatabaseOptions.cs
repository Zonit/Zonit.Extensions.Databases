namespace Zonit.Extensions.Databases.Abstractions.Options;

/// <summary>
/// Database connection options
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Raw connection string <br/>
    /// If set, all other properties are ignored
    /// </summary>
    public string? Raw { get; init; }

    /// <summary>
    /// Server address
    /// </summary>
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// Port number
    /// </summary>
    public int Port { get; init; } = 1433;

    /// <summary>
    /// Database name
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// User name
    /// </summary>
    public string User { get; init; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; init; } = string.Empty;

    /// <summary>
    /// Additional parameters
    /// </summary>
    public string Parameters { get; set; } = string.Empty;
}
