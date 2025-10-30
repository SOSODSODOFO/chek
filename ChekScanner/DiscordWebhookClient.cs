using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChekScanner;

public sealed class DiscordWebhookClient : IDisposable
{
    private readonly Uri _webhookUri;
    private readonly HttpClient _httpClient;

    public DiscordWebhookClient(Uri webhookUri)
    {
        _webhookUri = webhookUri;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public async Task TrySendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        try
        {
            var payload = JsonSerializer.Serialize(new { content = message });
            using var content = new StringContent(payload, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PostAsync(_webhookUri, content).ConfigureAwait(false);
            _ = response.IsSuccessStatusCode;
        }
        catch
        {
            // ignored: webhook failures should not stop the UI flow
        }
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
