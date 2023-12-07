namespace GlavLib.SourceGenerators.Tests;

public partial class AutofacSourceGeneratorTests
{
    public class SingleInstance
    {
        [Fact]
        public void It_should_register_as_self()
        {
            //language=CSharp
            const string source = """
                                  using GlavLib.Abstractions.DI;
                                  
                                  namespace TestNamespace;

                                  [SingleInstance]
                                  public sealed class TestClass
                                  {
                                  }
                                  """;
        
            var result = Run(source);

            //language=CSharp
            const string expected = """
                                    /// <auto-generated/>

                                    using Autofac;

                                    namespace GlavLib.SourceGenerators.Tests;

                                    internal sealed class CompositionRoot : Module
                                    {
                                        protected override void Load(ContainerBuilder builder)
                                        {
                                            builder.RegisterType<TestNamespace.TestClass>().SingleInstance();
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
                                  using GlavLib.Abstractions.DI;
                                  
                                  namespace TestNamespace;

                                  public interface ITest { }

                                  [SingleInstance<ITest>)]
                                  public sealed class TestClass
                                  {
                                  }
                                  """;
        
            var result = Run(source);

            //language=CSharp
            const string expected = """
                                    /// <auto-generated/>

                                    using Autofac;

                                    namespace GlavLib.SourceGenerators.Tests;

                                    internal sealed class CompositionRoot : Module
                                    {
                                        protected override void Load(ContainerBuilder builder)
                                        {
                                            builder.RegisterType<TestNamespace.TestClass>().As<TestNamespace.ITest>().SingleInstance();
                                        }
                                    }
                                    """;
        
            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
    }
}