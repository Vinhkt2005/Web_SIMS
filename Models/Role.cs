using System.ComponentModel.DataAnnotations;

namespace Web_SIMS.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự")]
        [Display(Name = "Tên vai trò")]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Mô tả không được vượt quá 200 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<User>? Users { get; set; }
    }
}