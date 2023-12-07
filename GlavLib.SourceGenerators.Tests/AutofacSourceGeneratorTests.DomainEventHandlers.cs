namespace GlavLib.SourceGenerators.Tests;

public partial class AutofacSourceGeneratorTests
{
    public class DomainEventHandlers
    {
        [Fact]
        public void It_should_generate_domain_event_handler_registration()
        {
            //language=CSharp
            const string source = """
                                  using GlavLib.App.DomainEvents;

                                  namespace TestNamespace;

                                  public sealed class TestEvent : DomainEvent  { }

                                  public sealed class TestEventHandler : DomainEventHandler<TestEvent>
                                  {
                                      protected override Task HandleAsync(TestEvent domainEvent, CancellationToken cancellationToken)
                                      {
                                          return Task.CompletedTask;
                                      }
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
                                            builder.RegisterType<TestNamespace.TestEventHandler>().As<GlavLib.App.DomainEvents.DomainEventHandler<TestNamespace.TestEvent>>().SingleInstance();
                                        }
                                    }
                                    """;

            Assert.Equal(expected, result, ignoreLineEndingDifferences: true);
        }
    }
}