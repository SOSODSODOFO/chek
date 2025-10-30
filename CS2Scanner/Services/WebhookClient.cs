
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CS2Scanner.Services;

internal sealed class WebhookClient : IDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private readonly HttpClient _httpClient;
    private readonly string _webhookUrl;
    private bool _disposed;

    public WebhookClient(string webhookUrl)
    {
        _webhookUrl = webhookUrl;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    public async Task SendVerificationCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            return;
        }

        var payload = new
        {
            username = "CS2 Scanner",
            embeds = new[]
            {
                new
                {
                    title = "🔐 Новый код подтверждения",
                    description = $"Код доступа: **{code}**",
                    color = 0x4C8BF5,
                    timestamp = DateTimeOffset.UtcNow
                }
            }
        };

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(_webhookUrl, content, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _httpClient.Dispose();
    }
}
