using System.Reflection;
using GlavLib.Basics.DI;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace GlavLib.SourceGenerators.Tests;

public static partial class AutofacSourceGeneratorTests
{
    private static string Run(string source)
    {
        var generator = new AutofacSourceGenerator();
        var driver    = CSharpGeneratorDriver.Create(generator);

        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;

        var references = new[]
        {
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Private.CoreLib.dll")),
            MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Assembly.Load("GlavLib.Basics").Location),
            MetadataReference.CreateFromFile(Assembly.Load("GlavLib.App").Location)
        };

        var compilation = CSharpCompilation.Create(assemblyName: nameof(AutofacSourceGeneratorTests),
                                                   syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source) },
                                                   references: references);

        //act
        var runResult = driver.RunGenerators(compilation).GetRunResult();

        var generatedFileSyntax = runResult.GeneratedTrees.Single(t => t.FilePath.EndsWith("CompositionRoot.g.cs"));

        return generatedFileSyntax.GetText().ToString();
    }
}