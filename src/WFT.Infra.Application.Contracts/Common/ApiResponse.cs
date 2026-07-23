using System;
using System.Collections.Generic;
using System.Text;

namespace WFT.Infra.Application.Contracts.Common
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public object? Meta { get; set; } // فیلد جدید برای اطلاعات Pagination

        public ApiResponse(bool isSuccess, int statusCode, string? message, T? data = default, object? meta = null)
        {
            IsSuccess = isSuccess;
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Meta = meta;
        }

        public static ApiResponse<T> Success(T data, string? message = "Success", int statusCode = 200, object? meta = null)
         => new ApiResponse<T>(true, statusCode, message, data, meta);

        public static ApiResponse<T> Failure(string? message, int statusCode = 400)
            => new ApiResponse<T>(false, statusCode, message, default);
    }
}
