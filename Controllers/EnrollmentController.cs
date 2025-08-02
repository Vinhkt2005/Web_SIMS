using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web_SIMS.DTOs;
using Web_SIMS.Interfaces.Services;
using Web_SIMS.Models;
using Web_SIMS.ViewModels;

namespace Web_SIMS.Controllers
{
    [Authorize]
    [Route("admin/enrollments")]
    public class EnrollmentController : Controller
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly ILogger<EnrollmentController> _logger;

        public EnrollmentController(
            IEnrollmentService enrollmentService,
            ICourseService courseService,
            IStudentService studentService,
            ILogger<EnrollmentController> logger)
        {
            _enrollmentService = enrollmentService;
            _courseService = courseService;
            _studentService = studentService;
            _logger = logger;
        }

        // GET: /admin/enrollments
        [HttpGet("")]
        public async Task<IActionResult> Index(string searchString, string statusFilter, string courseFilter, int? pageNumber)
        {
            try
            {
                ViewData["CurrentFilter"] = searchString;
                ViewData["StatusFilter"] = statusFilter;
                ViewData["CourseFilter"] = courseFilter;

                var filterOptions = new EnrollmentFilterOptions
                {
                    SearchString = searchString,
                    StatusFilter = statusFilter,
                    CourseFilter = courseFilter,
                    PageNumber = pageNumber ?? 1,
                    PageSize = 10
                };

                var enrollments = await _enrollmentService.GetEnrollmentsAsync(filterOptions);
                return View(enrollments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollments list");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách đăng ký.";
                return View(new PaginatedList<Enrollment>(new List<Enrollment>(), 0, 1, 10));
            }
        }

        // GET: /admin/enrollments/create
        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Load courses and students for dropdowns
                var courseFilterOptions = new CourseFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var courses = await _courseService.GetCoursesAsync(courseFilterOptions);
                ViewBag.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                var studentFilterOptions = new StudentFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var students = await _studentService.GetStudentsAsync(studentFilterOptions);
                ViewBag.Students = students.Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = $"{s.StudentCode} - {s.FullName}"
                }).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading create enrollment page");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang tạo đăng ký.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/enrollments/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StudentId,CourseId,Status,Grade,Notes")] Enrollment enrollment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    enrollment.EnrollmentDate = DateTime.Now;
                    await _enrollmentService.CreateEnrollmentAsync(enrollment);
                    TempData["SuccessMessage"] = "Tạo đăng ký thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while creating enrollment");
                    ModelState.AddModelError("", "Có lỗi xảy ra khi tạo đăng ký.");
                }
            }

            // Reload dropdowns if validation fails
            try
            {
                var courseFilterOptions = new CourseFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var courses = await _courseService.GetCoursesAsync(courseFilterOptions);
                ViewBag.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                var studentFilterOptions = new StudentFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var students = await _studentService.GetStudentsAsync(studentFilterOptions);
                ViewBag.Students = students.Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = $"{s.StudentCode} - {s.FullName}"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading dropdowns");
            }

            return View(enrollment);
        }

        // GET: /admin/enrollments/edit/{id}
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id.Value);
                if (enrollment == null)
                {
                    return NotFound();
                }

                // Load courses and students for dropdowns
                var courseFilterOptions = new CourseFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var courses = await _courseService.GetCoursesAsync(courseFilterOptions);
                ViewBag.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                var studentFilterOptions = new StudentFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var students = await _studentService.GetStudentsAsync(studentFilterOptions);
                ViewBag.Students = students.Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = $"{s.StudentCode} - {s.FullName}"
                }).ToList();

                return View(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollment for edit: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin đăng ký.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/enrollments/edit/{id}
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EnrollmentId,StudentId,CourseId,Status,Grade,Notes,EnrollmentDate")] Enrollment enrollment)
        {
            if (id != enrollment.EnrollmentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var success = await _enrollmentService.UpdateEnrollmentAsync(id, enrollment);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "Cập nhật đăng ký thành công.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating enrollment: {EnrollmentId}", id);
                    ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật đăng ký.");
                }
            }

            // Reload dropdowns if validation fails
            try
            {
                var courseFilterOptions = new CourseFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var courses = await _courseService.GetCoursesAsync(courseFilterOptions);
                ViewBag.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                var studentFilterOptions = new StudentFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var students = await _studentService.GetStudentsAsync(studentFilterOptions);
                ViewBag.Students = students.Select(s => new SelectListItem
                {
                    Value = s.StudentId.ToString(),
                    Text = $"{s.StudentCode} - {s.FullName}"
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reloading dropdowns");
            }

            return View(enrollment);
        }

        // GET: /admin/enrollments/delete/{id}
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id.Value);
                if (enrollment == null)
                {
                    return NotFound();
                }

                return View(enrollment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting enrollment for delete: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin đăng ký.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/enrollments/delete/{id}
        [HttpPost("delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var success = await _enrollmentService.DeleteEnrollmentAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Xóa đăng ký thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đăng ký để xóa.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting enrollment: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa đăng ký.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/enrollments/bulk-create
        [HttpGet("bulk-create")]
        public async Task<IActionResult> BulkCreate(int? courseId)
        {
            try
            {
                // Load courses for dropdown
                var filterOptions = new CourseFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var courses = await _courseService.GetCoursesAsync(filterOptions);
                ViewBag.Courses = courses.Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = $"{c.CourseCode} - {c.CourseName}"
                }).ToList();

                // If courseId is provided, set it as selected
                if (courseId.HasValue)
                {
                    ViewBag.SelectedCourseId = courseId.Value;
                }

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading bulk create page");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải trang đăng ký hàng loạt.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/enrollments/bulk-create
        [HttpPost("bulk-create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(int courseId, List<int> studentIds)
        {
            try
            {
                if (studentIds == null || !studentIds.Any())
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sinh viên.";
                    return RedirectToAction(nameof(BulkCreate), new { courseId });
                }

                var successCount = 0;
                var errorMessages = new List<string>();

                foreach (var studentId in studentIds)
                {
                    try
                    {
                        var enrollment = new Enrollment
                        {
                            StudentId = studentId,
                            CourseId = courseId,
                            Status = EnrollmentStatus.Pending,
                            EnrollmentDate = DateTime.Now
                        };

                        await _enrollmentService.CreateEnrollmentAsync(enrollment);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating enrollment for student {StudentId} in course {CourseId}", studentId, courseId);
                        errorMessages.Add($"Lỗi khi đăng ký sinh viên ID {studentId}");
                    }
                }

                if (successCount > 0)
                {
                    TempData["SuccessMessage"] = $"Đã đăng ký thành công {successCount} sinh viên.";
                }

                if (errorMessages.Any())
                {
                    TempData["ErrorMessage"] = string.Join("; ", errorMessages);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during bulk enrollment");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đăng ký hàng loạt.";
                return RedirectToAction(nameof(BulkCreate), new { courseId });
            }
        }

        // POST: /admin/enrollments/update-status
        [HttpPost("update-status")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int enrollmentId, EnrollmentStatus status)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(enrollmentId);
                if (enrollment == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đăng ký." });
                }

                enrollment.Status = status;
                var success = await _enrollmentService.UpdateEnrollmentAsync(enrollmentId, enrollment);

                if (success)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể cập nhật trạng thái." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating enrollment status: {EnrollmentId}", enrollmentId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái." });
            }
        }

        // POST: /admin/enrollments/approve/{id}
        [HttpPost("approve/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                if (enrollment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                enrollment.Status = EnrollmentStatus.Approved;
                var success = await _enrollmentService.UpdateEnrollmentAsync(id, enrollment);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã chấp nhận đăng ký thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể chấp nhận đăng ký.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving enrollment: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi chấp nhận đăng ký.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/enrollments/approve/{id}
        [HttpGet("approve/{id}")]
        public async Task<IActionResult> ApproveGet(int id)
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                if (enrollment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                enrollment.Status = EnrollmentStatus.Approved;
                var success = await _enrollmentService.UpdateEnrollmentAsync(id, enrollment);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã chấp nhận đăng ký thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể chấp nhận đăng ký.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving enrollment: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi chấp nhận đăng ký.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /admin/enrollments/reject/{id}
        [HttpPost("reject/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string rejectReason = "")
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                if (enrollment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                enrollment.Status = EnrollmentStatus.Rejected;
                if (!string.IsNullOrEmpty(rejectReason))
                {
                    enrollment.Notes = string.IsNullOrEmpty(enrollment.Notes) 
                        ? $"Lý do từ chối: {rejectReason}" 
                        : $"{enrollment.Notes}\nLý do từ chối: {rejectReason}";
                }
                
                var success = await _enrollmentService.UpdateEnrollmentAsync(id, enrollment);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã từ chối đăng ký thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể từ chối đăng ký.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting enrollment: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi từ chối đăng ký.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/enrollments/reject/{id}
        [HttpGet("reject/{id}")]
        public async Task<IActionResult> RejectGet(int id, string rejectReason = "")
        {
            try
            {
                var enrollment = await _enrollmentService.GetEnrollmentByIdAsync(id);
                if (enrollment == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đăng ký.";
                    return RedirectToAction(nameof(Index));
                }

                enrollment.Status = EnrollmentStatus.Rejected;
                if (!string.IsNullOrEmpty(rejectReason))
                {
                    enrollment.Notes = string.IsNullOrEmpty(enrollment.Notes) 
                        ? $"Lý do từ chối: {rejectReason}" 
                        : $"{enrollment.Notes}\nLý do từ chối: {rejectReason}";
                }
                
                var success = await _enrollmentService.UpdateEnrollmentAsync(id, enrollment);

                if (success)
                {
                    TempData["SuccessMessage"] = "Đã từ chối đăng ký thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể từ chối đăng ký.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting enrollment: {EnrollmentId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi từ chối đăng ký.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /admin/enrollments/check-enrollment-status
        [HttpGet("check-enrollment-status")]
        public async Task<IActionResult> CheckEnrollmentStatus(int studentId, int courseId)
        {
            try
            {
                var isEnrolled = await _enrollmentService.IsStudentEnrolledInCourseAsync(studentId, courseId);
                var canEnroll = await _enrollmentService.CanEnrollStudentInCourseAsync(studentId, courseId);

                return Json(new { isEnrolled, canEnroll });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking enrollment status for student {StudentId} and course {CourseId}", studentId, courseId);
                return Json(new { isEnrolled = false, canEnroll = false, error = "Có lỗi xảy ra khi kiểm tra trạng thái đăng ký." });
            }
        }

        // GET: /admin/enrollments/get-available-students/{courseId}
        [HttpGet("get-available-students/{courseId}")]
        public async Task<IActionResult> GetAvailableStudents(int courseId)
        {
            try
            {
                // Get course information
                var course = await _courseService.GetCourseByIdAsync(courseId);
                if (course == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy khóa học." });
                }

                // Get all active students
                var studentFilterOptions = new StudentFilterOptions
                {
                    StatusFilter = "active",
                    PageSize = 1000
                };
                var allStudents = await _studentService.GetStudentsAsync(studentFilterOptions);

                // Get students already enrolled in this course
                var enrolledStudents = await _enrollmentService.GetEnrollmentsByCourseAsync(courseId);
                var enrolledStudentIds = enrolledStudents.Select(e => e.StudentId).ToHashSet();

                // Filter out already enrolled students
                var availableStudents = allStudents
                    .Where(s => !enrolledStudentIds.Contains(s.StudentId))
                    .Select(s => new
                    {
                        studentId = s.StudentId,
                        studentCode = s.StudentCode,
                        fullName = s.FullName,
                        email = s.Email,
                        major = s.Major,
                        academicYear = s.AcademicYear
                    })
                    .ToList();

                // Prepare course information
                var currentEnrollments = enrolledStudents.Count();
                var remainingSlots = course.MaxStudents - currentEnrollments;
                
                var courseInfo = new
                {
                    courseId = course.CourseId,
                    courseCode = course.CourseCode,
                    courseName = course.CourseName,
                    maxStudents = course.MaxStudents,
                    currentEnrollments = currentEnrollments,
                    remainingSlots = remainingSlots
                };

                return Json(new { success = true, course = courseInfo, students = availableStudents });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting available students for course: {CourseId}", courseId);
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải danh sách sinh viên." });
            }
        }
    }
}