namespace GlavLib.App.Validation;

public sealed class ErrorResponse
{
    public string? Message { get; set; }

    public Dictionary<string, string>? ParameterErrors { get; set; }
}