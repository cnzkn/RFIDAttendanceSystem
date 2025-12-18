namespace CloudProject.Database.Models;

public class AttendanceLogModel : IEntity
{
    /// <inheritdoc />
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Date that this log entry was added.
    /// </summary>
    [Required]
    public DateTime Date { get; set; }
    
    /// <summary>
    /// ID of the timetable this log affects.
    /// </summary>
    [Required]
    public Guid TimetableId { get; set; }
    
    /// <summary>
    /// Week of the year.
    /// </summary>
    [Required]
    public int WeekNumber { get; set; }
    
    /// <summary>
    /// ID of the attendee this log affects.
    /// </summary>
    [Required]
    public Guid AttendeeId { get; set; }
    
    /// <summary>
    /// ID of the entity that affects this attendee's attendance status.
    /// </summary>
    [Required]
    public Guid MarkedById { get; set; }
    
    /// <summary>
    /// Type of the entity that affects this attendee's attendance status.
    /// </summary>
    [Required]
    public string MarkedByType { get; set; }
    
    /// <summary>
    /// Whether the attendee is present or not.
    /// </summary>
    [Required]
    public bool IsPresent { get; set; }
    
    
    [ForeignKey(nameof(TimetableId))] // C# auto-map
    public virtual TimetableModel Timetable { get; set; }
    
    [ForeignKey(nameof(AttendeeId))] // C# auto-map
    public virtual AttendeeModel Attendee { get; set; }

    [NotMapped]
    public IAttendanceRegistrar? Registrar
    {
        get => _resolver.ResolveAsync(MarkedById, MarkedByType).GetAwaiter().GetResult();
        set => (MarkedById, MarkedByType) = _resolver.GetProperties(value);
    }
    
    [NotMapped]
    private IEntityResolver<IAttendanceRegistrar, Guid> _resolver;

    internal void AttachResolver(IEntityResolver<IAttendanceRegistrar, Guid> resolver)
    {
        _resolver = resolver;
    }
}
