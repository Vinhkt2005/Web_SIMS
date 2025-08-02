using System.ComponentModel.DataAnnotations;

namespace Web_SIMS.ViewModels
{
    public class CreateFacultyViewModel
    {
        // Thông tin giảng viên
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

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khoa là bắt buộc")]
        [Display(Name = "Khoa")]
        public string Department { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chuyên môn là bắt buộc")]
        [Display(Name = "Chuyên môn")]
        public string Specialization { get; set; } = string.Empty;

        // Thông tin tài khoản (tùy chọn)
        [StringLength(50, ErrorMessage = "Tên đăng nhập không được vượt quá 50 ký tự")]
        [Display(Name = "Tên đăng nhập")]
        public string? Username { get; set; }

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        public string? ConfirmPassword { get; set; }
    }
}