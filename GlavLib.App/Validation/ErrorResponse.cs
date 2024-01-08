namespace GlavLib.App.Validation;

public sealed class ErrorResponse
{
    public string? Code { get; set; }
    
    public string? Message { get; set; }

    public Dictionary<string, string>? ParameterCodes { get; set; }
    
    public Dictionary<string, string>? ParameterMessages { get; set; }
}
