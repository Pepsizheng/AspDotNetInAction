using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/{id: int}", (int id) => $"Hello World! {id}");
/*
ModelBinding发生在终端执行前，首先是先route起作用，在制定到某个终端的时候，参数类型均为string
所以asp.net core要先执行ModelBinding，判断需要创建哪些类型来对应请求路径中的参数
可以使用六种数据源来创建终端所需要的参数
1. Route value:请求路径
2. Query string:请求路径中携带的参数值
3. Header value:请求头
4. body json:请求体——在mininal api中，只会自动绑定json格式的全球体
5. 依赖注入服务
6. 自定义绑定

绑定的时候，是依靠终端参数名称与数据源名称相同的情况下才进行匹配
*/

app.MapGet("/person/{id}", () => "person");//person/12
app.MapGet("/person", (int id) => "person");//person?id=12
//匹配的顺序是跟数据源的顺序是一样的，先匹配请求路径，在匹配请求查询参数值

app.MapGet("/person/{id}", ([FromRoute] int id) => "");
app.MapGet("/person/{id}", ([FromQuery] int id) => "");
//前面两种为约定，所以可以不用显示声明
app.MapGet("/person/{id}", ([FromHeader(Name = "pagesize")] int id) => "");
//可以自己制定对应的数据源，采取参数前加[FromXXX]的方式表明来自哪里

/*
这三种[FromXXX]方式，只能转换有实现TryParse方法的类型，如int， string， double等
所以当需要匹配特殊的转换格式的时候，可以采取如下方式：
*/
app.MapGet("/person/{id}", (ProductID id) => "");//此处映射sting到ProductID类型，是因为该类型有实现TryParse方法

app.MapGet("/person", (int[] ids) => "");//person?ids=12&ids=13时生效

app.MapGet("/person", (int[] id) => "");//person?id=12&id=13时生效，但这里还叫id而不是ids就会让人觉得错乱

app.MapGet("/person", ([FromQuery(Name = "id")]int[] ids) => "");//这里就能更改每次为ids更加符合操作了

app.MapGet("/person/{id?}", (int? id) => "");//可选的参数

app.MapGet("/person/{id}", (int id = 1) => "");//默认参数

app.MapPost("/person/product", (Product p) => $"p' name is {p.name}" );//这里采取的是post方法，该方法在请求的时候，会将请求body中的参数反序列化为Product类。
//对于需要显示声明的时候，则采取参数前加[FromBody]，一般出现在如GET方法但也携带请求体的时候。

app.MapGet("context", (HttpContext context) => "");//可以访问Httpcontext，从而访问请求的内容
/*
还可以通过HttpContext访问下列对象：
1. HttpRequest = HttpContext.Request
2. HttpResponse = HttpContext.Response
3. CancellationToken = HttpContext.RequestAborted
4. ClaimsPrincipal = HttpContext.User
5. Stream = HttpRequest.Body
6. PipeReader = HttpContext.BodyReader
*/

app.MapGet("/services", (LinkGenerator links) => "");//可以使用DI注入的服务，也可以显示声明加上[FromServices]

app.MapGet("/upload", (IFormFile file) => "");//上传文件，但要注意，不要直接使用上传文件中的名称，防止文件名称恶意破坏
app.MapGet("/uploads", (IFormFileCollection file) => "");//上传多个文件

app.MapGet("/mapCustom", (SizeDetails d) => "");
/*
这里在类中实现了static ValueTask<T?> BindAsync(HttpContext context);接口
所以该类可以实现自定义绑定，当满足一定条件的时候，才进行model binding

至此，所有model binding的源已经有了，但是绑定的查找顺序如下：
1. If the parameter defines an explicit binding source using attributes such
as [FromRoute], [FromQuery], or [FromBody], the parameter binds to
that part of the request.
2. If the parameter is a well-known type such as HttpContext,
HttpRequest, Stream, or IFormFile, the parameter is bound to the
corresponding value.
3. If the parameter type has a BindAsync() method, use that method for
binding.
4. If the parameter is a string or has an appropriate TryParse() method
(so is a simple type):
a. If the name of the parameter matches a route parameter name, bind
to the route value.
b. Otherwise, bind to the query string.
5. If the parameter is an array of simple types, a string[] or
StringValues, the request is a GET or similar HTTP verb that normally
doesn’t have a request body, bind to the query string.
6. If the parameter is a known service type from the dependency injection
container, bind by injecting the service from the container.
7. Finally, bind to the body by deserializing from JSON.
*/

app.MapGet("/mix/{id}", (int id, [FromHeader]int head, [FromQuery]string name) => "");
/*
这种混合会容易造成阅读障碍，所以可以将他们形成一个类，同时声明[AsParameters]
*/
app.MapGet("/mix/{id}", ([AsParameters] ModelsMix mix) => "");

app.MapGet("/users", (ModelUser user) => "");
//在类中定义属性的时候，加上DataAnnotations中的特性修饰，
//同时可以实现IValidatableObject，在接口中实现特性无法实现的validate。
app.Run();
record struct ProductID(int ID)
{
    public static bool TryParse(string s, out ProductID result)
    {
        if (s is not null && s.StartsWith("p") && int.TryParse(s.AsSpan().Slice(1), out int id))
        {
            result = new ProductID(id);
            return true;
        }
        result = default;
        return false;
    }
}

record Product(int id, string name, double price);

record SizeDetails()
{
    static async ValueTask<SizeDetails?> BindAsync(HttpContext context)
    {
        return null;
    }
}

record struct ModelsMix(int id, [FromHeader] int head, [FromQuery]string name)
{

}

public class ModelUser : IValidatableObject
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set;}

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(FirstName.StartsWith("bs"))
        {
            yield return new ValidationResult("no error");
        }
    }
}