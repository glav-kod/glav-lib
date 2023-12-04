namespace GlavLib.SourceGenerators.Tests;

public partial class AutofacSourceGeneratorTests
{
    public class Transient
    {
        [Fact]
        public void It_should_register_as_self()
        {
            //language=CSharp
            const string source = """
                                  using GlavLib.Basics.DI;
                                  
                                  namespace TestNamespace;

                                  [Transient]
                                  public sealed class TestClass
                                  {
                                  }
                                  """;
        
            var result = Run(source);

            //language=CSharp
            const string expected = """
                                    /// <auto-generated/>

                                    using Autofac;

                                    namespace AutofacSourceGeneratorTests;

                                    internal sealed class CompositionRoot : Module
                                    {
                                        protected override void Load(ContainerBuilder builder)
                                        {
                                            builder.RegisterType<TestNamespace.TestClass>();
                                        }
                                    }
                                    """;
        
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
        
        [Fact]
        public void It_should_register_as_interface()
        {
            //language=CSharp
            const string source = """
                                  using GlavLib.Basics.DI;
                                  
                                  namespace TestNamespace;

                                  public interface ITest { }
                                  
                                  [Transient<ITest>]
                                  public sealed class TestClass
                                  {
                                  }
                                  """;
        
            var result = Run(source);

            //language=CSharp
            const string expected = """
                                    /// <auto-generated/>

                                    using Autofac;

                                    namespace AutofacSourceGeneratorTests;

                                    internal sealed class CompositionRoot : Module
                                    {
                                        protected override void Load(ContainerBuilder builder)
                                        {
                                            builder.RegisterType<TestNamespace.TestClass>().As<TestNamespace.ITest>();
                                        }
                                    }
                                    """;
        
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
    }
}