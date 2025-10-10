namespace WFT.Infra.Application.Contracts.Models
{
    /// <summary>
    /// Standardized paginated response following JSON:API conventions
    /// Will be serialized to camelCase format with flat data and meta sections
    /// </summary>
    /// <typeparam name="T">The type of items in the response</typeparam>
    public class PagedResponse<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// Serialized as: "success"
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The collection of items (flat array)
        /// Serialized as: "data"
        /// </summary>
        public IEnumerable<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Pagination metadata
        /// Serialized as: "meta"
        /// </summary>
        public Meta Meta { get; set; } = new Meta();

        /// <summary>
        /// Optional success/info message
        /// Serialized as: "message"
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Error message if request failed
        /// Serialized as: "error"
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PagedResponse()
        {
        }

        /// <summary>
        /// Constructor for successful response
        /// </summary>
        public PagedResponse(IEnumerable<T> data, int page, int pageSize, int totalItems, string? message = null)
        {
            Success = true;
            Data = data;
            Meta = new Meta
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0
            };
            Message = message;
            Error = null;
        }

        /// <summary>
        /// Static factory method for successful response
        /// </summary>
        public static PagedResponse<T> Ok(IEnumerable<T> data, int page, int pageSize, int totalItems, string? message = null)
        {
            return new PagedResponse<T>(data, page, pageSize, totalItems, message);
        }

        /// <summary>
        /// Static factory method for error response
        /// </summary>
        public static PagedResponse<T> Fail(string error, string? message = null)
        {
            return new PagedResponse<T>
            {
                Success = false,
                Data = new List<T>(),
                Meta = new Meta(),
                Message = message,
                Error = error
            };
        }
    }
}

