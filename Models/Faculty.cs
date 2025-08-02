using System.ComponentModel.DataAnnotations;

namespace Web_SIMS.Models
{
    public class Faculty
    {
        [Key]
        public int FacultyId { get; set; }

        [Required(ErrorMessage = "Mã giảng viên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã giảng viên không được vượt quá 20 ký tự")]
        [Display(Name = "Mã giảng viên")]
        public string FacultyCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Số điện thoại không được vượt quá 15 ký tự")]
        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [StringLength(100, ErrorMessage = "Khoa không được vượt quá 100 ký tự")]
        [Display(Name = "Khoa")]
        public string? Department { get; set; }

        [StringLength(100, ErrorMessage = "Chuyên môn không được vượt quá 100 ký tự")]
        [Display(Name = "Chuyên môn")]
        public string? Specialization { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Display(Name = "Ngày cập nhật")]
        public DateTime? UpdatedDate { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }
        public virtual ICollection<Course>? Courses { get; set; }
    }
}