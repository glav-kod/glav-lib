using FluentAssertions;
using FluentAssertions.Execution;
using GlavLib.Basics.DataTypes;
using GlavLib.Basics.Serialization;
using JetBrains.Annotations;

namespace GlavLib.Tests.Basics.Serialization;

public sealed class SerializationOptionalPropertyTests
{
    public sealed class Foo
    {
        public Optional<int> A { get; [UsedImplicitly] init; }

        public Optional<int?> B { get; [UsedImplicitly] init; }

        public Optional<int> C { get; [UsedImplicitly] init; }
    }


    [Fact]
    public void It_should_deserialize_optional_properties()
    {
        const string json
            = """
            {
              "b": null,
              "c": 123
            }
            """;

        var foo = GlavJsonSerializer.Deserialize<Foo>(json);

        using (new AssertionScope())
        {
            foo.A.HasValue.Should().BeFalse();

            foo.B.HasValue.Should().BeTrue();
            foo.B.Value.Should().BeNull();

            foo.C.HasValue.Should().BeTrue();
            foo.C.Value.Should().Be(123);
        }
    }
    
    [Fact]
    public void It_should_serialize_optional_properties()
    {
        var foo = new Foo
        {
            A = Optional<int>.Undefined,
            B = Optional<int?>.Null,
            C = 123
        };

        var json   = GlavJsonSerializer.Serialize(foo);
        var result = GlavJsonSerializer.Deserialize<IDictionary<string, object>>(json);

        result.Should().BeEquivalentTo(new Dictionary<string, object?>
        {
            ["b"] = null,
            ["c"] = 123
        });
    }
}