Title : 深入理解ASP.NET Core构建Web应用的启动流程

Date : 2023-12-8

Tag : C# .NET

---

## 深入理解ASP.NET Core构建Web应用的启动流程

每次用.NET写Web应用配置Program类的时候都是从文档或书上生搬硬套，基本都不理解为什么要这么写，为什么服务注册顺序很重要。今天查了很多遍MSDN的文档加上谷歌各种搜索，总算是弄明白了大概。

先贴上一份完整的Program.cs代码

```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 访问配置和环境
        var configuration = builder.Configuration;
        var env = builder.Environment;

        //MVC
        builder.Services.AddControllersWithViews();

        //Razor
        if (env.IsDevelopment())
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
        else
            builder.Services.AddRazorPages();

        //DbContext
        builder.Services.AddDbContextPool<AppDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("SchoolDBConnections"));
        });

        //Identity
        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                        .AddErrorDescriber<CustomIdentityErrorDescriber>()
                        .AddEntityFrameworkStores<AppDbContext>()
                        .AddDefaultTokenProviders();

        //Identity配置
        builder.Services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 3;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = true;
        });

        //授权验证
        builder.Services.AddAuthorizationBuilder()
                        .AddPolicy("DeleteUserPolicy", p => p.RequireClaim("Permission", "DeleteUser"))
                        .AddPolicy("DeleteRolePolicy", p => p.RequireRole("Administrator", "SuperAdministrator"));

        //账户验证
        builder.Services.AddAuthentication()
        .AddMicrosoftAccount(options =>
        {
            options.ClientId = configuration["Authentication:Microsoft:ClientId"];
            options.ClientSecret = configuration["Authentication:Microsoft:ClientSecret"];
            options.CallbackPath = configuration["Authentication:Microsoft:CallbackPath"];
        })
        .AddGitHub(options =>
        {
            options.ClientId = configuration["Authentication:GitHub:ClientId"];
            options.ClientSecret = configuration["Authentication:GitHub:ClientSecret"];
            options.CallbackPath = configuration["Authentication:GitHub:CallbackPath"];
        });

        //Application配置
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/SignIn";
            options.LogoutPath = "/Account/SignOut";
            options.AccessDeniedPath = "/Error/AccessDenied";
        });

        //自定义仓储 / 服务
        builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IStudentService, StudentService>();
        builder.Services.AddScoped<ICourseService, CourseService>();
        builder.Services.AddScoped<ITeacherService, TeacherService>();

        //构建
        StartUp(builder);
    }

    public static void StartUp(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        //数据初始化
        app.UseDataInitializer();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        else if (app.Environment.IsStaging() || app.Environment.IsProduction())
        {
            app.UseExceptionHandler("/Exception");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHsts();
        }

        app.UseStaticFiles();

        app.UseAuthentication();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllers();

        app.MapRazorPages();

        app.Run();
    }

}
```



##### 第一步，应用程序的入口

```c#
var builder = WebApplication.CreateBuilder(args);
```

这行代码起到三个作用:

构建主机(Host)，加载 Kestrel 服务器、日志系统、配置系统等

创建依赖注入容器，后面所有builder.Services.Add添加的服务都放在容器里面

加载应用配置和环境变量，appsettings.json，launchSettings.json还有命令行参数等等

这就搭建好了运行所需的基础环境，相当于一个"工厂"



##### 第二步，服务注册

```C#
builder.Services.AddControllersWithViews();	// 启用Mvc服务支持
```

让程序能使用传统的Mvc模式，一定要放在最前面，后面的身份认证，授权验证都需要依赖它



```c#
if (env.IsDevelopment())
    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
else
    builder.Services.AddRazorPages();
```

这里是配置RazorPages支持，主要是方便在开发环境可以让页面热更新



```c#
builder.Services.AddDbContextPool<AppDbContext>(options =>
{
    options.UseSqlServer(configuration.GetConnectionString("SchoolDBConnections"));
});
```

使用EFCore管理数据库连接，注册AppDbContext数据库上下文，这里用AddDbContextPool而不是AddDbContext是可以通过连接池重复利用上下文，减少性能损耗，代价是安全性会差一些。



```c#
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddErrorDescriber<CustomIdentityErrorDescriber>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
```

注册Identity身份系统，里面包含多个服务，常用的有UserManager，SignInManager，RoleManager等。AddErrorDescriber用于自定义错误提示信息，AddEntityFrameworkStores可以让Identity使用EFCore操作数据库，创建用于身份认证的表，AddDefaultTokenProviders用于提供邮箱验证，密码重置的Token功能。



```c#
builder.Services.AddAuthorizationBuilder()
                .AddPolicy("DeleteUserPolicy", p => p.RequireClaim("Permission", "DeleteUser"))
                .AddPolicy("DeleteRolePolicy", p => p.RequireRole("Administrator", "SuperAdministrator"));
```

这里是授权验证，定义哪些控制器可以访问，哪些不能访问。与身份认证的区别在于，身份认证是问：“你是谁”，而授权验证是说：“你能够做什么”。当控制器上使用[Authorize(Policy = "DeleteUserPolicy")]时就会触发授权验证。



```c#
builder.Services.AddAuthentication()
.AddMicrosoftAccount(options => {...})
.AddGitHub(options => {...});
```

这里就是添加身份认证了，同时还添加第三方账户认证，Microsoft和Gitub。这里和上面的授权验证的顺序不重要。



```c#
builder.Services.ConfigureApplicationCookie(options => {...});
builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));
...
```

后面就是配置用户登录，登出，还有拒绝访问的跳转路径，以及自定义依赖注入的服务，这里写AddScoped代表同一个Http请求中使用同一个服务实例。



##### 第三步，构建应用

把整个构建应用相关的代码全部放到StartUp方法里，便于区分。

```c#
var app = builder.Build();
app.UseDataInitializer();
```

这里两行是开始构建应用，以及初始化数据，主要方便在开发环境能有一些数据用于调试。



```c#
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

else if (app.Environment.IsStaging() || app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Exception");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    app.UseHsts();
}
```

配置异常处理页面，在开发环境直接就使用开发人员错误页面，而生产或演示环境就用自定义的错误页面，这里有两个，UseExceptionHandler用于捕获应用程序本身产生的异常，UseStatusCodePagesWithReExecute用于捕获状态码错误的异常，UseExceptionHandler一定要在前面，否则容易所有异常全部被UseStatusCodePagesWithReExecute捕获。UseHsts强制使用HTTPS。



```c#
app.UseStaticFiles();
```

使用静态文件，提供wwwroot中的文件，这个一定要放在路由中间件前面，否则会被Mvc截取



```c#
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
```

这三行分别是，使用身份认证中间件，使用路由中间件，使用授权验证中间件。这三个的顺序很重要，先识别用户的身份是否被允许，再进行匹配路由，最后根据授权决定用户是否可以进行访问。



```c#
app.MapControllers();
app.MapRazorPages();
```

这两行是注册Controller的属性路由和注册Razor页面，一定要放在UseRouting之后，否则路由还有建立。



```c#
app.Run();
```

最后运行应用，开始监听Http请求，这一行之后的代码用于不会被执行了。

整个启动流程大概就是，建立容器，配置服务，使用中间件，运行应用。