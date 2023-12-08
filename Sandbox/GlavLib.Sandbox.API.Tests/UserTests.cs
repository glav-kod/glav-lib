using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using GlavLib.App.Validation;
using GlavLib.Basics.Serialization;
using GlavLib.Sandbox.API.Commands;
using Xunit.Abstractions;

namespace GlavGlavLib.Sandbox.API.Tests;

public class UserTests : IntegrationTestsBase
{
    private readonly HttpClient _httpClient;

    public UserTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _httpClient = WebAppFactory.CreateClient();
    }

    [Fact]
    public async Task It_should_get_user()
    {
        var request = GlavJsonSerializer.Serialize(new GetUserRequest
        {
            Field1 = "value1",
            Field2 = null!
        });

        _httpClient.DefaultRequestHeaders.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en"));
        
        var response = await _httpClient.GetAsync($"/api/users/get?request={request}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        result.Should().BeEquivalentTo(new ErrorResponse
        {
            Message = null,
            ParameterErrors = new Dictionary<string, string>
            {
                ["field2"] = "Fill value"
            }
        });
    }

    [Fact]
    public async Task It_should_create_user()
    {
        TestServiceFake.SetFoo("TestFoo");

        var response = await _httpClient.PostAsJsonAsync("/api/users/create", new CreateUserArgs
        {
            Value = "hello"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<CreateUserResult>();

        result.Should().BeEquivalentTo(new CreateUserResult
        {
            Value = "hello TestFoo"
        });
    }
}