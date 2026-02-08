using System.Net.Http.Json;

public class AiApiClient(HttpClient httpClient)
{
    public async Task<AiResponse> AskAsync(string message)
    {
        try
        {
            // 👇 ГЛАВНОЕ: Адрес должен совпадать с контроллером!
            // Мы сделали в контроллере [Route("api/ai")] и [HttpPost("send")]
            var response = await httpClient.PostAsJsonAsync("api/ai/send", new { Message = message });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<AiApiResponse>();
                return new AiResponse { IsSuccess = true, Answer = result?.Message ?? "Пустой ответ" };
            }

            return new AiResponse { IsSuccess = false, ErrorMessage = $"Ошибка {response.StatusCode}: Адрес не найден" };
        }
        catch (Exception ex)
        {
            return new AiResponse { IsSuccess = false, ErrorMessage = "Сбой сети: " + ex.Message };
        }
    }
}

// Вспомогательные классы (чтобы всё работало)
public class AiResponse
{
    public bool IsSuccess { get; set; }
    public string Answer { get; set; }
    public string ErrorMessage { get; set; }
}

public class AiApiResponse
{
    public string Message { get; set; }
}
