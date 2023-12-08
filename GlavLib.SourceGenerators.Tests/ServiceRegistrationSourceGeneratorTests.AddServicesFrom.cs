namespace GlavLib.SourceGenerators.Tests;

public partial class ServiceRegistrationSourceGeneratorTests
{
    public class AddServicesFrom
    {
        [Fact]
        public void It_should_add_registration_method()
        {
            //language=CSharp
            const string source = """
                                  using GlavLib.Abstractions.DI;
                                  using Microsoft.Extensions.DependencyInjection;

                                  namespace TestNamespace;

                                  public interface IFoo;
                                  
                                  public class IImpl1 : IFoo;
                                  
                                  public class IImpl2 : IFoo;
                                  
                                  [AddServicesFrom(nameof(RegisterServices))]
                                  public sealed class Factory : IHaveServiceRegistrations
                                  {
                                      public static void RegisterServices(IServiceCollection services)
                                      {
                                          services.AddKeyedSingleton<IFoo, IImpl1>("k1");
                                          services.AddKeyedSingleton<IFoo, IImpl2>("k2");
                                      }
                                  }
                                  """;

            var result = Run(source);

            //language=CSharp
            const string expected = """
                                    /// <auto-generated/>
                                    using Microsoft.Extensions.DependencyInjection;

                                    namespace GlavLib.SourceGenerators.Tests;

                                    public static class ServiceExtensions
                                    {
                                        public static void Add_GlavLib_SourceGenerators_Tests(this IServiceCollection services)
                                        {
                                            TestNamespace.Factory.RegisterServices(services);
                                        }
                                    }
                                    """;

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
    }
}