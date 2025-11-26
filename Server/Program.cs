var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
    });
});

builder.Services.AddControllers();
builder.Services.AddScoped<ICertificateValidator, DeviceCertificateValidator>();
builder.Services.AddSingleton<IModuleHandler, ModuleHandler>();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();
