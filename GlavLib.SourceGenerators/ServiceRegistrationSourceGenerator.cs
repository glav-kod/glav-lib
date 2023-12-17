using System.Collections.Immutable;
using System.Text;
using GlavLib.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace GlavLib.SourceGenerators;

[Generator]
public sealed class ServiceRegistrationSourceGenerator : IIncrementalGenerator
{
    // ReSharper disable once InconsistentNaming
    private const string IDomainEventHandler = "GlavLib.App.DomainEvents.IDomainEventHandler";

    private const string DomainEventHandlerAttribute = "GlavLib.Abstractions.DI.DomainEventHandlerAttribute";
    private const string SingleInstanceAttribute     = "GlavLib.Abstractions.DI.SingleInstanceAttribute";
    private const string TransientAttributeName      = "GlavLib.Abstractions.DI.TransientAttribute";
    private const string AddServicesFromAttribute    = "GlavLib.Abstractions.DI.AddServicesFromAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
                              .CreateSyntaxProvider(predicate: (s,   _) => IsTransformCandidate(s),
                                                    transform: (ctx, _) => GetTypeRegistration(ctx))
                              .SelectMany((r, _) => r);

        // Generate the source code.
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()),
                                     (ctx, t) => GenerateCode(ctx, t.Left, t.Right));
    }

    private static bool IsTransformCandidate(SyntaxNode node)
    {
        if (node is not ClassDeclarationSyntax classDeclarationSyntax)
            return false;

        //Возможно класс помечен аттрибутом из GlavLib.Abstractions.DI.*
        if (classDeclarationSyntax.AttributeLists.Count > 0)
            return true;

        return true;
    }

    private static IList<TypeRegistration> GetTypeRegistration(GeneratorSyntaxContext context)
    {
        var registrations = new List<TypeRegistration>();

        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol classSymbol)
            return registrations;

        var fullClassName = classSymbol.ToDisplayString();

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            var attributeTypeSymbol = attributeData.AttributeClass;
            if (attributeTypeSymbol is null)
                continue;

            var attributeTypeFullName = attributeTypeSymbol.GetFullNameWithoutGenericArguments();
            if (attributeTypeFullName == AddServicesFromAttribute)
            {
                var methodName = attributeData.ConstructorArguments[0].Value!;
                registrations.Add(TypeRegistration.AddServicesFromMethod(fullClassName,
                                                                         methodName.ToString()));
                continue;
            }

            string? asType = null;


            if (attributeTypeSymbol.TypeArguments.Length > 0)
            {
                var asTypeSymbol = attributeTypeSymbol.TypeArguments[0];
                asType = asTypeSymbol.ToDisplayString();
            }

            switch (attributeTypeFullName)
            {
                case SingleInstanceAttribute:
                    registrations.Add(TypeRegistration.Singleton(fullClassName, asType));
                    break;

                case TransientAttributeName:
                    registrations.Add(TypeRegistration.Transient(fullClassName, asType));
                    break;

                case DomainEventHandlerAttribute when asType is not null:
                    registrations.Add(TypeRegistration.Singleton(fullClassName: fullClassName,
                                                                 asType: IDomainEventHandler,
                                                                 key: $"typeof({asType})"));
                    break;
            }
        }

        return registrations;
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

        var assemblyName = compilation.Assembly.Name;

        var methodName = $"Add_{assemblyName.Replace(".", "_")}";

        var source = $$"""
                       /// <auto-generated/>
                       using Microsoft.Extensions.DependencyInjection;

                       namespace {{assemblyName}};

                       public static class ServiceExtensions
                       {
                           public static IServiceCollection {{methodName}}(this IServiceCollection services)
                           {
                       {{registerMethodsBody}}
                               return services;
                           }
                       }
                       """;

        context.AddSource("ServiceExtensions.g.cs", SourceText.From(source, Encoding.UTF8));
    }

    private static string BuildTypeRegistration(TypeRegistration typeRegistration)
    {
        var fullClassName = typeRegistration.FullClassName;

        if (typeRegistration.AddServicesFrom is not null)
        {
            return $"{fullClassName}.{typeRegistration.AddServicesFrom}(services);";
        }

        var sb = new StringBuilder();

        if (typeRegistration.IsSingleton)
        {
            sb.Append(typeRegistration.Key is not null
                          ? "services.AddKeyedSingleton<"
                          : "services.AddSingleton<");
        }
        else
        {
            sb.Append(typeRegistration.Key is not null
                          ? "services.AddKeyedTransient<"
                          : "services.AddTransient<");
        }

        if (typeRegistration.RegisterAs is not null)
        {
            var registerAs = typeRegistration.RegisterAs;
            sb.Append(registerAs).Append(", ");
        }

        sb.Append(fullClassName);
        sb.Append(">(");

        if (typeRegistration.Key is not null)
            sb.Append(typeRegistration.Key);

        sb.Append(");");

        return sb.ToString();
    }

    private struct TypeRegistration : IEquatable<TypeRegistration>
    {
        public string FullClassName { get; private set; }

        public bool IsSingleton { get; private set; }

        public string? RegisterAs { get; private set; }

        public string? Key { get; private set; }

        public string? AddServicesFrom { get; private set; }

        public bool Equals(TypeRegistration other)
        {
            return FullClassName.Equals(other.FullClassName)
                   && IsSingleton == other.IsSingleton
                   && RegisterAs == other.RegisterAs
                   && Key == other.Key
                   && AddServicesFrom == other.AddServicesFrom;
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
                hashCode = (hashCode * 397) ^ IsSingleton.GetHashCode();
                hashCode = (hashCode * 397) ^ (RegisterAs != null ? RegisterAs.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AddServicesFrom != null ? AddServicesFrom.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static TypeRegistration AddServicesFromMethod(string fullClassName,
                                                             string methodName)
        {
            return new TypeRegistration
            {
                FullClassName = fullClassName,
                AddServicesFrom = methodName
            };
        }

        public static TypeRegistration Singleton(string  fullClassName,
                                                 string? asType = null,
                                                 string? key    = null)
        {
            return new TypeRegistration
            {
                FullClassName = fullClassName,
                RegisterAs = asType,
                IsSingleton = true,
                Key = key
            };
        }

        public static TypeRegistration Transient(string  fullClassName,
                                                 string? asType = null,
                                                 string? key    = null)
        {
            return new TypeRegistration
            {
                FullClassName = fullClassName,
                RegisterAs = asType,
                IsSingleton = false,
                Key = key
            };
        }
    }
}