Title : ASP.NET MVC路由冲突问题

Date : 2023-5-9

Tag : C# .NET

---

## ASP.NET MVC路由冲突问题

问题起因，是给项目控制器添加属性路由时，直接访问首页（也就是"/"路径）发现找不到控制器。

```C#
[Authorize]
[Route("Home")]
public class HomeController : Controller
{
    private readonly IStudentService studentService;

    public HomeController(IStudentService studentService)
    {
        this.studentService = studentService;
    }

    [Route("Index")]
    public async Task<IActionResult> Index(GetStudentInput input)
    {
        var model = await studentService.GetPagedResult(input);
        return View("Index", model);
    }
}
```

这样加上[Route]后，直接访问"/"变得行不通，试了几次无果。最终发现是由于默认路由（约定路由）和属性路由冲突，我在Program类中定义

```C#
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
```

相当于告诉框架我启用了默认路由，按照控制器名/方法名/id来匹配路径，如果不输入那就默认Home/Index/
这样一来既可以让用户在请求"/"这个路径时访问到Home/Index，又可以在别的控制器写自定义的属性路由。

但是.NET不支持这样做，这会导致路由冲突，也就是属性路由覆盖掉了默认路由，如果我们现在访问"/"这个url会发现返回404找不到页面。这是因为Home控制器已经被属性路由接管，不能参与到默认路由的匹配，必须要访问Home/Index这个路径才行。

**正确做法**是去掉默认路由，在Program类中注册属性路由

```c#
app.MapControllers();
```

然后给Home控制器和Index方法都添加上一个根路由

```c#
[Authorize]
[Route("")]
[Route("Home")]
public class HomeController : Controller
{
    private readonly IStudentService studentService;

    public HomeController(IStudentService studentService)
    {
        this.studentService = studentService;
    }
	
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(GetStudentInput input)
    {
        var model = await studentService.GetPagedResult(input);
        return View("Index", model);
    }
}
```

这样也能实现，在不输入url的情况下默认访问Home/Index