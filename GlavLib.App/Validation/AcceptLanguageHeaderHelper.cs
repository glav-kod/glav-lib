using Microsoft.Net.Http.Headers;

namespace GlavLib.App.Validation;

public static class AcceptLanguageHeaderHelper
{
    public static string[] Parse(string header)
    {
        return header.Split(',')
                     .Select(x => StringWithQualityHeaderValue.Parse(x))
                     .OrderByDescending(s => s.Quality.GetValueOrDefault(1))
                     .Select(x => x.Value.Value!)
                     .ToArray();
    }
}