using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ResoConsultant.Services; // Убедись, что этот namespace существует

namespace ResoConsultant.ApiService.Controllers
{
    [ApiController]
    [Route("api/ai")] // 👈 ЖЕСТКИЙ АДРЕС (чтобы точно работало)
    public class AiController : ControllerBase
    {
        private readonly RagService _ragService;
        // Убедись, что Ollama запущена на этом порту
        private const string OllamaUrl = "http://127.0.0.1:11434/api/generate";

        public AiController(RagService ragService)
        {
            _ragService = ragService;
        }

        // Итоговый адрес будет: POST /api/ai/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            // Проверка на пустое сообщение
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Сообщение не может быть пустым" });

            try
            {
                // 1. Получаем контекст из базы знаний
                var prompt = _ragService.GetSystemPromptWithContext(request.Message);

                // 2. Готовим запрос к Ollama
                var payload = new
                {
                    model = "qwen3:4b", // Убедись, что модель скачана (ollama pull qwen3:4b)
                    prompt = request.Message,
                    system = prompt,
                    stream = false
                };

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(120) }; // Увеличил таймаут до 2 мин

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                // 3. Отправляем в Ollama
                var response = await client.PostAsync(OllamaUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { error = "Ошибка от Ollama API" });

                var responseBody = await response.Content.ReadAsStringAsync();
                var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

                // 4. Достаем ответ
                if (json.TryGetProperty("response", out var responseText))
                {
                    return Ok(new { message = responseText.GetString() });
                }

                return StatusCode(500, new { error = "Ollama вернула пустой ответ" });
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, new { error = "Ollama недоступна. Убедитесь, что она запущена." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Внутренняя ошибка: {ex.Message}" });
            }
        }
    }

    public class MessageRequest
    {
        public string Message { get; set; }
    }
}
