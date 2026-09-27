using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpenToWork.WEB.Components;
using OpenToWork.WEB.Services;
using OpenToWork.SharedUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Cliente WASM: sin circuito SignalR - la app corre en el navegador y solo habla HTTP con la API.
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(sp => sp.GetRequiredService<AppAuthStateProvider>());

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped(sp => new SecureValueCipher(builder.Configuration["Security:LocalStorageKey"]));
builder.Services.AddScoped<AppAuthStateProvider>();
builder.Services.AddScoped(sp => new LanguageService(
    sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
    sp.GetRequiredService<HttpClient>(),
    new[] { "common", "auth", "wizard", "dashboard", "vacancies", "profile", "validation", "errors", "applications" }));

// HttpClient por defecto: origen de la app (para assets como config/language/*.json).
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// ApiAuthService: cliente dedicado apuntando a la API del portal.
builder.Services.AddScoped(sp => new ApiAuthService(
    new HttpClient { BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5100/") },
    sp.GetRequiredService<LocalStorageService>(),
    sp.GetRequiredService<ILogger<ApiAuthService>>()));

await builder.Build().RunAsync();
