using OpenToWork.AdminWEB.Components;
using OpenToWork.AdminWEB.Services;
using OpenToWork.SharedUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AdminAuthApiService>();
builder.Services.AddScoped<AdminAuthStateProvider>();
builder.Services.AddScoped(sp => new LanguageService(
    sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
    sp.GetRequiredService<IWebHostEnvironment>(),
    new[] { "admin" }));
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(sp => sp.GetRequiredService<AdminAuthStateProvider>());

builder.Services.AddHttpClient<AdminAuthApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/");
});

var app = builder.Build();

// Detras de Cloudflare Tunnel (cloudflared en el mismo servidor) la peticion llega a IIS por HTTP aunque el
// usuario use HTTPS: se respeta X-Forwarded-Proto para que UseHttpsRedirection no entre en bucle. Solo se
// confia en proxies locales (loopback, el valor por defecto de KnownNetworks/KnownProxies).
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor
        | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
