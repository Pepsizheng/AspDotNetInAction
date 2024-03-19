public class Recipe
{
    public int RecipeId { get; set; }
    public ICollection<Ingredient> Ingredients { get; set; }
}