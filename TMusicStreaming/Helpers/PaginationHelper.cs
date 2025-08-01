using TMusicStreaming.DTOs.Paginate;

namespace TMusicStreaming.Helpers
{
    public static class PaginationHelper
    {
        public static PagedResponse<T> CreatePagedResponse<T>(IEnumerable<T> source, int page, int pageSize) 
                                    //->Viết 1 helper để sau này tái sử dụng cho nhiều trang :))
        {
            var totalItems = source.Count();
            var lastPage = (int)Math.Ceiling((double)totalItems / pageSize);

            var data = source
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResponse<T>
            {
                Data = data,
                Pagination = new PaginationInfo
                {
                    TotalItems = totalItems,
                    CurrentPage = page,
                    PerPage = pageSize,
                    LastPage = lastPage
                }
            };
        }
    }

}
