using System;

namespace AvaloniaPdbAccounts.Models
{
    public class CourseModel
    {
        public string CourseID { get; set; } = string.Empty;  // MAMM (e.g. MTH00003_1_2024)
        public string BaseCode { get; set; } = string.Empty;  // MAHP (e.g. MTH00003)
        public string TeacherID { get; set; } = string.Empty; // MAGV
        public int Semester { get; set; }                     // HK
        public int Year { get; set; }                         // NAM
    }
} 