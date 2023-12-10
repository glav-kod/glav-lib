using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
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
    public async Task It_should_create_user()
    {
        //act
        var createResponse = await _httpClient.PostAsJsonAsync("/api/users/create", new CreateUserArgs
        {
            Name = "Peter"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var createResult = await createResponse.Content.ReadFromJsonAsync<CreateUserResult>();
        
        //assert
        var getUserRequest = GlavJsonSerializer.Serialize(new GetUserRequest
        {
            UserId = createResult!.UserId
        });
        
        var response = await _httpClient.GetAsync($"/api/users/get?request={getUserRequest}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var userDTO = await response.Content.ReadFromJsonAsync<UserDTO>();

        userDTO!.Name.Should().Be("Peter");
    }
}