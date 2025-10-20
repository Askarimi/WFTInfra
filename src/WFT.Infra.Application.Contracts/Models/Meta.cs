namespace WFT.Infra.Application.Contracts.Models
{
    /// <summary>
    /// Metadata for paginated responses following JSON:API conventions
    /// Will be serialized to camelCase: { "page": 1, "pageSize": 10, ... }
    /// </summary>
    public class Meta
    {
        /// <summary>
        /// Current page number (1-based)
        /// Serialized as: "page"
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Number of items per page
        /// Serialized as: "pageSize"
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// Serialized as: "totalItems"
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Total number of pages
        /// Serialized as: "totalPages"
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Optional warnings
        /// Serialized as: "warnings"
        /// </summary>
        public List<string>? Warnings { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Meta()
        {
        }

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        public Meta(int page, int pageSize, int totalItems, int totalPages, List<string>? warnings = null)
        {
            Page = page;
            PageSize = pageSize;
            TotalItems = totalItems;
            TotalPages = totalPages;
            Warnings = warnings;
        }
    }
}



