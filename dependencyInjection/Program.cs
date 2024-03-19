var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IService, ServiceA>();
builder.Services.AddScoped<Data>();
builder.Services.AddScoped<Respository>(); //创建服务的时候，生命周期短的可依赖长的，但是长的不能依赖短的

var app = builder.Build();

app.Services.GetService<IService>(); //对于singleton周期的实例，可以采取该方式

using (var scope = app.Services.CreateAsyncScope())
{
    //对于要使用GetService的方法的非单例周期的，需采取该方式，因为上面的方式会跟随应用程序直到重启为止，所以会造成内存泄漏。
    var service = scope.ServiceProvider.GetService<IService>();
    //该方式就会在运行完后自动销毁实例内存
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/count", ServiceScope);

string ServiceScope(Data data, Respository repo)
{
    return "data's row is " + data.RowCount + ", repo's row is " + repo.RowCount;
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
