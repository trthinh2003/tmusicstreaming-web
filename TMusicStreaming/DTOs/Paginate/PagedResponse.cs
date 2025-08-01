namespace TMusicStreaming.DTOs.Paginate
{
    public class PagedResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public PaginationInfo Pagination { get; set; }
    }

    public class PaginationInfo
    {
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public int PerPage { get; set; }
        public int LastPage { get; set; }
    }

}
