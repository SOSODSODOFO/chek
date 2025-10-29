using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CS2Scanner.Services;

internal sealed class DiscordWebhookService : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _webhookUrl;
    private bool _disposed;

    public DiscordWebhookService(string webhookUrl)
    {
        _webhookUrl = webhookUrl ?? throw new ArgumentNullException(nameof(webhookUrl));
        _client = new HttpClient();
        _client.DefaultRequestHeaders.UserAgent.ParseAdd("CS2Scanner/1.0");
    }

    public async Task SendMessageAsync(string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        var payload = new
        {
            content,
            username = "CS2 Scanner",
            embeds = Array.Empty<object>()
        };

        using var response = await _client.PostAsJsonAsync(_webhookUrl, payload, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendReportAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(htmlContent))
        {
            return;
        }

        var payloadJson = JsonSerializer.Serialize(new
        {
            content = "📄 Завершён новый скан. Отчёт прилагается.",
            username = "CS2 Scanner"
        });

        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(payloadJson, Encoding.UTF8, "application/json"), "payload_json");

        var bytes = Encoding.UTF8.GetBytes(htmlContent);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
        multipart.Add(fileContent, "file", "CS2ScanReport.html");

        using var response = await _client.PostAsync(_webhookUrl, multipart, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _client.Dispose();
        _disposed = true;
    }
}
