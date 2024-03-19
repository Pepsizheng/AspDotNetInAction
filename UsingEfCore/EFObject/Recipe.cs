public class Recipe
{
    public int RecipeId { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; }

    public bool IsVet { get; set; }

    public bool IsDelete { get; set; }
}