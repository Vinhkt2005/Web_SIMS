using Microsoft.EntityFrameworkCore;
using System.Data;
using Web_SIMS.Models;

namespace Web_SIMS.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<AcademicRecord> AcademicRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Password).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordSalt).HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Cấu hình User-Student relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne()
                .HasForeignKey<User>(u => u.StudentId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Cấu hình Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);
            });

            // Cấu hình Student
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasKey(e => e.StudentId);
                entity.Property(e => e.StudentCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Gender).HasMaxLength(10);
                entity.Property(e => e.Major).HasMaxLength(100);
                entity.Property(e => e.AcademicYear).HasMaxLength(20);
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.HasIndex(e => e.StudentCode).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Cấu hình Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.CourseId);
                entity.Property(e => e.CourseCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CourseName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Semester).HasMaxLength(20);
                entity.Property(e => e.AcademicYear).HasMaxLength(20);
                entity.Property(e => e.Instructor).HasMaxLength(100);
                entity.HasIndex(e => e.CourseCode).IsUnique();
            });

            // Cấu hình Enrollment
            modelBuilder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => e.EnrollmentId);
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Cấu hình AcademicRecord
            modelBuilder.Entity<AcademicRecord>(entity =>
            {
                entity.HasKey(e => e.AcademicRecordId);
                entity.Property(e => e.MidtermScore).HasPrecision(5, 2);
                entity.Property(e => e.FinalScore).HasPrecision(5, 2);
                entity.Property(e => e.TotalScore).HasPrecision(5, 2);
                entity.Property(e => e.Grade).HasMaxLength(2);
                entity.Property(e => e.Notes).HasMaxLength(500);
            });

            // Cấu hình relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AcademicRecord>()
                .HasOne(ar => ar.Student)
                .WithMany(s => s.AcademicRecords)
                .HasForeignKey(ar => ar.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AcademicRecord>()
                .HasOne(ar => ar.Course)
                .WithMany(c => c.AcademicRecords)
                .HasForeignKey(ar => ar.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed data
            SeedData(modelBuilder);
        }

        public void SeedData()
        {
            try
            {
                // Seed Roles if not exists
                if (!Roles.Any())
                {
                    Roles.AddRange(
                        new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên hệ thống", IsActive = true },
                        new Role { RoleId = 2, RoleName = "Faculty", Description = "Giảng viên", IsActive = true },
                        new Role { RoleId = 3, RoleName = "Student", Description = "Sinh viên", IsActive = true }
                    );
                    SaveChanges();
                }

                // Seed Students if not exists
                if (!Students.Any())
                {
                    Students.AddRange(
                        new Student { StudentId = 1, StudentCode = "SV001", FullName = "Nguyễn Văn A", Email = "nguyenvana@sims.edu", PhoneNumber = "0123456789", DateOfBirth = new DateTime(2000, 1, 1), Address = "Hà Nội", Gender = "Nam", Major = "Công nghệ thông tin", AcademicYear = "2024-2025", EnrollmentDate = DateTime.Now, IsActive = true, CreatedDate = DateTime.Now },
                        new Student { StudentId = 2, StudentCode = "SV002", FullName = "Trần Thị B", Email = "tranthib@sims.edu", PhoneNumber = "0987654321", DateOfBirth = new DateTime(2001, 5, 15), Address = "TP.HCM", Gender = "Nữ", Major = "Kinh tế", AcademicYear = "2024-2025", EnrollmentDate = DateTime.Now, IsActive = true, CreatedDate = DateTime.Now }
                    );
                    SaveChanges();
                }

                // Seed Users if not exists
                if (!Users.Any())
                {
                    Users.AddRange(
                        new User { UserId = 1, Username = "admin", Password = "admin123", Email = "admin@sims.edu", FullName = "Administrator", RoleId = 1, IsActive = true, CreatedDate = DateTime.Now },
                        new User { UserId = 2, Username = "faculty1", Password = "faculty123", Email = "faculty1@sims.edu", FullName = "Giảng viên 1", RoleId = 2, IsActive = true, CreatedDate = DateTime.Now },
                        new User { UserId = 3, Username = "student1", Password = "student123", Email = "student1@sims.edu", FullName = "Sinh viên 1", RoleId = 3, StudentId = 1, IsActive = true, CreatedDate = DateTime.Now }
                    );
                    SaveChanges();
                }

                // Seed Courses if not exists
                if (!Courses.Any())
                {
                    Courses.AddRange(
                        new Course { CourseId = 1, CourseCode = "CS101", CourseName = "Lập trình Cơ bản", Description = "Khóa học lập trình cơ bản cho sinh viên", Credits = 3, MaxStudents = 50, Semester = "Học kỳ 1", AcademicYear = "2024-2025", Instructor = "TS. Nguyễn Văn Giảng", IsActive = true, CreatedDate = DateTime.Now },
                        new Course { CourseId = 2, CourseCode = "CS102", CourseName = "Cơ sở dữ liệu", Description = "Khóa học về cơ sở dữ liệu", Credits = 3, MaxStudents = 40, Semester = "Học kỳ 1", AcademicYear = "2024-2025", Instructor = "TS. Trần Thị Giảng", IsActive = true, CreatedDate = DateTime.Now }
                    );
                    SaveChanges();
                }

                // Seed Enrollments if not exists
                if (!Enrollments.Any())
                {
                    Enrollments.AddRange(
                        new Enrollment { StudentId = 1, CourseId = 1, EnrollmentDate = DateTime.Now, Status = EnrollmentStatus.Approved },
                        new Enrollment { StudentId = 1, CourseId = 2, EnrollmentDate = DateTime.Now, Status = EnrollmentStatus.Approved }
                    );
                    SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Log error but don't throw
                Console.WriteLine($"Error seeding data: {ex.Message}");
            }
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin", Description = "Quản trị viên hệ thống", IsActive = true },
                new Role { RoleId = 2, RoleName = "Faculty", Description = "Giảng viên", IsActive = true },
                new Role { RoleId = 3, RoleName = "Student", Description = "Sinh viên", IsActive = true }
            );

            // Seed Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    Password = "admin123",
                    Email = "admin@sims.edu",
                    FullName = "Administrator",
                    RoleId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new User
                {
                    UserId = 2,
                    Username = "faculty1",
                    Password = "faculty123",
                    Email = "faculty1@sims.edu",
                    FullName = "Giảng viên 1",
                    RoleId = 2,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new User
                {
                    UserId = 3,
                    Username = "student1",
                    Password = "student123",
                    Email = "student1@sims.edu",
                    FullName = "Sinh viên 1",
                    RoleId = 3,
                    StudentId = 1,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );

            // Seed Students
            modelBuilder.Entity<Student>().HasData(
                new Student
                {
                    StudentId = 1,
                    StudentCode = "SV001",
                    FullName = "Nguyễn Văn A",
                    Email = "nguyenvana@sims.edu",
                    PhoneNumber = "0123456789",
                    DateOfBirth = new DateTime(2000, 1, 1),
                    Address = "Hà Nội",
                    Gender = "Nam",
                    Major = "Công nghệ thông tin",
                    AcademicYear = "2024-2025",
                    EnrollmentDate = DateTime.Now,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Student
                {
                    StudentId = 2,
                    StudentCode = "SV002",
                    FullName = "Trần Thị B",
                    Email = "tranthib@sims.edu",
                    PhoneNumber = "0987654321",
                    DateOfBirth = new DateTime(2001, 5, 15),
                    Address = "TP.HCM",
                    Gender = "Nữ",
                    Major = "Kinh tế",
                    AcademicYear = "2024-2025",
                    EnrollmentDate = DateTime.Now,
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );

            // Seed Courses
            modelBuilder.Entity<Course>().HasData(
                new Course
                {
                    CourseId = 1,
                    CourseCode = "CS101",
                    CourseName = "Lập trình Cơ bản",
                    Description = "Khóa học lập trình cơ bản cho sinh viên",
                    Credits = 3,
                    MaxStudents = 50,
                    Semester = "Học kỳ 1",
                    AcademicYear = "2024-2025",
                    Instructor = "TS. Nguyễn Văn Giảng",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Course
                {
                    CourseId = 2,
                    CourseCode = "CS201",
                    CourseName = "Lập trình Hướng đối tượng",
                    Description = "Khóa học lập trình hướng đối tượng",
                    Credits = 4,
                    MaxStudents = 40,
                    Semester = "Học kỳ 1",
                    AcademicYear = "2024-2025",
                    Instructor = "TS. Trần Thị Giảng",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                },
                new Course
                {
                    CourseId = 3,
                    CourseCode = "EC101",
                    CourseName = "Kinh tế học đại cương",
                    Description = "Khóa học kinh tế học cơ bản",
                    Credits = 3,
                    MaxStudents = 60,
                    Semester = "Học kỳ 1",
                    AcademicYear = "2024-2025",
                    Instructor = "TS. Lê Văn Kinh",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                }
            );
        }
    }
}