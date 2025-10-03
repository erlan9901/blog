Title : ASP.NET中依赖注入和循环依赖问题

Date : 2023-5-20

Tag : C# .NET

---

## 在ASP.NET中依赖注入以及循环依赖问题

在使用.NET开发Web应用时，基本都离不开依赖注入。依赖注入是一种设计模式，是实现控制反转（IoC）的方式，目的是为了降低代码耦合度。比如，A类中需要用到B类，这就是对B类有依赖，传统的做法是在A类里面new一个B类，这样写以后若要修改就很麻烦，要是有一百个A类就需要修改一百个地方。

依赖注入就可以完全把实例化操作交给.NET框架，不需要自己手动完成，只需要在Program.cs里面注册依赖关系

```C#
builder.Services.AddScoped<IStudentService, StudentService>();
```

这里就是注册一个IStudent类型的服务，当有类中需要IStudentService时，.NET框架就会去查找IStudentService对应的实现，也就是StudentService，然后创建这个实例，传递给类的构造函数。

所以.NET中的依赖注入更像是一个"自动化工厂"，在你需要对象时，自动帮你new一个出来，不需要你手动进行操作。

这里的AddScoped意思是在每个Http请求中都是用同一个实例。同样的，还有AddTransient（瞬时的，每次获取服务都会创建一个新的实例）和AddSingleton（单例的，在整个应用程序中都只有这一个实例）。

**循环依赖问题**即注册的两个服务中的构造函数都相互依赖对方，比如在IStudentService的实现中依赖于ICourseService，而ICourseService的实现也依赖IStudentService

```c#
public class StudentService : IStudentService
{
    private readonly ICourseService courseService;

    public StudentService(ICourseService courseService)	//依赖ICourseService服务
    {
        this.courseService = courseService;
    }
}
```

```C#
public class CourseService : ICourseService
{
    private readonly IStudentService studentService;

    public CourseService(IStudentService studentService)  //依赖IStudentService服务
    {
        this.studentService = studentService;
    }
}
```

这样一来，当某个类需要IStudentService服务时，.NET框架会去试图创建一个StudentService实例，发现StudentService中依赖ICourseService服务，转而再去创建CourseService实例，但是CourseService又依赖于IStudentService，这就造成了死循环，框架无法解析，抛出InvalidOperationException异常。

**如何解决？**

最根本的办法就是重新设计依赖关系，出现循环依赖就说明业务逻辑设计的就有问题，StudentService不该依赖于ICourseService的实现，同样的CourseService也不应该依赖IStudentService，它们都应该依赖于另一个第三者服务，比如IStudentCourseService，把这个服务作为中间站，Student和Course中需要互相依赖的公共逻辑放到这里面，实现解除两个服务的相互依赖。

另一个办法是使用延迟解析，.NET自带的延迟加载工具Lazy<T>，它不会马上创建依赖，而是等到需要用到的时候，访问.Value时，再去创建依赖。

```C#
public class StudentService : IStudentService
{
    private readonly Lazy<ICourseService> courseService;

    public StudentService(Lazy<ICourseService> courseService)	//依赖ICourseService服务
    {
        this.courseService = courseService;
    }
    
    public string GetCourseName(int id){
        int courseId;
        // ...获取CourseID的一些逻辑操作
        courseService.Value.GetById(courseId);	//直到真正访问GetCourseName方法时才会创建ICourseService依赖
    }
}
```

我仍然不推荐使用延迟加载方法，循环依赖问题的出现，根本原因就是业务逻辑出现了问题，重新更改业务逻辑才是最推荐的做法，使用延迟加载是没有办法中的办法。