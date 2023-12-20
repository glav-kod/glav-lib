using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GlavLib.SourceGenerators;

[Generator]
public class EnumObjectSourceGenerator : IIncrementalGenerator
{
    private const string EnumObjectItemAttribute = "GlavLib.Abstractions.DataTypes.EnumObjectItemAttribute";
    private const string EnumObject              = "GlavLib.Abstractions.DataTypes.EnumObject";

    // ReSharper disable once InconsistentNaming
    private const string IEnumObject = "GlavLib.Abstractions.DataTypes.IEnumObject";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
                              .CreateSyntaxProvider(predicate: (s,   _) => IsTransformCandidate(s),
                                                    transform: (ctx, _) => GetEnumRegistration(ctx))
                              .Where(t => t is not null)
                              .Select((t, _) => (EnumRegistration)t!);

        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
                                     (ctx, t) => GenerateCode(ctx, t.Right));
    }

    private static bool IsTransformCandidate(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
            return false;

        //Должен быть как минимум один базовый класс - EnumObject
        var baseTypes = classDeclarationSyntax.BaseList?.Types;
        if (baseTypes is null)
            return false;

        //Должен быть как минимум один EnumObjectItem аттрибут
        return classDeclarationSyntax.AttributeLists.Count > 0;
    }

    private static EnumRegistration? GetEnumRegistration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        var baseTypes = classDeclarationSyntax.BaseList?.Types;
        if (baseTypes is null)
            return null;

        var inheritedFromEnumObject = false;
        foreach (var baseTypeSyntax in baseTypes.OfType<SimpleBaseTypeSyntax>())
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(baseTypeSyntax.Type);
            var type     = typeInfo.Type;
            if (type is null)
                continue;

            var baseTypeName = type.ToDisplayString();
            if (baseTypeName == EnumObject)
            {
                inheritedFromEnumObject = true;
                break;
            }
        }

        if (!inheritedFromEnumObject)
            return null;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
        if (classSymbol is null)
            return null;

        var enumItems = new List<EnumItem>();

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass is null)
                continue;

            var attributeName = attributeData.AttributeClass.ToDisplayString();
            if (attributeName != EnumObjectItemAttribute)
                continue;

            var fieldNameArg   = attributeData.ConstructorArguments[0];
            var valueArg       = attributeData.ConstructorArguments[1];
            var displayNameArg = attributeData.ConstructorArguments[2];

            enumItems.Add(new EnumItem(fieldName: fieldNameArg.Value!.ToString(),
                                       value: valueArg.Value!.ToString(),
                                       displayName: displayNameArg.Value!.ToString()));
        }

        string? ns = null;

        if (!classSymbol.ContainingNamespace.IsGlobalNamespace)
            ns = classSymbol.ContainingNamespace.ToDisplayString();

        return new EnumRegistration(classNamespace: ns,
                                    className: classSymbol.Name,
                                    items: enumItems);
    }

    private static void GenerateCode(SourceProductionContext          context,
                                     ImmutableArray<EnumRegistration> enumRegistrations)
    {
        foreach (var enumRegistration in enumRegistrations)
        {
            var enumObjectClass = GenerateClass(enumRegistration);

            var compilationUnit = CompilationUnit();

            var ns = enumRegistration.ClassNamespace;
            if (ns is null)
            {
                compilationUnit = compilationUnit.AddMembers(enumObjectClass);
            }
            else
            {
                var namespaceDeclaration = NamespaceDeclaration(IdentifierName(ns));
                namespaceDeclaration = namespaceDeclaration.AddMembers(enumObjectClass);

                compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);
            }


            var code = compilationUnit.NormalizeWhitespace()
                                      .WithLeadingTrivia(ParseLeadingTrivia("/// <auto-generated/>\n"))
                                      .ToFullString();

            var className = enumObjectClass.Identifier;

            context.AddSource($"{className}.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }

    private static ClassDeclarationSyntax GenerateClass(EnumRegistration enumRegistration)
    {
        var className = enumRegistration.ClassName;

        var keys   = new List<MemberDeclarationSyntax>();
        var fields = new List<MemberDeclarationSyntax>();

        var switchBuilder = new StringBuilder();

        foreach (var enumItem in enumRegistration.Items)
        {
            keys.Add(ParseMemberDeclaration($"""public const string {enumItem.FieldName}Key = "{enumItem.Value}";""")!);
            fields.Add(ParseMemberDeclaration($"public static readonly {className} {enumItem.FieldName} = new ({enumItem.FieldName}Key, \"{enumItem.DisplayName}\");")!);

            switchBuilder.AppendLine($"{enumItem.FieldName}Key => {enumItem.FieldName},");
        }

        var constructorMember = ParseMemberDeclaration($$"""
                                                         private {{className}}(string key, string displayName) : base(key, displayName)
                                                         {
                                                         }
                                                         """);

        var createMethod = (MethodDeclarationSyntax?)ParseMemberDeclaration($$"""
                                                                              public static {{className}} Create(string key)
                                                                              {
                                                                                  return key switch
                                                                                  {
                                                                                    {{switchBuilder}}
                                                                                    _ => throw new global::System.InvalidOperationException($"Неожиданный ключ '{key}'")
                                                                                  };
                                                                              }
                                                                              """);

        var fullClassName = enumRegistration.GetFullClassName();
        var iEnumObject   = ParseTypeName($"{IEnumObject}<{fullClassName}>");

        return ClassDeclaration(className)
               .AddBaseListTypes(SimpleBaseType(iEnumObject))
               .AddModifiers(Token(SyntaxKind.PartialKeyword))
               .AddMembers(keys.ToArray())
               .AddMembers(fields.ToArray())
               .AddMembers(constructorMember!)
               .AddMembers(createMethod!);
    }

    private struct EnumRegistration
    {
        public string? ClassNamespace { get; }

        public string ClassName { get; }

        public List<EnumItem> Items { get; }

        public EnumRegistration(string? classNamespace, string className, List<EnumItem> items)
        {
            ClassNamespace = classNamespace;
            ClassName = className;
            Items = items;
        }

        public string GetFullClassName()
        {
            if (ClassNamespace is null)
                return $"global::{ClassName}";

            return $"{ClassNamespace}.{ClassName}";
        }


        private bool Equals(EnumRegistration other)
        {
            return ClassNamespace == other.ClassNamespace && ClassName == other.ClassName && Items.SequenceEqual(other.Items);
        }

        public override bool Equals(object? obj)
        {
            return obj is EnumRegistration other && Equals(other);
        }

        public override int GetHashCode()
        {
            var sb = new StringBuilder();
            foreach (var enumItem in Items)
                sb.Append($"{enumItem.FieldName}{enumItem.Value}");

            unchecked
            {
                var hashCode = ClassNamespace != null ? ClassNamespace.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ ClassName.GetHashCode();
                hashCode = (hashCode * 397) ^ sb.ToString().GetHashCode();
                return hashCode;
            }
        }
    }

    private struct EnumItem
    {
        public string FieldName { get; }

        public string Value { get; }

        public string DisplayName { get; }

        public EnumItem(string fieldName, string value, string displayName)
        {
            FieldName = fieldName;
            Value = value;
            DisplayName = displayName;
        }
    }
}