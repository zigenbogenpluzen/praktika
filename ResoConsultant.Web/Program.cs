using MudBlazor.Services;
using ResoConsultant.Web;
using ResoConsultant.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Стандартные настройки Aspire
builder.AddServiceDefaults();
builder.AddRedisOutputCache("cache");

// Сервисы MudBlazor
builder.Services.AddMudServices();

// Поддержка интерактивности (Server Mode - самый надежный)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 🔥 РЕГИСТРАЦИЯ КЛИЕНТА С ТАЙМАУТОМ 10 МИНУТ
builder.Services.AddHttpClient<AiApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
    client.Timeout = TimeSpan.FromMinutes(10); // Железобетонный таймаут
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
