namespace CloudAPI.Database;

public class DatabaseContext : DbContext
{
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DeviceModel>()
            .Property(x => x.Fingerprint)
            .HasConversion(x => Convert.ToBase64String(x), y => Convert.FromBase64String(y));

        modelBuilder.Entity<TimetableModel>()
            .Property(x => x.Timeslot)
            .HasColumnType("jsonb");
    }

    public DbSet<AttendanceLogModel> AttendanceLogs { get; set; }
    public DbSet<AttendeeModel> Attendee { get; set; }
    public DbSet<ClassroomModel> Classrooms { get; set; }
    public DbSet<CourseModel> Courses { get; set; }
    public DbSet<DeviceModel> Devices { get; set; }
    public DbSet<SectionModel> Sections { get; set; }
    public DbSet<TimetableModel> Timetables { get; set; }
    public DbSet<UserModel> Users { get; set; }
}
