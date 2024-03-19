using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppContext>(option => 
{
    option.UseNpgsql(builder.Configuration["PgConnect:connection"]);
});

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/add", AddRecipe);

app.MapGet("/all", GetAll);

app.MapGet("/update", UpdateId);

async Task<int> AddRecipe()
{
    var ingre = new Ingredient();
    ingre.Name = "name";
    ingre.RecipeId = 1;
    ingre.IngredientId = 2;
    var reci = new Recipe();
    reci.IsVet = true;
    reci.IsDelete = true;
    reci.RecipeId = 1;
    reci.Ingredients = [ingre];
    using (var scope = app.Services.CreateAsyncScope())
    {
        var context = scope.ServiceProvider.GetService<AppContext>();
        context.Add(reci);
        await context.SaveChangesAsync();
    }
    return ingre.RecipeId;
}

async Task<ICollection<Recipe>> GetAll()
{
    using (var scope = app.Services.CreateAsyncScope())
    {
        var context = scope.ServiceProvider.GetService<AppContext>();
        return await context.recipes.Where(i => i.IsDelete).ToListAsync();
    }
}

async Task<int> UpdateId()
{
    using (var scope = app.Services.CreateAsyncScope())
    {
        var context = scope.ServiceProvider.GetService<AppContext>();
        var record = context.recipes.Where(i => i.RecipeId == 1).SingleOrDefaultAsync();
        record.Result.IsVet = false;
        await context.SaveChangesAsync();
        return record.Result.RecipeId;
    }
}

app.Run();
