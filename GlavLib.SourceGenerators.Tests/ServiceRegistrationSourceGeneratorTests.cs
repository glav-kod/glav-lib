using Microsoft.CodeAnalysis;

namespace GlavLib.SourceGenerators.Tests;

public partial class ServiceRegistrationSourceGeneratorTests : SourceGeneratorTestsBase
{
    private static string Run(string source)
    {
        return RunSourceGenerator(sourceGenerator: new ServiceRegistrationSourceGenerator().AsSourceGenerator(),
                                  sourceCode: source,
                                  resultFile: "ServiceExtensions.g.cs");
    }
}