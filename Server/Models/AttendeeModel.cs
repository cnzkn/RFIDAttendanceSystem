namespace CloudAPI.Models;

public class AttendeeModel
{
    /// <summary>
    /// Unique identifier of this model.
    /// </summary>
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    /// <summary>
    /// Student ID of this attendee.
    /// </summary>
    [Required]
    public int StudentID { get; set; }
    
    /// <summary>
    /// Full name of this attendee.
    /// </summary>
    [Required]
    public string FullName { get; set; }
    
    /// <summary>
    /// Unique identifier of this attendee's ID card.
    /// </summary>
    [Required]
    public byte[] CardUID { get; set; }
    
    
    [InverseProperty("Attendees")]
    public virtual List<SectionModel> AttendingSections { get; set; }


    public AttendeeDto ToDto()
    {
        return new AttendeeDto()
        {
            StudentID = StudentID,
            FullName = FullName,
        };
    }
}
