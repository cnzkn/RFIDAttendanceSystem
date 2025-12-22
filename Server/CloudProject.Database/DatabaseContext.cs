using CloudProject.Database.Utility;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CloudProject.Database;

// A repository pattern could be better here, but passing DatabaseContext directly to consumers due to time constraints.
public class DatabaseContext : IdentityDbContext<UserModel, IdentityRole<Guid>, Guid>
{
    private readonly AttendanceRegistrarEntityResolver _registrarResolver;
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        _registrarResolver = new(this);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.AddInterceptors(new ResolverAttachmentInterceptor(_registrarResolver));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<DeviceModel>()
            .Property(x => x.Fingerprint)
            .HasConversion(x => Convert.ToBase64String(x), y => Convert.FromBase64String(y));

        modelBuilder.Entity<TimetableModel>()
            .Property(x => x.Timeslot)
            .HasColumnType("jsonb")
            .HasConversion(x => JsonSerializer.Serialize(x),
                y => JsonSerializer.Deserialize<TimeslotModel>(y)!);
    }

    public DbSet<AttendanceLogModel> AttendanceLogs { get; set; }
    public DbSet<AttendeeModel> Attendee { get; set; }
    public DbSet<ClassroomModel> Classrooms { get; set; }
    public DbSet<CourseModel> Courses { get; set; }
    public DbSet<DeviceModel> Devices { get; set; }
    public DbSet<SectionModel> Sections { get; set; }
    public DbSet<SemesterModel> Semesters { get; set; }
    public DbSet<TimetableModel> Timetables { get; set; }
}

public class ResolverAttachmentInterceptor(AttendanceRegistrarEntityResolver registrarResolver) : IMaterializationInterceptor
{
    public object CreatedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is AttendanceLogModel log)
        {
            log.AttachResolver(registrarResolver);
        }
        return instance;
    }

    public object InitializedInstance(MaterializationInterceptionData materializationData, object instance)
    {
        if (instance is AttendanceLogModel log)
        {
            log.AttachResolver(registrarResolver);
        }
        return instance;
    }
}