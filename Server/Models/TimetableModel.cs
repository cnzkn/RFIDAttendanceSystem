namespace CloudAPI.Models;

public class TimetableModel
{
    /// <summary>
    /// Unique identifier of this model.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID of the section this timetable belongs to.
    /// </summary>
    [Required]
    public Guid SectionId { get; set; }
    
    /// <summary>
    /// ID of the classroom that this timeslot takes place in.
    /// </summary>
    [Required]
    public Guid ClassroomId { get; set; }
    
    /// <summary>
    /// A composite object representing the day of week and timeslot number.
    /// </summary>
    [Required]
    public TimeslotModel Timeslot { get; set; } // Embedded object
    
    
    [ForeignKey(nameof(SectionId))] // C# auto-map
    public virtual SectionModel CourseSection { get; set; }
    
    [ForeignKey(nameof(ClassroomId))] // C# auto-map
    public virtual ClassroomModel Classroom { get; set; }


    public TimetableDto ToDto()
    {
        return new TimetableDto()
        {
            Id = Id,
            Classroom = Classroom.ToDto(),
            Section = CourseSection.ToDto(),
            Timeslot = Timeslot
        };
    }
}
