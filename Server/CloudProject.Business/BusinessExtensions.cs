namespace CloudProject.Business;

public static class BusinessExtensions
{
    public static IServiceCollection AddBusiness(this IServiceCollection services)
    {
        services.AddScoped<AttendanceManager>();
        services.AddScoped<AttendeeManager>();
        services.AddScoped<ClassroomManager>();
        services.AddScoped<CourseManager>();
        services.AddScoped<DeviceManager>();
        services.AddScoped<SemesterManager>();
        services.AddScoped<TimetableManager>();
        services.AddScoped<UserManager>();

        return services;
    }
}
