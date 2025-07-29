using System.ComponentModel.DataAnnotations;

namespace Web_SIMS.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Mã khóa học là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã khóa học không được vượt quá 20 ký tự")]
        [Display(Name = "Mã khóa học")]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên khóa học là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên khóa học không được vượt quá 100 ký tự")]
        [Display(Name = "Tên khóa học")]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Số tín chỉ là bắt buộc")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        [Display(Name = "Số tín chỉ")]
        public int Credits { get; set; }

        [Required(ErrorMessage = "Số sinh viên tối đa là bắt buộc")]
        [Range(1, 200, ErrorMessage = "Số sinh viên tối đa phải từ 1 đến 200")]
        [Display(Name = "Số sinh viên tối đa")]
        public int MaxStudents { get; set; }

        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        [Display(Name = "Học kỳ")]
        public string Semester { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        [Display(Name = "Năm học")]
        public string AcademicYear { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giảng viên là bắt buộc")]
        [Display(Name = "Giảng viên")]
        public string Instructor { get; set; } = string.Empty;

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual ICollection<Enrollment>? Enrollments { get; set; }
        public virtual ICollection<AcademicRecord>? AcademicRecords { get; set; }
    }
}