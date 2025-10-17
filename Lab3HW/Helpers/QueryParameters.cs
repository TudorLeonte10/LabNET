namespace Lab3.Helpers
{
    public class QueryParameters
    {
        public string? Author { get; set; }
        public string? SortBy { get; set; }
        public string SortDirection { get; set; } = "asc";
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
