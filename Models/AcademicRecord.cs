using System.ComponentModel.DataAnnotations;

namespace Web_SIMS.Models
{
    public class AcademicRecord
    {
        [Key]
        public int AcademicRecordId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        [Range(1, 3, ErrorMessage = "Học kỳ phải từ 1 đến 3")]
        [Display(Name = "Học kỳ")]
        public int Semester { get; set; }

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        [Range(2020, 2030, ErrorMessage = "Năm học phải từ 2020 đến 2030")]
        [Display(Name = "Năm học")]
        public int AcademicYear { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm giữa kỳ phải từ 0 đến 10")]
        [Display(Name = "Điểm giữa kỳ")]
        public decimal? MidtermScore { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm cuối kỳ phải từ 0 đến 10")]
        [Display(Name = "Điểm cuối kỳ")]
        public decimal? FinalScore { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm tổng kết phải từ 0 đến 10")]
        [Display(Name = "Điểm tổng kết")]
        public decimal? TotalScore { get; set; }

        [StringLength(2, ErrorMessage = "Xếp loại không được vượt quá 2 ký tự")]
        [Display(Name = "Xếp loại")]
        public string? Grade { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [Display(Name = "Ghi chú")]
        public string? Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual Student? Student { get; set; }
        public virtual Course? Course { get; set; }
    }
}