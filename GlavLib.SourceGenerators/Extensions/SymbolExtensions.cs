using Microsoft.CodeAnalysis;

namespace GlavLib.SourceGenerators.Extensions;

public static class SymbolExtensions
{
    public static string GetFullNameWithoutGenericArguments(this INamedTypeSymbol typeSymbol)
    {
        var containingNamespace = typeSymbol.ContainingNamespace;
        var typeName            = typeSymbol.Name;

        if (containingNamespace.IsGlobalNamespace)
            return $"global::{typeName}";

        var typeNs = containingNamespace.ToDisplayString();

        return $"{typeNs}.{typeName}";
    }
}