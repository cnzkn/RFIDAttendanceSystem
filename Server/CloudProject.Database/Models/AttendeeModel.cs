namespace CloudProject.Database.Models;

public class AttendeeModel : IEntity
{
    /// <inheritdoc />
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Student ID of this attendee.
    /// </summary>
    [Required, Range(0, 134217727)] // 27-bits
    public int StudentID { get; set; }
    
    /// <summary>
    /// Full name of this attendee.
    /// </summary>
    [Required, MaxLength(256)]
    public string FullName { get; set; }
    
    /// <summary>
    /// Unique identifier of this attendee's ID card.
    /// </summary>
    [Required, MaxLength(8)]
    public byte[] CardUID { get; set; }
    
    
    [InverseProperty("Attendees")]
    public virtual List<SectionModel> AttendingSections { get; set; }
}
