using OpenToWork.AdminWEB.Components;
using OpenToWork.AdminWEB.Services;
using OpenToWork.SharedUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication();

builder.Services.AddSingleton(new SecureValueCipher(builder.Configuration["Security:LocalStorageKey"]));
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AdminAuthApiService>();
builder.Services.AddScoped<AdminAuthStateProvider>();
builder.Services.AddScoped(sp => new LanguageService(
    sp.GetRequiredService<Microsoft.JSInterop.IJSRuntime>(),
    builder.Environment.WebRootPath,
    new[] { "admin", "challenges" }));
builder.Services.AddScoped<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider>(sp => sp.GetRequiredService<AdminAuthStateProvider>());

builder.Services.AddHttpClient<AdminAuthApiService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/");
});
// Reenvio de archivos grandes (video de presentacion) del AdminAPI, que solo escucha en local.
builder.Services.AddHttpClient("AdminApiMedia", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5001/");
    client.Timeout = TimeSpan.FromMinutes(5);
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

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; script-src 'self' https://cdn.jsdelivr.net https://unpkg.com; style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://unpkg.com; img-src 'self' data: https:; media-src 'self' blob:; font-src 'self' data:; connect-src 'self' ws: wss:; frame-ancestors 'none'; base-uri 'self'; form-action 'self'";
    await next();
});

app.UseStaticFiles();
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAntiforgery();

// Video de presentacion del candidato. El navegador no llega al AdminAPI (solo escucha en 127.0.0.1),
// asi que AdminWEB reenvia la peticion tal cual: el token del admin va en Authorization y el AdminAPI
// decide (solo rol Admin). Se reenvia Range para poder avanzar en el reproductor.
app.MapGet("/media/candidates/{userId:guid}/video", async (Guid userId, HttpContext ctx, IHttpClientFactory factory) =>
{
    var auth = ctx.Request.Headers.Authorization.ToString();
    if (string.IsNullOrEmpty(auth)) return Results.Unauthorized();

    using var request = new HttpRequestMessage(HttpMethod.Get, $"api/admin/candidates/{userId}/video");
    request.Headers.TryAddWithoutValidation("Authorization", auth);
    if (ctx.Request.Headers.Range.Count > 0)
        request.Headers.TryAddWithoutValidation("Range", ctx.Request.Headers.Range.ToString());

    var response = await factory.CreateClient("AdminApiMedia").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted);
    ctx.Response.RegisterForDispose(response);
    if (!response.IsSuccessStatusCode) return Results.StatusCode((int)response.StatusCode);

    ctx.Response.StatusCode = (int)response.StatusCode;
    ctx.Response.Headers.CacheControl = "no-store";
    if (response.Content.Headers.ContentRange != null)
        ctx.Response.Headers.ContentRange = response.Content.Headers.ContentRange.ToString();
    ctx.Response.Headers.AcceptRanges = "bytes";
    ctx.Response.ContentLength = response.Content.Headers.ContentLength;
    ctx.Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "video/mp4";
    // Copia directa (no Results.Stream) para conservar el 206 de las respuestas con rango.
    await response.Content.CopyToAsync(ctx.Response.Body, ctx.RequestAborted);
    return Results.Empty;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
