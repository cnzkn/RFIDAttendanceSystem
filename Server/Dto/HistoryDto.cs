namespace CloudAPI.Dto;

public class HistoryStudentDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public Dictionary<string, string> Attendance { get; set; }
}

public class HistoryDto
{
    public string CourseName { get; set; }
    public string CourseCode { get; set; }
    public string Section { get; set; }
    public List<HistoryStudentDto> Students { get; set; }
    public int Weeks { get; set; }
    public int DaysPerWeek { get; set; }
}
