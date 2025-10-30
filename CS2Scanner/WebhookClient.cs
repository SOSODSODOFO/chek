using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CS2Scanner;

internal sealed class WebhookClient
{
    private readonly HttpClient _httpClient;
    private readonly string? _webhookUrl;

    public WebhookClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(15);
        _webhookUrl = Environment.GetEnvironmentVariable("DISCORD_WEBHOOK_URL");

        if (string.IsNullOrWhiteSpace(_webhookUrl))
        {
            _webhookUrl = "https://discord.com/api/webhooks/REPLACE_WITH_REAL_WEBHOOK";
        }
    }

    public async Task SendCodeAsync(string code)
    {
        if (!IsWebhookConfigured())
        {
            return;
        }

        var payload = new
        {
            content = $"Новый проверочный код: **{code}**"
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_webhookUrl, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task SendReportAsync(string html)
    {
        if (!IsWebhookConfigured())
        {
            return;
        }

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("Сгенерирован новый отчёт системы."), "content");

        var bytes = Encoding.UTF8.GetBytes(html);
        var fileContent = new ByteArrayContent(bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html");
        form.Add(fileContent, "file", $"cs2-report-{DateTime.Now:yyyyMMddHHmmss}.html");

        var response = await _httpClient.PostAsync(_webhookUrl, form);
        response.EnsureSuccessStatusCode();
    }

    private bool IsWebhookConfigured()
    {
        return !string.IsNullOrWhiteSpace(_webhookUrl) && !_webhookUrl.Contains("REPLACE_WITH_REAL_WEBHOOK", StringComparison.OrdinalIgnoreCase);
    }
}
