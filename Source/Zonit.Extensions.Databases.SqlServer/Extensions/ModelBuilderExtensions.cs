using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using Zonit.Extensions;

namespace Zonit.Extensions.Databases.SqlServer.Extensions;

/// <summary>
/// Extension methods for configuring EF Core ModelBuilder with Value Object converters.
/// </summary>
public static class ModelBuilderExtensions
{
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
                else if (property.ClrType == typeof(Url))
                {
                    property.SetValueConverter(new UrlConverter());
                    property.SetMaxLength(2048); // Standard max URL length
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
            .Properties<Url>()
            .HaveConversion<UrlConverter>()
            .HaveMaxLength(2048);
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
