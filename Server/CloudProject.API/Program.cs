Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });
});

builder.Services.ConfigureApplicationCookie(x =>
{
    x.LoginPath = "/User/Login";
    x.LogoutPath = "/User/Logout";
    x.AccessDeniedPath = "/User/AccessDenied";
    x.SlidingExpiration = true;
    x.Cookie.HttpOnly = true;
    x.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;
    x.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
});

builder.Services.AddApplicationDatabase();
builder.Services.AddBusiness();

builder.Services.AddIdentity<UserModel, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://umutsen2662.github.io")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddScoped<IEntityResolver<IAttendanceRegistrar, Guid>, AttendanceRegistrarEntityResolver>();
builder.Services.AddScoped<ICertificateValidator, DeviceCertificateValidator>();
builder.Services.AddSingleton<IModuleHandler, ModuleHandler>();
builder.Services.AddSingleton<IClientHandler, ClientHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
await using (var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>())
{
    await db.Database.MigrateAsync();
    
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseRouting();

app.UseCors("AllowMyFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30)
});

app.MapControllers();

var moduleHandler = app.Services.GetRequiredService<IModuleHandler>();
var clientHandler = app.Services.GetRequiredService<IClientHandler>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStopping.Register(() =>
{
    Console.WriteLine("Application stopping, closing WebSockets...");
    moduleHandler.CloseAllAsync(CancellationToken.None)
        .GetAwaiter()
        .GetResult();
    
    clientHandler.CloseAllAsync(CancellationToken.None)
        .GetAwaiter()
        .GetResult();
});


app.Run();
