using System.ComponentModel.DataAnnotations;

namespace Web_SIMS.Models
{
    public class Enrollment
    {
        [Key]
        public int EnrollmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Display(Name = "Ngày đăng ký")]
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái")]
        public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        // Navigation Properties
        public virtual Student? Student { get; set; }
        public virtual Course? Course { get; set; }
    }

    public enum EnrollmentStatus
    {
        [Display(Name = "Chờ duyệt")]
        Pending,
        [Display(Name = "Đã duyệt")]
        Approved,
        [Display(Name = "Từ chối")]
        Rejected,
        [Display(Name = "Hoàn thành")]
        Completed,
        [Display(Name = "Đã hủy")]
        Cancelled
    }
}