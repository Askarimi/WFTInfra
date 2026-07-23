namespace WFT.Infra.Application.Contracts.Models
{
    /// <summary>
    /// Standardized API response envelope following JSON:API conventions
    /// Ensures consistent structure: { success, data, meta, message, error }
    /// Serialized to camelCase: { "success", "data", "meta", "message", "error" }
    /// </summary>
    /// <typeparam name="T">The type of data being returned</typeparam>
    public class ApiEnvelope<T>
    {
        /// <summary>
        /// Indicates if the request was successful
        /// Serialized as: "success"
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The response data (can be object, array, primitive, etc.)
        /// Serialized as: "data"
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Metadata (pagination info, warnings, etc.)
        /// Serialized as: "meta"
        /// </summary>
        public object? Meta { get; set; }

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
        public ApiEnvelope()
        {
        }

        /// <summary>
        /// Factory method for successful response
        /// </summary>
        /// <param name="data">The data to return</param>
        /// <param name="message">Optional success message</param>
        /// <param name="meta">Optional metadata (pagination, warnings, etc.)</param>
        /// <returns>Success envelope</returns>
        public static ApiEnvelope<T> Ok(T data, string? message = null, object? meta = null)
        {
            return new ApiEnvelope<T>
            {
                Success = true,
                Data = data,
                Meta = meta,
                Message = message,
                Error = null
            };
        }

        /// <summary>
        /// Factory method for error response
        /// </summary>
        /// <param name="error">Error message</param>
        /// <param name="message">Optional additional message</param>
        /// <param name="meta">Optional metadata</param>
        /// <returns>Error envelope</returns>
        public static ApiEnvelope<T> Fail(string error, string? message = null, object? meta = null)
        {
            return new ApiEnvelope<T>
            {
                Success = false,
                Data = default,
                Meta = meta,
                Message = message,
                Error = error
            };
        }
    }
}







