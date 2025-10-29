using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChekScanner;

public sealed class DiscordWebhookClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;

    public DiscordWebhookClient(string webhookUrl)
    {
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _httpClient = new HttpClient();
    }

    public async Task SendMessageAsync(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new { content });
        using var message = new HttpRequestMessage(HttpMethod.Post, _webhookUrl)
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(message);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendReportAsync(string message, string fileName, byte[] fileContent)
    {
        if (fileContent == null || fileContent.Length == 0)
        {
            throw new ArgumentException("Пустой отчёт нельзя отправить", nameof(fileContent));
        }

        using var content = new MultipartFormDataContent();
        if (!string.IsNullOrWhiteSpace(message))
        {
            content.Add(new StringContent(message, Encoding.UTF8), "content");
        }

        var filePart = new ByteArrayContent(fileContent);
        filePart.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
        content.Add(filePart, "files[0]", string.IsNullOrWhiteSpace(fileName) ? "report.html" : fileName);

        using var response = await _httpClient.PostAsync(_webhookUrl, content);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
