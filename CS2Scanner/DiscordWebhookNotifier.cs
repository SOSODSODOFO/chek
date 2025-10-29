using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CS2Scanner
{
    public static class DiscordWebhookNotifier
    {
        public static async Task SendReportAsync(string webhookUrl, ReportData report, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                return;
            }

            using var httpClient = new HttpClient();
            using var form = new MultipartFormDataContent();

            var payload = new
            {
                username = "CS2 Scanner",
                content = $"Сканирование завершено. Итог: {report.Judgement}",
            };

            string json = JsonSerializer.Serialize(payload);
            var payloadContent = new StringContent(json, Encoding.UTF8, "application/json");
            form.Add(payloadContent, "payload_json");

            var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(htmlContent));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/html; charset=utf-8");
            form.Add(fileContent, "files[0]", "cs2_report.html");

            try
            {
                using var response = await httpClient.PostAsync(webhookUrl, form);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Не удалось отправить отчёт в Discord webhook.", ex);
            }
        }
    }
}
