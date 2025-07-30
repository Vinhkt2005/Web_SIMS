using System.Collections.Generic;
using Web_SIMS.Models;

namespace Web_SIMS.ViewModels
{
    public class FacultyHomeViewModel
    {
        public int ClassCount { get; set; }
        public int StudentCount { get; set; }
    }

    public class ClassStudentsViewModel
    {
        public Course Course { get; set; } = new Course();
        public List<Enrollment> Enrollments { get; set; } = new();
    }

    public class StudentGradeViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public decimal? MidtermScore { get; set; }
        public decimal? FinalScore { get; set; }
        public decimal? TotalScore { get; set; }
        public string? Grade { get; set; }
    }

    public class ClassGradesViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public List<StudentGradeViewModel> Students { get; set; } = new();
    }
}
