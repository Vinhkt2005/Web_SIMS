namespace Web_SIMS.ViewModels
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan
        public int TotalStudents { get; set; }
        public int TotalCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalUsers { get; set; }
        public int PendingEnrollments { get; set; }
        public int ActiveStudents { get; set; }
        public int ActiveCourses { get; set; }
        public decimal AverageScore { get; set; }

        // Danh sách gần đây
        public List<RecentEnrollmentViewModel> RecentEnrollments { get; set; } = new();
        public List<TopCourseViewModel> TopCourses { get; set; } = new();
        public List<StudentPerformanceViewModel> StudentPerformance { get; set; } = new();
    }

    public class RecentEnrollmentViewModel
    {
        public int EnrollmentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TopCourseViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
        public int MaxStudents { get; set; }
        public decimal EnrollmentRate { get; set; }
    }

    public class StudentPerformanceViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public decimal AverageScore { get; set; }
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
    }
}