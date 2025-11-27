namespace CloudAPI.Models;

public class AttendanceLogModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public Guid TimetableId { get; set; }
    
    [Required]
    public int WeekNumber { get; set; }
    
    [Required]
    public Guid AttendeeId { get; set; }
    
    [Required]
    public Guid MarkedById { get; set; }
    
    [Required]
    public bool IsPresent { get; set; }
    
    
    [ForeignKey(nameof(TimetableId))] // C# auto-map
    public virtual TimetableModel Timetable { get; set; }
    
    [ForeignKey(nameof(AttendeeId))] // C# auto-map
    public virtual AttendeeModel Attendee { get; set; }
}
