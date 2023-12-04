using System.Collections.Immutable;
using System.Text;
using GlavLib.SourceGenerators.Extensions;
using GlavLib.SourceGenerators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GlavLib.SourceGenerators;

[Generator]
public sealed class AutofacSourceGenerator : IIncrementalGenerator
{
    private const string CommandHandler     = "GlavLib.App.Commands.CommandHandler";
    private const string UnitCommandHandler = "GlavLib.App.Commands.UnitCommandHandler";
    private const string DomainEventHandler = "GlavLib.App.DomainEvents.DomainEventHandler";

    private const string SingleInstanceAttribute = "GlavLib.Basics.DI.SingleInstanceAttribute";
    private const string TransientAttributeName  = "GlavLib.Basics.DI.TransientAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
                              .CreateSyntaxProvider(predicate: (s,   _) => IsTransformCandidate(s),
                                                    transform: (ctx, _) => GetTypeRegistration(ctx))
                              .Where(t => t is not null)
                              .Select((t, _) => (TypeRegistration)t!);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
                                     (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private static bool IsTransformCandidate(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
            return false;

        //Возможно класс помечен Register* аттрибутом
        if (classDeclarationSyntax.AttributeLists.Count > 0)
            return true;

        //Возможно класс наследован от CommandHandler или UnitCommandHandler
        var baseTypes = classDeclarationSyntax.BaseList?.Types;
        if (baseTypes is null)
            return false;

        return true;
    }

    private static TypeRegistration? GetTypeRegistration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            return null;

        var fullClassName = classSymbol.ToDisplayString();

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            var attributeTypeSymbol = attributeData.AttributeClass;
            if (attributeTypeSymbol is null)
                continue;

            var attributeTypeFullName = attributeTypeSymbol.GetFullNameWithoutGenericArguments();

            if (attributeTypeSymbol.TypeArguments.Length > 0)
            {
                var asTypeSymbol = attributeTypeSymbol.TypeArguments[0];
                var asType       = asTypeSymbol.ToDisplayString();

                switch (attributeTypeFullName)
                {
                    case SingleInstanceAttribute:
                        return TypeRegistration.AsType(fullClassName, asType, singleInstance: true);

                    case TransientAttributeName:
                        return TypeRegistration.AsType(fullClassName, asType, singleInstance: false);
                }
            }

            switch (attributeTypeFullName)
            {
                case SingleInstanceAttribute:
                    return TypeRegistration.AsSelf(fullClassName, singleInstance: true);

                case TransientAttributeName:
                    return TypeRegistration.AsSelf(fullClassName, singleInstance: false);
            }
        }

        var baseType = classSymbol.BaseType;
        if (baseType is null)
            return null;

        var baseTypeFullName = baseType.ToDisplayString();
        var baseTypeName     = baseType.GetFullNameWithoutGenericArguments();

        switch (baseTypeName)
        {
            case CommandHandler:
                return TypeRegistration.AsType(fullClassName, baseTypeFullName, singleInstance: true);

            case UnitCommandHandler:
                return TypeRegistration.AsType(fullClassName, baseTypeFullName, singleInstance: true);

            case DomainEventHandler:
                return TypeRegistration.AsType(fullClassName, baseTypeFullName, singleInstance: true);
        }

        return null;
    }

    private static void GenerateCode(SourceProductionContext          context,
                                     Compilation                      compilation,
                                     ImmutableArray<TypeRegistration> registrations)
    {
        var registerMethodsBody = new StringBuilder();

        for (var i = 0; i < registrations.Length; i++)
        {
            var typeRegistration = registrations[i];

            var registerStatement = BuildTypeRegistration(typeRegistration);

            registerMethodsBody.Append("        ");
            registerMethodsBody.Append(registerStatement);

            if (i < registrations.Length - 1)
                registerMethodsBody.AppendLine();
        }

        if (registerMethodsBody.Length == 0)
            return;

        var source = $$"""
                       /// <auto-generated/>

                       using Autofac;

                       namespace {{compilation.Assembly.Name}};

                       internal sealed class CompositionRoot : Module
                       {
                           protected override void Load(ContainerBuilder builder)
                           {
                       {{registerMethodsBody}}
                           }
                       }
                       """;

        context.AddSource("CompositionRoot.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string BuildTypeRegistration(TypeRegistration typeRegistration)
    {
        var fullClassName = typeRegistration.FullClassName;

        var sb = new StringBuilder($"builder.RegisterType<{fullClassName}>()");

        if (typeRegistration.RegisterAs is not null)
        {
            var registerAs = typeRegistration.RegisterAs;
            sb.Append($".As<{registerAs}>()");
        }

        if (typeRegistration.SingleInstance)
            sb.Append(".SingleInstance()");

        sb.Append(';');

        return sb.ToString();
    }

    private struct TypeRegistration : IEquatable<TypeRegistration>
    {
        public string FullClassName { get; private set; }

        public bool SingleInstance { get; private set; }

        public string? RegisterAs { get; private set; }

        public bool Equals(TypeRegistration other)
        {
            return FullClassName.Equals(other.FullClassName)
                   && SingleInstance == other.SingleInstance
                   && RegisterAs == other.RegisterAs;
        }

        public override bool Equals(object? obj)
        {
            return obj is TypeRegistration other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FullClassName.GetHashCode();
                hashCode = (hashCode * 397) ^ SingleInstance.GetHashCode();
                hashCode = (hashCode * 397) ^ (RegisterAs != null ? RegisterAs.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static TypeRegistration AsSelf(string fullClassName,
                                              bool   singleInstance)
        {
            return new TypeRegistration
            {
                FullClassName = fullClassName,
                SingleInstance = singleInstance
            };
        }

        public static TypeRegistration AsType(string fullClassName,
                                              string asType,
                                              bool   singleInstance)
        {
            return new TypeRegistration
            {
                FullClassName = fullClassName,
                RegisterAs = asType,
                SingleInstance = singleInstance
            };
        }
    }
}