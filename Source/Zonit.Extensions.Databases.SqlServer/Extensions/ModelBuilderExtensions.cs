using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Reflection;
using Zonit.Extensions;

namespace Zonit.Extensions.Databases.SqlServer.Extensions;

/// <summary>
/// Extension methods for configuring EF Core ModelBuilder with Value Object converters.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Automatically ignores extension properties (properties ending with "Model" that have corresponding "Id" property).
    /// This eliminates the need for [NotMapped] attribute on extension properties.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance.</param>
    /// <returns>The ModelBuilder instance for chaining.</returns>
    /// <example>
    /// <code>
    /// // Before: Required [NotMapped] attribute
    /// public class Blog
    /// {
    ///     [NotMapped]
    ///     public UserModel? User { get; set; }
    ///     public Guid? UserId { get; set; }
    /// }
    /// 
    /// // After: No attribute needed, just call IgnoreExtensionProperties()
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     modelBuilder.IgnoreExtensionProperties();
    ///     base.OnModelCreating(modelBuilder);
    /// }
    /// </code>
    /// </example>
    public static ModelBuilder IgnoreExtensionProperties(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var properties = clrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Find all properties that look like extensions (PropertyName + PropertyNameId pattern)
            var propertyNames = new HashSet<string>(properties.Select(p => p.Name));

            foreach (var property in properties)
            {
                var propertyName = property.Name;

                // Check if this property has a corresponding Id property
                // e.g., "User" has "UserId", "Organization" has "OrganizationId"
                var idPropertyName = $"{propertyName}Id";

                if (propertyNames.Contains(idPropertyName))
                {
                    // This is likely an extension property - check if it's a complex type
                    var propertyType = property.PropertyType;
                    var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

                    // Ignore if it's a class (not a value type, not a primitive, not a built-in type)
                    if (underlyingType.IsClass &&
                        underlyingType != typeof(string) &&
                        !underlyingType.IsPrimitive)
                    {
                        modelBuilder.Entity(clrType).Ignore(propertyName);
                    }
                }
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures table naming convention for all entities.
    /// Generates table names in format: [schema].[prefix.EntityName]
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance.</param>
    /// <param name="schema">The database schema (e.g., "Zonit", "dbo").</param>
    /// <param name="prefix">Optional prefix for table names (e.g., "Examples", "Blog").</param>
    /// <returns>The ModelBuilder instance for chaining.</returns>
    /// <example>
    /// <code>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     // Tables will be: [Zonit].[Examples.Blog], [Zonit].[Examples.User]
    ///     modelBuilder.SetPrefix("Zonit", "Examples");
    ///     
    ///     // Or without prefix: [Zonit].[Blog], [Zonit].[User]
    ///     modelBuilder.SetPrefix("Zonit");
    ///     
    ///     base.OnModelCreating(modelBuilder);
    /// }
    /// </code>
    /// </example>
    public static ModelBuilder SetPrefix(this ModelBuilder modelBuilder, string schema, string? prefix = null)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var entityName = entityType.ClrType.Name;
            var tableName = string.IsNullOrWhiteSpace(prefix)
                ? entityName
                : $"{prefix}.{entityName}";

            modelBuilder.Entity(entityType.Name).ToTable(tableName, schema);
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures table naming convention with a custom naming function.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance.</param>
    /// <param name="tableNameFactory">Function that generates table name from entity type.</param>
    /// <param name="schema">The database schema.</param>
    /// <returns>The ModelBuilder instance for chaining.</returns>
    /// <example>
    /// <code>
    /// modelBuilder.SetPrefix(
    ///     entityType => $"App_{entityType.Name}",
    ///     schema: "dbo");
    /// </code>
    /// </example>
    public static ModelBuilder SetPrefix(
        this ModelBuilder modelBuilder,
        Func<Type, string> tableNameFactory,
        string schema)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        ArgumentNullException.ThrowIfNull(tableNameFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(schema);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = tableNameFactory(entityType.ClrType);
            modelBuilder.Entity(entityType.Name).ToTable(tableName, schema);
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures global value converters for all Zonit Value Objects.
    /// Call this method in your DbContext's OnModelCreating or ConfigureConventions.
    /// </summary>
    /// <param name="modelBuilder">The ModelBuilder instance.</param>
    /// <returns>The ModelBuilder instance for chaining.</returns>
    public static ModelBuilder UseZonitValueObjects(this ModelBuilder modelBuilder)
    {
        // Configure Culture converter
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(Culture))
                {
                    property.SetValueConverter(new CultureConverter());
                    property.SetMaxLength(10);
                }
                else if (property.ClrType == typeof(UrlSlug))
                {
                    property.SetValueConverter(new UrlSlugConverter());
                    property.SetMaxLength(200);
                }
                else if (property.ClrType == typeof(Title))
                {
                    property.SetValueConverter(new TitleConverter());
                    property.SetMaxLength(Title.MaxLength);
                }
                else if (property.ClrType == typeof(Description))
                {
                    property.SetValueConverter(new DescriptionConverter());
                    property.SetMaxLength(Description.MaxLength);
                }
                else if (property.ClrType == typeof(Content))
                {
                    property.SetValueConverter(new ContentConverter());
                    // Content uses nvarchar(max) - no length limit
                }
                else if (property.ClrType == typeof(Price))
                {
                    property.SetValueConverter(new PriceConverter());
                    property.SetPrecision(19);
                    property.SetScale(8);
                }
                else if (property.ClrType == typeof(Money))
                {
                    property.SetValueConverter(new MoneyConverter());
                    property.SetPrecision(19);
                    property.SetScale(8);
                }
                else if (property.ClrType == typeof(Url))
                {
                    property.SetValueConverter(new UrlConverter());
                    property.SetMaxLength(2048); // Standard max URL length
                }
                else if (property.ClrType == typeof(Asset))
                {
                    property.SetValueConverter(new AssetBytesConverter());
                    // Asset is stored as byte[] with embedded header
                }
                else if (property.ClrType == typeof(Color))
                {
                    property.SetValueConverter(new ColorConverter());
                    property.SetMaxLength(100); // OKLCH format with alpha
                }
            }
        }

        return modelBuilder;
    }

    /// <summary>
    /// Configures conventions for Zonit Value Objects using EF Core 7+ ConfigureConventions.
    /// Use this in your DbContext's ConfigureConventions method for automatic configuration.
    /// </summary>
    /// <param name="configurationBuilder">The ModelConfigurationBuilder instance.</param>
    public static void UseZonitValueObjectConventions(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Culture>()
            .HaveConversion<CultureConverter>()
            .HaveMaxLength(10);

        configurationBuilder
            .Properties<UrlSlug>()
            .HaveConversion<UrlSlugConverter>()
            .HaveMaxLength(200);

        configurationBuilder
            .Properties<Title>()
            .HaveConversion<TitleConverter>()
            .HaveMaxLength(Title.MaxLength);

        configurationBuilder
            .Properties<Description>()
            .HaveConversion<DescriptionConverter>()
            .HaveMaxLength(Description.MaxLength);

        configurationBuilder
            .Properties<Content>()
            .HaveConversion<ContentConverter>();
        // Content uses nvarchar(max) - no length limit

        configurationBuilder
            .Properties<Price>()
            .HaveConversion<PriceConverter>()
            .HavePrecision(19, 8);

        configurationBuilder
            .Properties<Money>()
            .HaveConversion<MoneyConverter>()
            .HavePrecision(19, 8);

        configurationBuilder
            .Properties<Url>()
            .HaveConversion<UrlConverter>()
            .HaveMaxLength(2048);

        configurationBuilder
            .Properties<Asset>()
            .HaveConversion<AssetBytesConverter>();
        // Asset is stored as byte[] with embedded header

        configurationBuilder
            .Properties<Color>()
            .HaveConversion<ColorConverter>()
            .HaveMaxLength(100);
        // Color stored as OKLCH string
    }
}

