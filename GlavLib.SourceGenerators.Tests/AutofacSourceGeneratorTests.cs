using Microsoft.CodeAnalysis;

namespace GlavLib.SourceGenerators.Tests;

public partial class AutofacSourceGeneratorTests : SourceGeneratorTestsBase
{
    private static string Run(string source)
    {
        return RunSourceGenerator(sourceGenerator: new AutofacSourceGenerator().AsSourceGenerator(),
                                  sourceCode: source,
                                  resultFile: "CompositionRoot.g.cs");
    }
}