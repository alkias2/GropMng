// Project namespace that exposes custom startup extension methods.
using GropMng.Web.Startup;

// Built-in ASP.NET Core host builder initialization (.NET).
var builder = WebApplication.CreateBuilder(args);

// Project extension method: registers application services and DI modules.
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

// Built-in app construction from the configured host builder (.NET).
var app = builder.Build();

// Project extension method: runs migrations/seeding/startup initialization.
await app.RunStartupInitializationAsync();

// Project extension method: configures the HTTP middleware pipeline.
app.UseApplicationPipeline();

// Built-in ASP.NET Core call that starts the web application (.NET).
app.Run();