/// <summary>
/// Value converter for Culture value object.
/// Handles null/empty values from database by returning default (empty) Culture.
/// </summary>
public class CultureConverter : ValueConverter<Culture, string>
{
    public CultureConverter()
        : base(
            v => v.Value,
            v => string.IsNullOrWhiteSpace(v) ? default : new Culture(v))
    {
    }
}

/// <summary>
/// Value converter for UrlSlug value object.
/// Handles null/empty values from database by returning default (empty) UrlSlug.
/// </summary>
public class UrlSlugConverter : ValueConverter<UrlSlug, string>
{
    public UrlSlugConverter()
        : base(
            v => v.Value,
            v => string.IsNullOrWhiteSpace(v) ? default : new UrlSlug(v))
    {
    }
}

/// <summary>
/// Value converter for Title value object.
/// Handles null/empty values from database by returning default (empty) Title.
/// </summary>
public class TitleConverter : ValueConverter<Title, string>
{
    public TitleConverter()
        : base(
            v => v.Value,
            v => string.IsNullOrWhiteSpace(v) ? default : new Title(v))
    {
    }
}

/// <summary>
/// Value converter for Description value object.
/// Handles null/empty values from database by returning default (empty) Description.
/// </summary>
public class DescriptionConverter : ValueConverter<Description, string>
{
    public DescriptionConverter()
        : base(
            v => v.Value,
            v => string.IsNullOrWhiteSpace(v) ? default : new Description(v))
    {
    }
}

