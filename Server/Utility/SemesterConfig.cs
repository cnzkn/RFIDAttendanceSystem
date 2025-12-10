using System.Globalization;

namespace CloudAPI.Utility;

public static class SemesterConfig
{
    // Fall 2025 Start: Week 40 (approx Sept 29, 2025)
    // This allows Week 50 (Dec 9) to be around Week 11.
    public const int SemesterStartWeek = 40; 

    public static int GetSemesterWeek(DateTime date)
    {
        int isoWeek = ISOWeek.GetWeekOfYear(date);
        int semesterWeek = isoWeek - SemesterStartWeek + 1;
        return semesterWeek > 0 ? semesterWeek : 1;
    }

    public static int GetIsoWeek(int semesterWeek)
    {
        return SemesterStartWeek + semesterWeek - 1;
    }
}
