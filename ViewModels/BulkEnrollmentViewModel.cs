using System.ComponentModel.DataAnnotations;
using Web_SIMS.Models;

namespace Web_SIMS.ViewModels
{
    public class BulkEnrollmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn khóa học")]
        public int CourseId { get; set; }
        
        [Required(ErrorMessage = "Vui lòng chọn ít nhất một sinh viên")]
        public List<int> StudentIds { get; set; } = new List<int>();
        
        public string? Notes { get; set; }
        
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
        
        // For display purposes
        public Course? Course { get; set; }
        public List<Student> AvailableStudents { get; set; } = new List<Student>();
        public List<Student> SelectedStudents { get; set; } = new List<Student>();
        
        // Statistics
        public int TotalSelected { get; set; }
        public int MaxStudentsAllowed { get; set; }
        public int CurrentEnrollments { get; set; }
        public int RemainingSlots { get; set; }
    }
    
    public class BulkEnrollmentResultViewModel
    {
        public int TotalProcessed { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int SkippedCount { get; set; }
        
        public List<EnrollmentResult> Results { get; set; } = new List<EnrollmentResult>();
        
        public class EnrollmentResult
        {
            public int StudentId { get; set; }
            public string StudentName { get; set; } = string.Empty;
            public string StudentCode { get; set; } = string.Empty;
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public EnrollmentStatus Status { get; set; }
        }
    }
}