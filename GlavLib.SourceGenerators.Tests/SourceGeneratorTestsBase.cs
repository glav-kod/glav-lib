using System.Reflection;
using GlavLib.SourceGenerators.Tests.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GlavLib.SourceGenerators.Tests;

public abstract class SourceGeneratorTestsBase
{
    protected static string RunSourceGenerator(
            ISourceGenerator sourceGenerator,
            string? sourceCode,
            string resultFile,
            TestAdditionalText[]? additionalTexts = null
        )
    {
        var generators = new[]
        {
            sourceGenerator
        };

        var driver = CSharpGeneratorDriver.Create(generators, additionalTexts);

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var references = new[]
        {
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Assembly.Load("GlavLib.Abstractions").Location),
            MetadataReference.CreateFromFile(Assembly.Load("GlavLib.Basics").Location),
            MetadataReference.CreateFromFile(Assembly.Load("GlavLib.SourceGenerators").Location),
        };

        var syntaxTrees = new List<SyntaxTree>();
        if (sourceCode is not null)
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(sourceCode));

        var compilation = CSharpCompilation.Create(assemblyName: "GlavLib.SourceGenerators.Tests",
                                                   syntaxTrees: syntaxTrees,
                                                   references: references);

        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generatedFileSyntax = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith(resultFile));

        return generatedFileSyntax.GetText().ToString();
    }
}