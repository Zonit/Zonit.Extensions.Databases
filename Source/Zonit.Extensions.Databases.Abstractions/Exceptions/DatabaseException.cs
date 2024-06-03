namespace Zonit.Extensions.Databases.Abstractions.Exceptions;

// TODO: Znaleźć wszystkie exception i je nazwać
public class DatabaseException(string message) : Exception(message)
{
    public string? Type { get; }
    public object[]? Parameters { get; }

    public DatabaseException(string name, string message) : this(message)
        => Type = name;

    public DatabaseException(string name, string message, params object[] args) : this(name, message)
        => Parameters = args;

    public DatabaseException(string message, params object[] args) : this(message)
        => Parameters = args;
}
