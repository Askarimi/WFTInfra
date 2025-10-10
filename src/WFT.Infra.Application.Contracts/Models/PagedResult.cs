namespace WFT.Infra.Application.Contracts.Models
{
    /// <summary>
    /// Standardized pagination result model for API responses
    /// Matches the canonical contract expected by frontend HttpService.getPaged()
    /// </summary>
    /// <typeparam name="T">The type of items in the paged result</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// The collection of items for the current page
        /// </summary>
        public List<T> Data { get; set; } = new();

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number (1-based)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PagedResult()
        {
        }

        /// <summary>
        /// Constructor with all parameters
        /// </summary>
        public PagedResult(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        }

        /// <summary>
        /// Constructor from IEnumerable with automatic TotalPages calculation
        /// </summary>
        public PagedResult(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data?.ToList() ?? new List<T>();
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
        }
    }
}

