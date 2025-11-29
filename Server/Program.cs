Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });
});

//builder.Services.AddEntityFrameworkProxies();
builder.Services.AddDbContext<DatabaseContext>(options =>
{
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var port = Environment.GetEnvironmentVariable("DB_PORT");
    var user = Environment.GetEnvironmentVariable("DB_USER");
    var pass = Environment.GetEnvironmentVariable("DB_PASS");
    var name = Environment.GetEnvironmentVariable("DB_NAME");
    var ssl = Environment.GetEnvironmentVariable("DB_SECURE") ?? "Require";

    options.UseNpgsql($"Host={host};Port={port};Username={user};Password={pass};Database={name};SslMode={ssl};Trust Server Certificate=true");
    options.UseLazyLoadingProxies();
});

builder.Services.AddIdentity<UserModel, IdentityRole<Guid>>()
    .AddEntityFrameworkStores<DatabaseContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(x =>
{
    x.LoginPath = "/User/Login";
    x.LogoutPath = "/User/Logout";
    x.AccessDeniedPath = "/User/AccessDenied";
    x.SlidingExpiration = true;
    x.Cookie.HttpOnly = true;
    x.Cookie.SameSite = SameSiteMode.None;
    x.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
});

builder.Services.AddControllers();

builder.Services.AddScoped<IEntityResolver<IAttendanceRegistrar, Guid>, AttendanceRegistrarEntityResolver>();
builder.Services.AddScoped<ICertificateValidator, DeviceCertificateValidator>();
builder.Services.AddSingleton<IModuleHandler, ModuleHandler>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
await using (var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>())
{
    await db.Database.MigrateAsync();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
