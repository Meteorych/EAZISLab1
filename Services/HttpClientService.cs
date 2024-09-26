using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace EAZISLab1.Services;

public class HttpClientService
{
    private const string BaseUrl = "https://6efe-80-79-118-244.ngrok-free.app";
    private const string HandledDocumentsSection = "paths.txt";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public HttpClientService(HttpClient httpClient, IConfiguration configuration)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<ResponseBody>?> SendQuery(string query, int textLength)
    {
        var queryObject = new QueryBody { Query = query, Limit = textLength };
        var response = await _httpClient.PostAsJsonAsync(ApiPathConstants.MainQueryPath, queryObject);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadFromJsonAsync<List<ResponseBody>>();
            if (string.IsNullOrEmpty(query)) return responseContent;
            responseContent?.ForEach(rb =>
            {
                if (rb.Text.Contains(query))
                {
                    rb.QueryText = query;
                }
            });
            return responseContent;
        }

        throw new HttpRequestException($"Response returned {response.StatusCode} status code.");
    }

    public async Task SendPathToDocuments()
    {
        var documentsToHandle = GetPaths();
        await _httpClient.PostAsJsonAsync(ApiPathConstants.PathToDocumentsApi, documentsToHandle);
    } 

    private string[] GetPaths()
    {
        return _configuration.GetSection(HandledDocumentsSection).Value?.Split(",") ?? throw new InvalidOperationException("You should configure sending documents.");
    }
}

public class QueryBody
{
    public string Query { get; set; }
    public int Limit { get; set; }
}


public class ResponseBody
{
    public string Doc { get; set; }
    public string Text { get; set; }
    public string? QueryText { get; set; }
}