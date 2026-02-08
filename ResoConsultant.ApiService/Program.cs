using ResoConsultant.ApiService.Controllers;
using ResoConsultant.Services; // ПОДКЛЮЧАЕМ НАШ NAMESPACE

var builder = WebApplication.CreateBuilder(args);

// 1. Стандартные настройки .NET Aspire
builder.AddServiceDefaults();

// 2. Регистрация контроллеров и HTTP клиента
builder.Services.AddControllers();
builder.Services.AddHttpClient();

// 3. РЕГИСТРИРУЕМ RAGSERVICE
// Используем AddSingleton, чтобы файлы загружались один раз при старте приложения, 
// а не при каждом сообщении. Теперь конструктор вызывается без аргументов.
builder.Services.AddSingleton<RagService>();

// 4. Настройка документации API и обработки ошибок
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Настройка конвейера обработки запросов (Middleware)
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Включаем маршрутизацию для контроллеров (чтобы работал AiController)
app.MapControllers();

// Эндпоинты проверки здоровья (Health Checks) от Aspire
app.MapDefaultEndpoints();

app.Run();
