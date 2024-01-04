var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

/*
在WebApplication中，已经自动帮我们在一开始添加了下列的中间件
1. HostFilteringMiddleware
2. ForwardedHeadersMiddleware
3. DeveloperExceptionPageMiddleware
4. RoutingMiddleware//自动添加在开头
//又因为访问静态文件不需要routing，所以可以先写静态文件中间件，然后再自己明写出routing
这样WebApplication就不会给我们自动在开头添加该中间件了。
5. AuthenticationMiddleware
6. AuthorizationMiddleware
7. EndpointMiddleware//该中间件是加在所有中间件最后，防止我们没有短路中间件。此为虚拟的短路中间件

*/
app.UseDeveloperExceptionPage();//用于开发环境下的报错界面显示，不要用在生产环境中
app.UseExceptionHandler("/error");//处理异常的中间件会捕捉到异常之后，再次返回执行向下的中间件。
//注意，这里如果处理异常的路径再次发生异常，则最终返回到客户端会显示500，并不会有该中间件的处理结果。
//所以这里的异常处理需要非常的简单，甚至不需要动态返回。

app.UseStaticFiles();//访问wwwroot路径中的静态文件，注意访问静态文件不需要routing，所以可以将UseRouting放在该中间件后面。
app.UseRouting();
app.UseWelcomePage();//访问的时候，显示welcome内置界面，短路中间件

app.MapGet("/error", () => "sorry error occur!");
app.Run();
