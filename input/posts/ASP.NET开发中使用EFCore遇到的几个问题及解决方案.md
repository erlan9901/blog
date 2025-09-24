Title : ASP.NET开发中使用EFCore遇到的几个问题及解决方案

Date : 2023-5-18

Tag : C# .NET

---

## ASP.NET开发中使用EFCore遇到的几个问题及解决方案

之前在学习ASP.NET Core开发Web网站时，看了几一些书，但一直没有完整的写一个系统出来。以前都是使用.NET Framework，现在换成.NET Core还是有些变化的，结果这种系统就是在不断的出现Bug与解决Bug中完成的。

第一个问题是使用EFCore进行数据库操作时，报InvalidOperationException错误。例如：

```c#
public async Task<IActionResult> Index()
{
    var stu1 = context.Students.SingleAsync(p => p.ID == 1);
    var stu2 = await context.Students.SingleAsync(p => p.ID == 2);
    return View(stu2);
}
```

这里获取stu1的任务还没有完成，马上就接着获取stu2，造成了同一个DbContext实例被多个进程使用。

虽然这里的根本原因是因为第一个操作前面没有加await导致的，但还是要避免这种情况。尤其是在一次Http请求内需要多次操作数据库的时候，因为DbContext的默认生命周期是Scoped，一次请求内都是同一个实例。



第二个问题是DbContext到底什么时候连接数据库去执行SQL操作，假设这样一段仓储代码：

```
public IQueryable<Student> GetAll(Expression<Func<Student, bool>> predicate)
{
    return context.Students.Where(predicate);
}
```

这个方法需要加异步操作吗？答案是不需要，因为根本就没有去访问数据库。返回的IQueryable接口表示的是一个可构建表达式树的查询，说简单点就是存了一段Sql查询代码，但不会立即执行。直到里面的数据被调用（比如foreach、ToList、First等方法）才会往数据库里去执行这段Sql。

就比如下面这段代码，运行就会报ObjectDisposedException错误。

```c#
IEnumerable<Student> result;
using (AppDbContext context = new AppDbContext(option))
{
    result = context.Students.Where(p => p.ID > 1);
}

foreach (var stu in result)
{
    model.Add(stu);
}

return View(model);
```

因为在using块中只是将一个IQueryable赋值给了result，而里面根本就没有Student的数据，只是一段查询。当foreach想要访问result里面的数据，这时就会去执行查询，结果DbContext已经被释放掉了，就会报ObjectDisposedException错误。解决办法就是在using块里面就把数据全部查询出来，直接在Where后面加上一个ToList就好了。

```c#
result = context.Students.Where(p => p.ID > 1).ToList();
```



第三个问题：
访问导航属性时报NullReferenceException错误，看下错误代码：

```c#
public IActionResult Index()
{
    var stuList = context.Students.ToList();
    IEnumerable<HomeIndexViewModel> model = stuList.Select(p => new HomeIndexViewModel
    {
        StudentName = p.Name,
        CourseName = p.Course.CourseName
    });
    return View(model);
}
```

先用EFCore查出Students然后ToList，再投影为视图模型。但EFCore不会直接把导航属性给你查出来，如果没有启用懒加载，那么查出来的导航属性Course会是null。

解决办法为要么加上Include

```C#
var stuList = context.Students.Include(p => p.Course).ToList();
```

要么启用懒加载，安装Microsoft.EntityFrameworkCore.Proxies包，在Startup中UseLazyLoadingProxies()，然后把导航属性设为virtual，这样在访问的时候会自动发起查询。

最好的办法是不要先ToList有导航属性的类，而是在投影中使用子查询查出单个字段

```c#
public IActionResult Index()
{
    IEnumerable<HomeIndexViewModel> model = context.Students.Select(p => new HomeIndexViewModel
    {
        StudentName = p.Name,
        CourseName = context.Courses.Where(c => c.CourseID == p.CourseID)
                                    .Select(c => c.CourseName)
                                    .First()
    });
    return View(model);
}
```

