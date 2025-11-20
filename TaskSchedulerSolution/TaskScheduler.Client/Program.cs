using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using TaskScheduler.Client;
using TaskScheduler.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Register Custom Authentication State Provider
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthenticationStateProvider>());

// Add Authorization services
builder.Services.AddAuthorizationCore();

// Register HttpClient with base address
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5081";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// Register HTTP Message Handler for Authorization
builder.Services.AddTransient<AuthorizingHttpMessageHandler>();

// Register HttpClient with Authorization Handler
builder.Services.AddHttpClient("AuthorizedClient", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
})
.AddHttpMessageHandler<AuthorizingHttpMessageHandler>();

// Register API Services
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<TaskApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<CategoryApiService>();

await builder.Build().RunAsync();
