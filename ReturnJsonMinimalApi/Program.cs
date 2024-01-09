using System.Collections.Concurrent;
using System.Net.Mime;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();//该语句将IProblemDetailsService注册进入容器中
                                    //就能使得所有有Exception出现的时候，会返回ProblemDetail格式的说明

var app = builder.Build();

app.UseExceptionHandler(); //当使用该错误中间件时，引入了IProblemDetailsService服务就不能再填写错误路径

app.UseStatusCodePages(); //添加该中间件，使得将出现的Error转换为ProblemDetail格式

app.MapGet("/", (o) => throw new Exception("robot"));

app.MapGet("/s", () => Results.NotFound());

/*
http api的使用场景：
1. 当前端已经实现了，但需要有后端数据支撑的时候
2. 需要提供服务接口给其他后端程序调用
*/

/*
minimalApi匹配api路径返回的类型有如下：
1. void
2. string
3. 具体类
4. IResult

当使用前3种作为返回结构的时候，报错发生时页面对于用户不友好，而这时
可以采用第4种返回类型。
*/
app.MapGet("/fruit/{id}", (string id) => 
    Fruit.All.TryGetValue(id, out Fruit result) 
    ? Results.Ok(result)
    : Results.NoContent()
);
app.MapGet("/fruit", () => Fruit.All);


app.MapGet("/fruit/self", (HttpResponse response) => 
{
    response.StatusCode = 204;
    response.ContentType = MediaTypeNames.Text.Plain;
    return response.WriteAsync("i'm a robot");
});

app.MapGet("/fruit/file/{file}", (string file) => Results.File(file));

app.MapPost("/fruit/{id}", (string id) => 
    Fruit.All.TryGetValue(id, out var result)
    ? TypedResults.Ok(result)
    : Results.ValidationProblem(new Dictionary<string, string[]>{{"id", new[] {"this id is not exist!"}}}, statusCode: 400) //ValidationProblem方法可返回标准的web错误报警信息，使得报错信息有统一的结构
    //此为采取第四种返回类型IResult
    //TypedResults跟Results类的接口都是类似的，唯一区别是Results返回值类型是IResult，而TypedResults则是具体的类。
    //这两个类可用于返回正确的HTTP状态值而不需要自己手动去构建Httpresponse结构。

/*
当错误发生的时候，如果使用Results/TypeResults的错误返回方法，如nocontent或badrequest
则对于使用的客户端而言，返回的错误信息结构多样，不利于处理。
所以有Problem Detail这项公共的错误返回格式说明。该说明定义了所有的字段和自己定义的字段，采取json的格式返回。
1. Results.Problem 默认返回500
2. Results.ValidationProblem 默认返回400且需要添加自己的自定义错误说明

建议全部采取返回Problem Detail的格式说明作为错误的信息提示格式

*/

); //当以正常浏览器GET的方式请求该路径的时候，会报405的错误

app.MapPost("/fruit/{id}", Handler.AddFruit);//这里请求路径id会被赋值到委托中的id，而请求体则会被反序列化为Fruit


/*
对于上面的，均为endpoint的返回值采取Results.ValidationProblem，而不是统一所有可能出现的情况。
1. 将异常Exception转换为Problem Detail格式 -> UseExceptionHandler该中间件 + IProblemDetailsService
2. 将错误Error转换为Problem Detail格式 -> 统一，所以采取加StatusCodePagesMiddleware中间件的方法，判断返回的时候是否有报错

*/

app.Run();
record Fruit(string name, int stock)
{
    public static readonly ConcurrentDictionary<string, Fruit> All = new(); 
}

class Handler
{
    public static void AddFruit(string id, Fruit newfruit)
    {
        Fruit.All.TryAdd(id, newfruit);
    }
}