using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Zonit.Extensions.Databases.SourceGenerators;

/// <summary>
/// Source generator that creates AOT-compatible property accessors for database entities.
/// Automatically discovers entities from DbSet&lt;T&gt; properties in DbContext classes.
/// No attributes required - entities are discovered from DbContext definitions.
/// </summary>
[Generator]
public class EntityAccessorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all classes inheriting from DbContext
        var dbContextDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsClassWithBaseType(s),
                transform: static (ctx, _) => GetDbContextInfo(ctx))
            .Where(static m => m is not null);

        // Combine with compilation to get entity info
        var compilationAndContexts = context.CompilationProvider
            .Combine(dbContextDeclarations.Collect());

        // Generate the source
        context.RegisterSourceOutput(compilationAndContexts,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsClassWithBaseType(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classDecl &&
               classDecl.BaseList is not null;
    }

    private static DbContextInfo? GetDbContextInfo(GeneratorSyntaxContext context)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;

        if (symbol is null)
            return null;

        // Check if inherits from DbContext (directly or via DatabaseContextBase)
        if (!InheritsFromDbContext(symbol))
            return null;

        // Get all DbSet<T> properties
        var dbSetEntities = new List<EntityInfo>();

        foreach (var member in symbol.GetMembers())
        {
            if (member is not IPropertySymbol prop)
                continue;

            if (prop.Type is not INamedTypeSymbol propType)
                continue;

            // Check if it's DbSet<T>
            if (propType.Name != "DbSet" || propType.TypeArguments.Length != 1)
                continue;

            var entityType = propType.TypeArguments[0] as INamedTypeSymbol;
            if (entityType is null)
                continue;

            var entityInfo = ExtractEntityInfo(entityType);
            if (entityInfo is not null)
                dbSetEntities.Add(entityInfo);
        }

        if (dbSetEntities.Count == 0)
            return null;

        return new DbContextInfo
        {
            ContextName = symbol.Name,
            FullContextName = symbol.ToDisplayString(),
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            Entities = dbSetEntities
        };
    }

    private static bool InheritsFromDbContext(INamedTypeSymbol symbol)
    {
        var current = symbol.BaseType;
        while (current is not null)
        {
            if (current.Name == "DbContext" ||
                current.Name == "DatabaseContextBase" ||
                current.Name.StartsWith("DatabaseContextBase"))
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static EntityInfo? ExtractEntityInfo(INamedTypeSymbol entityType)
    {
        var properties = entityType.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                        !p.IsStatic &&
                        !p.IsIndexer)
            .Select(p => new PropertyInfo
            {
                Name = p.Name,
                TypeName = p.Type.ToDisplayString(),
                IsNullable = p.NullableAnnotation == NullableAnnotation.Annotated,
                HasSetter = p.SetMethod is not null && p.SetMethod.DeclaredAccessibility == Accessibility.Public,
                IsReferenceType = p.Type.IsReferenceType
            })
            .ToList();

        return new EntityInfo
        {
            ClassName = entityType.Name,
            FullClassName = entityType.ToDisplayString(),
            Namespace = entityType.ContainingNamespace.ToDisplayString(),
            Properties = properties
        };
    }

    private static void Execute(
        Compilation compilation,
        ImmutableArray<DbContextInfo?> contexts,
        SourceProductionContext context)
    {
        if (contexts.IsDefaultOrEmpty)
            return;

        // Collect all unique entities from all DbContexts
        var allEntities = contexts
            .Where(c => c is not null)
            .SelectMany(c => c!.Entities)
            .GroupBy(e => e.FullClassName)
            .Select(g => g.First())
            .ToList();

        if (allEntities.Count == 0)
            return;

        // Generate individual accessor for each entity
        foreach (var entity in allEntities)
        {
            var source = GenerateEntityAccessor(entity);
            var safeName = entity.FullClassName.Replace(".", "_").Replace("<", "_").Replace(">", "_");
            context.AddSource($"{safeName}.Accessor.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        // Generate registry
        var registrySource = GenerateAccessorRegistry(allEntities);
        context.AddSource("EntityAccessorRegistry.g.cs", SourceText.From(registrySource, Encoding.UTF8));
    }

    private static string GenerateEntityAccessor(EntityInfo entity)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("#pragma warning disable IL2075 // Type.GetProperty may be trimmed - Source Generator generates code for known types");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Reflection;");
        sb.AppendLine("using Zonit.Extensions.Databases.Accessors;");
        sb.AppendLine();
        sb.AppendLine($"namespace {entity.Namespace};");
        sb.AppendLine();
        sb.AppendLine($"/// <summary>");
        sb.AppendLine($"/// AOT-safe property accessor for {entity.ClassName}.");
        sb.AppendLine($"/// </summary>");
        sb.AppendLine($"internal sealed class {entity.ClassName}Accessor : IEntityAccessor<{entity.ClassName}>");
        sb.AppendLine("{");

        // Static PropertyInfo cache
        sb.AppendLine("    private static readonly Lazy<Dictionary<string, PropertyInfo?>> _propertyCache = new(() => new()");
        sb.AppendLine("    {");
        foreach (var prop in entity.Properties)
        {
            sb.AppendLine($"        [\"{prop.Name}\"] = typeof({entity.FullClassName}).GetProperty(\"{prop.Name}\"),");
        }
        sb.AppendLine("    });");
        sb.AppendLine();

        // GetPropertyInfo
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    public PropertyInfo? GetPropertyInfo(string propertyName)");
        sb.AppendLine("    {");
        sb.AppendLine("        return _propertyCache.Value.TryGetValue(propertyName, out var prop) ? prop : null;");
        sb.AppendLine("    }");
        sb.AppendLine();

        // GetValue
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public object? GetValue({entity.FullClassName} entity, string propertyName)");
        sb.AppendLine("    {");
        if (entity.Properties.Count > 0)
        {
            sb.AppendLine("        return propertyName switch");
            sb.AppendLine("        {");
            foreach (var prop in entity.Properties)
            {
                sb.AppendLine($"            \"{prop.Name}\" => entity.{prop.Name},");
            }
            sb.AppendLine("            _ => null");
            sb.AppendLine("        };");
        }
        else
        {
            sb.AppendLine("        return null;");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // SetValue
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine($"    public bool SetValue({entity.FullClassName} entity, string propertyName, object? value)");
        sb.AppendLine("    {");
        var settableProps = entity.Properties.Where(p => p.HasSetter).ToList();
        if (settableProps.Count > 0)
        {
            sb.AppendLine("        switch (propertyName)");
            sb.AppendLine("        {");
            foreach (var prop in settableProps)
            {
                var cast = prop.IsNullable || prop.IsReferenceType
                    ? $"({prop.TypeName}?)value"
                    : $"({prop.TypeName})value!";
                sb.AppendLine($"            case \"{prop.Name}\": entity.{prop.Name} = {cast}; return true;");
            }
            sb.AppendLine("            default: return false;");
            sb.AppendLine("        }");
        }
        else
        {
            sb.AppendLine("        return false;");
        }
        sb.AppendLine("    }");
        sb.AppendLine();

        // Non-generic interface implementation
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    object? IEntityAccessor.GetValue(object entity, string propertyName)");
        sb.AppendLine($"        => GetValue(({entity.FullClassName})entity, propertyName);");
        sb.AppendLine();
        sb.AppendLine("    /// <inheritdoc />");
        sb.AppendLine("    bool IEntityAccessor.SetValue(object entity, string propertyName, object? value)");
        sb.AppendLine($"        => SetValue(({entity.FullClassName})entity, propertyName, value);");

        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GenerateAccessorRegistry(List<EntityInfo> entities)
    {
        var sb = new StringBuilder();

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Frozen;");
        sb.AppendLine("using Zonit.Extensions.Databases.Accessors;");
        sb.AppendLine();

        // Add usings for all entity namespaces
        var namespaces = entities.Select(e => e.Namespace).Distinct().OrderBy(n => n);
        foreach (var ns in namespaces)
        {
            sb.AppendLine($"using {ns};");
        }

        sb.AppendLine();
        sb.AppendLine("namespace Zonit.Extensions.Databases.Accessors;");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine("/// AOT-safe registry of all entity accessors. Auto-generated partial class.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine("public static partial class EntityAccessorRegistry");
        sb.AppendLine("{");

        // Static registry with frozen dictionary for performance
        sb.AppendLine("    private static readonly FrozenDictionary<Type, IEntityAccessor> _generatedAccessors = new Dictionary<Type, IEntityAccessor>");
        sb.AppendLine("    {");
        foreach (var entity in entities)
        {
            sb.AppendLine($"        [typeof({entity.FullClassName})] = new {entity.Namespace}.{entity.ClassName}Accessor(),");
        }
        sb.AppendLine("    }.ToFrozenDictionary();");
        sb.AppendLine();

        // Implement partial method
        sb.AppendLine("    static partial void GetGeneratedAccessorCore(Type entityType, ref IEntityAccessor? result)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (_generatedAccessors.TryGetValue(entityType, out var accessor))");
        sb.AppendLine("            result = accessor;");
        sb.AppendLine("    }");

        sb.AppendLine("}");

        return sb.ToString();
    }
}

internal class DbContextInfo
{
    public string ContextName { get; set; } = "";
    public string FullContextName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public List<EntityInfo> Entities { get; set; } = new();
}

internal class EntityInfo
{
    public string ClassName { get; set; } = "";
    public string FullClassName { get; set; } = "";
    public string Namespace { get; set; } = "";
    public List<PropertyInfo> Properties { get; set; } = new();
}

internal class PropertyInfo
{
    public string Name { get; set; } = "";
    public string TypeName { get; set; } = "";
    public bool IsNullable { get; set; }
    public bool HasSetter { get; set; }
    public bool IsReferenceType { get; set; }
}
