namespace GlavLib.App.Validation;

public sealed class ErrorResponse
{
    public required string? Message { get; set; }

    public required Dictionary<string, string>? ParameterErrors { get; set; }
}