using Fume.Web;
using CurrieTechnologies.Razor.SweetAlert2;
using Fume.Web.Repositories;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Fume.Web.Auth;
using Blazored.Modal;
using MudBlazor.Services;
using Fume.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7181/") });
builder.Services.AddScoped<IRepository, Repository>();
builder.Services.AddSweetAlert2();
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazorBootstrap();
builder.Services.AddBlazoredModal();
builder.Services.AddMudServices();
builder.Services.AddScoped<AutenticacionPro>();
builder.Services.AddScoped<AuthenticationStateProvider, AutenticacionPro>(x => x.GetRequiredService<AutenticacionPro>());
builder.Services.AddScoped<IloginService, AutenticacionPro>(x => x.GetRequiredService<AutenticacionPro>());
builder.Services.AddSingleton<CatalogNavigationService>();



await builder.Build().RunAsync();
