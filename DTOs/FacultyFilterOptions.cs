namespace Web_SIMS.DTOs
{
    public class FacultyFilterOptions
    {
        public string? SearchString { get; set; }
        public string? StatusFilter { get; set; }
        public string? DepartmentFilter { get; set; }
        public string? SortOrder { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}