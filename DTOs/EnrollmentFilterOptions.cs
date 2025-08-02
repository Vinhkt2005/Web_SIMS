namespace Web_SIMS.DTOs
{
    public class EnrollmentFilterOptions
    {
        public string? SearchString { get; set; }
        public string? StatusFilter { get; set; }
        public string? CourseFilter { get; set; }
        public string? StudentFilter { get; set; }
        public string? SortOrder { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}