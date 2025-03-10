﻿using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using GlavLib.SourceGenerators.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using YamlDotNet.Serialization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GlavLib.SourceGenerators;

[Generator]
public class ErrorsSourceGenerator : IIncrementalGenerator
{
    private static readonly Deserializer YamlDeserializer = new();

    private const string ValidationNamespace = "GlavLib.Abstractions.Validation";

    private static readonly DiagnosticDescriptor UnhandledException =
        new("XX0002",
            "Unhandled exception occurred",
            "The ErrorsSourceGenerator caused an exception {0}: {1} {2}",
            "Error",
            DiagnosticSeverity.Error,
            true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var translations = context.AdditionalTextsProvider
                                  .Where(text => text.Path.EndsWith(".errors.yaml",
                                                                    StringComparison.OrdinalIgnoreCase))
                                  .Select((text, ct) => text.GetText(ct)?.ToString())
                                  .Where(text => text is not null)!
                                  .Collect<string>();

        context.RegisterSourceOutput(translations, GenerateCode);
    }

    private static void GenerateCode(
            SourceProductionContext context,
            ImmutableArray<string> errorYamls
        )
    {
        try
        {
            foreach (var errorYamlText in errorYamls)
            {
                var errorsBundle = YamlDeserializer.Deserialize<ErrorsBundle>(errorYamlText);

                var bundleClassSource = GenerateErrorsClassSource(errorsBundle);

                context.AddSource($"{errorsBundle.ClassName}.g.cs", SourceText.From(bundleClassSource, Encoding.UTF8));
            }
        }
        catch (Exception ex)
        {
            var diagnostic = Diagnostic.Create(UnhandledException,
                                               null,
                                               ex.GetType(),
                                               ex.Message,
                                               ex.StackTrace);

            context.ReportDiagnostic(diagnostic);
        }
    }

    private static string GenerateErrorsClassSource(ErrorsBundle errorsBundle)
    {
        var fields  = new List<MemberDeclarationSyntax>();
        var methods = new List<MemberDeclarationSyntax>();

        var errorKeyPrefix = errorsBundle.Namespace is not null
            ? $"{errorsBundle.Namespace}.{errorsBundle.ClassName}"
            : errorsBundle.ClassName;

        foreach (var kvp in errorsBundle.Errors)
        {
            var    errorName = kvp.Key;
            var    errorCode = "null";
            string message;

            if (kvp.Value is Dictionary<object, object> errorWithCode)
            {
                message   = errorWithCode["Message"].ToString();
                errorCode = $"\"{errorWithCode["Code"]}\"";
            }
            else
            {
                message = kvp.Value.ToString();
            }

            var messageTemplate = ErrorMessageTemplate.Parse(message);

            if (messageTemplate.Arguments.Count == 0)
            {
                var field =
                    $"public static readonly Error {errorName} = new (\"{messageTemplate.InterpolatedMessage}\", \"{errorKeyPrefix}.{errorName}\", {errorCode});";
                fields.Add(ParseMemberDeclaration(field)!);
            }
            else
            {
                var messageFunction = BuildMessageFunction(errorKeyPrefix: errorKeyPrefix,
                                                           errorName: errorName,
                                                           errorCode: errorCode,
                                                           messageTemplate: messageTemplate);

                methods.Add(messageFunction);
            }
        }

        var usings = new[]
        {
            UsingDirective(IdentifierName("System")),
            UsingDirective(IdentifierName("System.Globalization")),
            UsingDirective(IdentifierName(ValidationNamespace))
        };

        var compilationUnit = CompilationUnit()
            .AddUsings(usings);

        var bundleClass = ClassDeclaration(errorsBundle.ClassName)
                          .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                          .AddMembers(fields.ToArray())
                          .AddMembers(methods.ToArray());

        var ns = errorsBundle.Namespace;
        if (string.IsNullOrWhiteSpace(ns))
        {
            compilationUnit = compilationUnit.AddMembers(bundleClass);
        }
        else
        {
            var namespaceDeclaration = NamespaceDeclaration(IdentifierName(ns!));
            namespaceDeclaration = namespaceDeclaration.AddMembers(bundleClass);

            compilationUnit = compilationUnit.AddMembers(namespaceDeclaration);
        }

        return compilationUnit.NormalizeWhitespace()
                              .WithLeadingTrivia(ParseLeadingTrivia("/// <auto-generated/>\n"))
                              .ToFullString();
    }

    private static MemberDeclarationSyntax BuildMessageFunction(
            string errorKeyPrefix,
            string errorName,
            string errorCode,
            ErrorMessageTemplate messageTemplate
        )
    {
        var methodArgsSb     = new StringBuilder();
        var dictionaryArgsSb = new StringBuilder();

        for (var i = 0; i < messageTemplate.Arguments.Count; i++)
        {
            var argument = messageTemplate.Arguments[i];

            methodArgsSb.Append(argument.Type);
            methodArgsSb.Append(' ');
            methodArgsSb.Append(argument.Name);
            if (i < messageTemplate.Arguments.Count - 1)
                methodArgsSb.Append(", ");

            var formatString = argument.FormatString is null
                ? string.Empty
                : $"\"{argument.FormatString}\"";

            if (argument.IsOptional)
            {
                dictionaryArgsSb.AppendLine($"    if ({argument.Name} is not null)");
                dictionaryArgsSb.Append("        args[\"");
                dictionaryArgsSb.Append(argument.Name);
                dictionaryArgsSb.Append("\"] = ");
                dictionaryArgsSb.Append(argument.Name);
                dictionaryArgsSb.Append($".Value.ToString({formatString})");
            }
            else
            {
                dictionaryArgsSb.Append("    args[\"");
                dictionaryArgsSb.Append(argument.Name);
                dictionaryArgsSb.Append("\"] = ");
                dictionaryArgsSb.Append(argument.Name);
                dictionaryArgsSb.Append($".ToString({formatString})");
            }

            dictionaryArgsSb.AppendLine(";");
        }

        var method = $$"""
            public static Error {{errorName}}({{methodArgsSb}})
            {
                var args = new Dictionary<string, string>();
            {{dictionaryArgsSb}}
                FormattableString message = $"{{messageTemplate.InterpolatedMessage}}";
                return new Error(message.ToString(CultureInfo.InvariantCulture), "{{errorKeyPrefix}}.{{errorName}}", {{errorCode}}, args);
            }
            """;

        return ParseMemberDeclaration(method)!;
    }

    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    private sealed class ErrorsBundle
    {
        public string? Namespace { get; set; }

        public string ClassName { get; set; } = null!;

        public IDictionary<string, object> Errors { get; set; } = null!;
    }
}