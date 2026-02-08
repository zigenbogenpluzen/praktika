namespace ResoConsultant.ApiService.Models;

public class ChatRequest
{
    public string Question { get; set; } = string.Empty;
}

public class AiResponse
{
    public string? Answer { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    // Конструктор для удобства
    public AiResponse(string? answer, bool isSuccess, string? errorMessage = null)
    {
        Answer = answer;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}
