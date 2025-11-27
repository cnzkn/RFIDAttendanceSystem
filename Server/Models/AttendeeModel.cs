namespace CloudAPI.Models;

public class AttendeeModel
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    public int StudentID { get; set; }
    
    [Required]
    public string FullName { get; set; }
    
    [Required]
    public byte[] CardUID { get; set; }
    
    
    [InverseProperty("Attendees")]
    public virtual List<SectionModel> AttendingSections { get; set; }
}
