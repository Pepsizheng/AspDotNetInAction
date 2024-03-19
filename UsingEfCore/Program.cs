using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppContext>(option => 
{
    option.UseNpgsql(builder.Configuration["path"]);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();