/// <summary>
/// Value converter for Content value object.
/// Handles null/empty values from database by returning default (empty) Content.
/// </summary>
public class ContentConverter : ValueConverter<Content, string>
{
    public ContentConverter()
        : base(
            v => v.Value,
            v => string.IsNullOrWhiteSpace(v) ? default : new Content(v))
    {
    }
}

/// <summary>
/// Value converter for Price value object.
/// </summary>
public class PriceConverter : ValueConverter<Price, decimal>
{
    public PriceConverter()
        : base(
            v => v.Value,
            v => new Price(v, true)) // allowNegative = true (cannot use named arguments in expression trees)
    {
    }
}

/// <summary>
/// Value converter for Money value object.
/// Money allows negative values for balances, transactions, adjustments, etc.
/// </summary>
public class MoneyConverter : ValueConverter<Money, decimal>
{
    public MoneyConverter()
        : base(
            v => v.Value,
            v => new Money(v))
    {
    }
}

/// <summary>
/// Value converter for Url value object.
/// Handles null/empty values from database by returning default (empty) Url.
/// </summary>
public class UrlConverter : ValueConverter<Url, string>
{
    public UrlConverter()
        : base(
            v => v.Value,
            v => string.IsNullOrWhiteSpace(v) ? default : new Url(v))
    {
    }
}

/// <summary>
/// Value converter for Asset value object.
/// Stores Asset as byte[] with embedded header (name, mimeType).
/// Format: [4 bytes: header length][UTF-8 JSON header][file data]
/// </summary>
public class AssetBytesConverter : ValueConverter<Asset, byte[]>
{
    public AssetBytesConverter()
        : base(
            v => v.ToStorageBytes(),
            v => Asset.FromStorageBytes(v))
    {
    }
}

/// <summary>
/// Value converter for Color value object.
/// Stores Color as OKLCH CSS string for full precision.
/// Example: "oklch(65% 0.15 250)" or "oklch(65% 0.15 250 / 0.5)" with alpha.
/// </summary>
public class ColorConverter : ValueConverter<Color, string>
{
    public ColorConverter()
        : base(
            v => v.CssOklch,
            v => string.IsNullOrWhiteSpace(v) ? Color.Transparent : Color.Parse(v, null))
    {
    }
}
