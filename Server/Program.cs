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

    options.UseNpgsql($"Host={host};Port={port};Username={user};Password={pass};Database={name};SslMode={ssl};Trust Server Certificate=true;Include Error Detail={(builder.Environment.IsDevelopment() ? "true" : "false")}");
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
    x.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;
    x.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
});

// Add CORS services and define a policy.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMyFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddScoped<DatabaseSeeder>();
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

app.UseWebSockets();

app.MapControllers();

app.Run();
